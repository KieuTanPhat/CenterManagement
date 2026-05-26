using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CenterManagement.DataAccess.Data;
using CenterManagement.Models.DTOs;
using CenterManagement.Models.Enums;

namespace CenterManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public ReportController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var today = DateTime.Today;
            var activeClasses = await _context.Classes.CountAsync(c => c.Status == ClassStatus.Active);
            var totalStudents = await _context.Students.CountAsync();
            var totalTeachers = await _context.Teachers.CountAsync(t => t.User.IsActive);
            var monthlyRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate.Month == today.Month && p.PaymentDate.Year == today.Year)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var pendingLeave = await _context.LeaveRequests.CountAsync(lr => lr.Status == LeaveRequestStatus.Pending);
            var pendingConfirm = await _context.Classes.CountAsync(c => c.Status == ClassStatus.Upcoming &&
                c.StartDate <= DateOnly.FromDateTime(today.AddDays(7)));

            // Doanh thu 6 tháng gần nhất
            var revenueChart = new List<MonthlyRevenueDto>();
            for (int i = 5; i >= 0; i--)
            {
                var d = today.AddMonths(-i);
                var rev = await _context.Payments
                    .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate.Month == d.Month && p.PaymentDate.Year == d.Year)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;
                revenueChart.Add(new MonthlyRevenueDto { Year = d.Year, Month = d.Month, Revenue = rev });
            }

            // Cảnh báo học viên vắng 3 buổi liên tiếp
            var alerts = new List<ClassAlertDto>();
            var activeClassList = await _context.Classes
                .Where(c => c.Status == ClassStatus.Active)
                .Include(c => c.Enrollments)
                .Include(c => c.Schedules)
                .Take(50)
                .ToListAsync();

            foreach (var cls in activeClassList)
            {
                var recentScheduleIds = cls.Schedules
                    .OrderByDescending(s => s.ScheduleDate)
                    .Take(3)
                    .Select(s => s.Id)
                    .ToList();

                if (recentScheduleIds.Count < 3) continue;

                foreach (var enrollment in cls.Enrollments.Where(e => e.Status == EnrollmentStatus.Active))
                {
                    var consecutiveAbsent = await _context.Attendances
                        .CountAsync(a => a.StudentId == enrollment.StudentId &&
                            recentScheduleIds.Contains(a.ScheduleId) &&
                            a.Status == AttendanceStatus.Absent);
                    if (consecutiveAbsent >= 3)
                    {
                        alerts.Add(new ClassAlertDto
                        {
                            ClassId = cls.Id,
                            ClassName = cls.ClassName,
                            AlertType = "AbsenceAlert",
                            Message = $"Có học viên vắng 3 buổi liên tiếp trong lớp {cls.ClassName}"
                        });
                        break;
                    }
                }
            }

            return Ok(new DashboardDto
            {
                ActiveClasses = activeClasses,
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                MonthlyRevenue = monthlyRevenue,
                PendingLeaveRequests = pendingLeave,
                ClassesPendingConfirmation = pendingConfirm,
                RevenueChart = revenueChart,
                Alerts = alerts
            });
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue([FromQuery] int? month, [FromQuery] int? year)
        {
            var m = month ?? DateTime.Today.Month;
            var y = year ?? DateTime.Today.Year;

            var payments = await _context.Payments
                .Include(p => p.Enrollment).ThenInclude(e => e.Student)
                .Include(p => p.Enrollment).ThenInclude(e => e.Class)
                .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate.Month == m && p.PaymentDate.Year == y)
                .ToListAsync();

            var paymentDtos = payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                EnrollmentId = p.EnrollmentId,
                StudentName = p.Enrollment.Student.FullName,
                ClassName = p.Enrollment.Class.ClassName,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                Note = p.Note,
                PaymentType = p.Note != null && p.Note.Contains("đặt cọc") ? "Đặt cọc" : "Học phí"
            }).ToList();

            return Ok(new RevenueReportDto
            {
                Month = m,
                Year = y,
                TotalRevenue = paymentDtos.Sum(p => p.Amount),
                DepositRevenue = paymentDtos.Where(p => p.PaymentType == "Đặt cọc").Sum(p => p.Amount),
                RemainingRevenue = paymentDtos.Where(p => p.PaymentType == "Học phí").Sum(p => p.Amount),
                TotalEnrollments = paymentDtos.Select(p => p.EnrollmentId).Distinct().Count(),
                Details = paymentDtos
            });
        }

        [HttpGet("teacher-salary/{teacherId}/{classId}")]
        public async Task<IActionResult> GetTeacherSalary(int teacherId, int classId)
        {
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == teacherId);
            if (teacher == null) return NotFound();

            var cls = await _context.Classes.Include(c => c.Course).FirstOrDefaultAsync(c => c.Id == classId);
            if (cls == null) return NotFound();

            // Tổng học phí thu được từ lớp
            var totalTuition = await _context.Payments
                .Include(p => p.Enrollment)
                .Where(p => p.Enrollment.ClassId == classId && p.Status == PaymentStatus.Completed)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // Lương cơ bản: 30% tổng học phí
            var baseSalary = totalTuition * 0.3m;

            // Số buổi bù đã được duyệt
            var leaveRequests = await _context.LeaveRequests
                .Where(lr => lr.TeacherId == teacherId && lr.ClassId == classId && lr.Status == LeaveRequestStatus.Approved)
                .ToListAsync();

            var penaltyAmount = leaveRequests.Sum(lr => lr.PenaltyAmount);
            var hasExcessiveLeave = leaveRequests.Count > 3;
            var disciplinePenalty = hasExcessiveLeave ? baseSalary * 0.05m : 0;

            return Ok(new TeacherSalaryDto
            {
                TeacherId = teacherId,
                TeacherName = teacher.User.FullName,
                ClassId = classId,
                ClassName = cls.ClassName,
                TotalTuitionCollected = totalTuition,
                BaseSalary = baseSalary,
                PenaltySessionCount = leaveRequests.Count,
                PenaltyAmount = penaltyAmount,
                DisciplinePenalty = disciplinePenalty,
                FinalSalary = baseSalary - penaltyAmount - disciplinePenalty
            });
        }

        [HttpGet("class-attendance/{classId}")]
        public async Task<IActionResult> GetClassAttendance(int classId)
        {
            var attendanceController = new AttendanceController(_context);
            return await attendanceController.GetClassAttendanceSummary(classId);
        }
    }
}

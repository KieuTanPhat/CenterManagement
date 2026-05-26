using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CenterManagement.DataAccess.Data;
using CenterManagement.Models.DTOs;
using CenterManagement.Models.Entities;
using CenterManagement.Models.Enums;

namespace CenterManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public EnrollmentController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? classId, [FromQuery] int? studentId)
        {
            var query = _context.Enrollments
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .Include(e => e.Student)
                .Include(e => e.Payments)
                .AsQueryable();

            if (classId.HasValue) query = query.Where(e => e.ClassId == classId.Value);
            if (studentId.HasValue) query = query.Where(e => e.StudentId == studentId.Value);

            var list = await query.Select(e => new EnrollmentDto
            {
                Id = e.Id,
                ClassId = e.ClassId,
                ClassName = e.Class.ClassName,
                CourseName = e.Class.Course != null ? e.Class.Course.CourseName : string.Empty,
                StudentId = e.StudentId,
                StudentName = e.Student.FullName,
                StudentCode = e.Student.StudentCode ?? string.Empty,
                EnrollmentDate = e.EnrollmentDate,
                Status = e.Status,
                TotalPaid = e.Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
                TuitionFee = e.Class.Course != null ? (e.Class.Course.TuitionFee ?? 0) : 0
            }).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] CreateEnrollmentDto dto)
        {
            var cls = await _context.Classes.Include(c => c.Course).Include(c => c.Enrollments).Include(c => c.Schedules)
                .FirstOrDefaultAsync(c => c.Id == dto.ClassId);

            if (cls == null) return BadRequest(new { message = "Lớp học không tồn tại." });
            if (cls.Status != ClassStatus.Upcoming)
                return BadRequest(new { message = "Không thể đăng ký lớp học đã bắt đầu hoặc đã kết thúc." });

            // Kiểm tra đã đăng ký chưa
            if (await _context.Enrollments.AnyAsync(e => e.ClassId == dto.ClassId && e.StudentId == dto.StudentId))
                return BadRequest(new { message = "Học viên đã đăng ký lớp học này." });

            // Kiểm tra còn chỗ
            var activeCount = cls.Enrollments.Count(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending);
            if (activeCount >= cls.MaxStudents)
                return BadRequest(new { message = "Lớp học đã đầy." });

            // Kiểm tra trùng khóa học đang học
            var sameCourseActive = await _context.Enrollments
                .Include(e => e.Class)
                .AnyAsync(e => e.StudentId == dto.StudentId &&
                    e.Class.CourseId == cls.CourseId &&
                    (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending));
            if (sameCourseActive)
                return BadRequest(new { message = "Học viên đang học cùng khóa tại một lớp khác." });

            // Kiểm tra trùng ca (kiểm tra dựa trên schedules)
            var newClassSchedules = cls.Schedules.Select(s => new { s.DayOfWeek, s.TimeSlotId }).ToList();
            if (newClassSchedules.Any())
            {
                var studentActiveEnrollments = await _context.Enrollments
                    .Include(e => e.Class).ThenInclude(c => c.Schedules)
                    .Where(e => e.StudentId == dto.StudentId && (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending))
                    .ToListAsync();

                foreach (var existingEnrollment in studentActiveEnrollments)
                {
                    var conflictSchedule = existingEnrollment.Class.Schedules
                        .Any(s => newClassSchedules.Any(ns => ns.DayOfWeek == s.DayOfWeek && ns.TimeSlotId == s.TimeSlotId));
                    if (conflictSchedule)
                        return BadRequest(new { message = "Lịch học bị trùng với lớp đang học." });
                }
            }

            var enrollment = new Enrollment
            {
                ClassId = dto.ClassId,
                StudentId = dto.StudentId,
                EnrollmentDate = DateOnly.FromDateTime(DateTime.Today),
                Status = EnrollmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { classId = dto.ClassId }, new
            {
                enrollment.Id,
                Message = "Đăng ký thành công. Vui lòng đóng 20% đặt cọc để giữ chỗ.",
                DepositAmount = (cls.Course?.TuitionFee ?? 0) * 0.2m
            });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelEnrollmentDto dto)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Class).ThenInclude(c => c.Schedules)
                .Include(e => e.Payments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment == null) return NotFound();
            if (enrollment.Status == EnrollmentStatus.Dropped)
                return BadRequest(new { message = "Đăng ký đã bị hủy." });

            var totalPaid = enrollment.Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
            decimal refundAmount = 0;
            string refundMessage;

            // Tính hoàn tiền theo chính sách
            var attendedCount = await _context.Attendances
                .CountAsync(a => a.StudentId == enrollment.StudentId &&
                    enrollment.Class.Schedules.Select(s => s.Id).Contains(a.ScheduleId) &&
                    (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late));

            if (enrollment.Class.Status == ClassStatus.Upcoming)
            {
                // Hủy trước khai giảng: mất 20% đặt cọc
                var deposit = enrollment.Payments
                    .Where(p => p.Status == PaymentStatus.Completed && p.Note != null && p.Note.Contains("đặt cọc"))
                    .Sum(p => p.Amount);
                refundAmount = totalPaid - deposit;
                refundMessage = "Mất 20% đặt cọc. Hoàn lại phần còn lại.";
            }
            else if (attendedCount <= 3)
            {
                refundAmount = totalPaid * 0.5m;
                refundMessage = "Đã học ≤3 buổi. Hoàn 50% học phí đã đóng.";
            }
            else
            {
                refundAmount = 0;
                refundMessage = "Đã học >3 buổi. Không hoàn tiền.";
            }

            enrollment.Status = EnrollmentStatus.Dropped;
            enrollment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hủy đăng ký thành công.", refundMessage, refundAmount });
        }

        [HttpPost("{id}/transfer")]
        public async Task<IActionResult> Transfer(int id, [FromBody] TransferEnrollmentDto dto)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Class)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment == null) return NotFound();

            // Chỉ được chuyển trong 2 tuần đầu
            var twoWeeksAfterStart = enrollment.Class.StartDate.AddDays(14);
            if (DateOnly.FromDateTime(DateTime.Today) > twoWeeksAfterStart)
                return BadRequest(new { message = "Chỉ được chuyển lớp trong 2 tuần đầu của khóa học." });

            var toClass = await _context.Classes
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == dto.ToClassId);

            if (toClass == null) return BadRequest(new { message = "Lớp đích không tồn tại." });
            if (toClass.CourseId != enrollment.Class.CourseId)
                return BadRequest(new { message = "Lớp đích phải cùng khóa học." });

            var activeCountTo = toClass.Enrollments.Count(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending);
            if (activeCountTo >= toClass.MaxStudents)
                return BadRequest(new { message = "Lớp đích đã đầy." });

            // Kiểm tra chênh lệch không quá 2 buổi
            var fromScheduleCount = await _context.Schedules.CountAsync(s => s.ClassId == enrollment.ClassId);
            var toScheduleCount = await _context.Schedules.CountAsync(s => s.ClassId == dto.ToClassId);
            if (Math.Abs(fromScheduleCount - toScheduleCount) > 2)
                return BadRequest(new { message = "Lớp đích chênh lệch quá 2 buổi so với lớp cũ." });

            // Tạo ClassTransfer record
            _context.ClassTransfers.Add(new ClassTransfer
            {
                StudentId = enrollment.StudentId,
                FromClassId = enrollment.ClassId,
                ToClassId = dto.ToClassId,
                TransferDate = DateOnly.FromDateTime(DateTime.Today),
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            });

            // Hủy enrollment cũ, tạo enrollment mới
            enrollment.Status = EnrollmentStatus.Transferred;
            enrollment.UpdatedAt = DateTime.UtcNow;

            _context.Enrollments.Add(new Enrollment
            {
                ClassId = dto.ToClassId,
                StudentId = enrollment.StudentId,
                EnrollmentDate = DateOnly.FromDateTime(DateTime.Today),
                Status = EnrollmentStatus.Active,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Chuyển lớp thành công. Phí chuyển lớp: 200.000 VNĐ." });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CenterManagement.DataAccess.Data;
using CenterManagement.Models.DTOs;
using CenterManagement.Models.Entities;
using CenterManagement.Models.Enums;
using System.Security.Claims;

namespace CenterManagement.API.Controllers
{
    [ApiController]
    [Route("api/leave-requests")]
    [Authorize]
    public class LeaveRequestController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public LeaveRequestController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? teacherId, [FromQuery] int? classId, [FromQuery] LeaveRequestStatus? status)
        {
            var query = _context.LeaveRequests
                .Include(lr => lr.Teacher).ThenInclude(t => t.User)
                .Include(lr => lr.Class)
                .Include(lr => lr.ApprovedBy)
                .AsQueryable();

            if (teacherId.HasValue) query = query.Where(lr => lr.TeacherId == teacherId.Value);
            if (classId.HasValue) query = query.Where(lr => lr.ClassId == classId.Value);
            if (status.HasValue) query = query.Where(lr => lr.Status == status.Value);

            var list = await query.OrderByDescending(lr => lr.CreatedAt).Select(lr => new LeaveRequestDto
            {
                Id = lr.Id,
                TeacherId = lr.TeacherId,
                TeacherName = lr.Teacher.User.FullName,
                ClassId = lr.ClassId,
                ClassName = lr.Class.ClassName,
                ScheduleId = lr.ScheduleId,
                RequestDate = lr.RequestDate,
                LeaveDate = lr.LeaveDate,
                Reason = lr.Reason,
                Status = lr.Status,
                IsAbrupt = lr.IsAbrupt,
                PenaltyAmount = lr.PenaltyAmount,
                ApprovedByName = lr.ApprovedBy != null ? lr.ApprovedBy.FullName : null
            }).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null) return BadRequest(new { message = "Tài khoản không phải giáo viên." });

            // Kiểm tra nghỉ đột xuất: < 3 ngày làm việc trước ngày nghỉ
            var today = DateOnly.FromDateTime(DateTime.Today);
            var daysUntilLeave = (dto.LeaveDate.ToDateTime(TimeOnly.MinValue) - DateTime.Today).TotalDays;
            var isAbrupt = daysUntilLeave < 3;

            var penaltyAmount = isAbrupt ? 600_000m : 300_000m;

            // Kiểm tra số buổi bù đã quá 3 chưa
            var approvedCount = await _context.LeaveRequests
                .CountAsync(lr => lr.TeacherId == teacher.Id && lr.ClassId == dto.ClassId && lr.Status == LeaveRequestStatus.Approved);

            if (approvedCount >= 3)
                penaltyAmount += 0; // sẽ được xét thêm khi duyệt

            var leaveRequest = new LeaveRequest
            {
                TeacherId = teacher.Id,
                ClassId = dto.ClassId,
                ScheduleId = dto.ScheduleId,
                RequestDate = today,
                LeaveDate = dto.LeaveDate,
                Reason = dto.Reason,
                Status = LeaveRequestStatus.Pending,
                IsAbrupt = isAbrupt,
                PenaltyAmount = penaltyAmount,
                CreatedAt = DateTime.UtcNow
            };
            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                leaveRequest.Id,
                IsAbrupt = isAbrupt,
                PenaltyAmount = penaltyAmount,
                Message = isAbrupt
                    ? "Đơn xin nghỉ đột xuất (< 3 ngày). Phạt 600.000 VNĐ."
                    : "Đơn xin nghỉ đã gửi. Phạt 300.000 VNĐ/buổi bù.",
                ApprovedCount = approvedCount
            });
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveLeaveDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null) return NotFound();
            if (leaveRequest.Status != LeaveRequestStatus.Pending)
                return BadRequest(new { message = "Đơn đã được xử lý." });

            leaveRequest.Status = dto.Approve ? LeaveRequestStatus.Approved : LeaveRequestStatus.Rejected;
            leaveRequest.ApprovedByUserId = userId;
            leaveRequest.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = dto.Approve ? "Đã duyệt đơn xin nghỉ." : "Đã từ chối đơn xin nghỉ." });
        }
    }
}

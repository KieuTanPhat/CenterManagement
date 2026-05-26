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
    public class AttendanceController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public AttendanceController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBySchedule([FromQuery] int scheduleId)
        {
            var list = await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.ScheduleId == scheduleId)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    ScheduleId = a.ScheduleId,
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    StudentCode = a.Student.StudentCode ?? string.Empty,
                    Status = a.Status,
                    Note = a.Note
                }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("summary/{classId}")]
        public async Task<IActionResult> GetClassAttendanceSummary(int classId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.ClassId == classId && (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Completed))
                .ToListAsync();

            var scheduleIds = await _context.Schedules
                .Where(s => s.ClassId == classId)
                .Select(s => s.Id)
                .ToListAsync();

            var totalSessions = scheduleIds.Count;
            var result = new List<AttendanceSummaryDto>();

            foreach (var e in enrollments)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == e.StudentId && scheduleIds.Contains(a.ScheduleId))
                    .ToListAsync();

                var presentCount = attendances.Count(a => a.Status == AttendanceStatus.Present);
                var lateCount = attendances.Count(a => a.Status == AttendanceStatus.Late);
                var absentCount = attendances.Count(a => a.Status == AttendanceStatus.Absent);
                var excusedCount = attendances.Count(a => a.Status == AttendanceStatus.Excused);

                // Tính tỷ lệ chuyên cần: đến trễ tính 0.5 buổi
                var effectiveSessions = presentCount + lateCount * 0.5;
                var rate = totalSessions > 0 ? effectiveSessions / totalSessions : 0;

                result.Add(new AttendanceSummaryDto
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student.FullName,
                    TotalSessions = totalSessions,
                    PresentCount = presentCount,
                    AbsentCount = absentCount,
                    LateCount = lateCount,
                    ExcusedCount = excusedCount,
                    AttendanceRate = rate,
                    EligibleForFreeRetake = rate >= 0.9
                });
            }
            return Ok(result);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkMark([FromBody] BulkAttendanceDto dto)
        {
            var schedule = await _context.Schedules.FindAsync(dto.ScheduleId);
            if (schedule == null) return BadRequest(new { message = "Buổi học không tồn tại." });

            foreach (var item in dto.Attendances)
            {
                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.ScheduleId == dto.ScheduleId && a.StudentId == item.StudentId);

                if (existing != null)
                {
                    existing.Status = item.Status;
                    existing.Note = item.Note;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.Attendances.Add(new Attendance
                    {
                        ScheduleId = dto.ScheduleId,
                        StudentId = item.StudentId,
                        Status = item.Status,
                        Note = item.Note,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Điểm danh thành công.", count = dto.Attendances.Count });
        }
    }
}

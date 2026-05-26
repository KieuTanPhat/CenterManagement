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
    public class ExamController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public ExamController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetByClass([FromQuery] int classId)
        {
            var exams = await _context.Exams
                .Include(e => e.ExamResults)
                .Where(e => e.ClassId == classId)
                .Select(e => new ExamDto
                {
                    Id = e.Id,
                    ClassId = e.ClassId,
                    ClassName = e.Class.ClassName,
                    ExamName = e.ExamName,
                    ExamDate = e.ExamDate,
                    MaxScore = e.MaxScore,
                    Description = e.Description,
                    ResultCount = e.ExamResults.Count
                }).ToListAsync();
            return Ok(exams);
        }

        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResults(int id)
        {
            var exam = await _context.Exams.Include(e => e.Class).ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (exam == null) return NotFound();

            var results = await _context.ExamResults
                .Include(r => r.Student)
                .Where(r => r.ExamId == id)
                .ToListAsync();

            var scheduleIds = await _context.Schedules
                .Where(s => s.ClassId == exam.ClassId)
                .Select(s => s.Id).ToListAsync();

            var examResultDtos = new List<ExamResultDto>();
            foreach (var r in results)
            {
                var presentCount = await _context.Attendances
                    .CountAsync(a => a.StudentId == r.StudentId && scheduleIds.Contains(a.ScheduleId) &&
                        (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late));
                var lateCount = await _context.Attendances
                    .CountAsync(a => a.StudentId == r.StudentId && scheduleIds.Contains(a.ScheduleId) && a.Status == AttendanceStatus.Late);
                var totalSessions = scheduleIds.Count;
                var rate = totalSessions > 0 ? (presentCount + lateCount * 0.5) / totalSessions : 0;

                examResultDtos.Add(new ExamResultDto
                {
                    Id = r.Id,
                    ExamId = r.ExamId,
                    StudentId = r.StudentId,
                    StudentName = r.Student.FullName,
                    StudentCode = r.Student.StudentCode ?? string.Empty,
                    Score = r.Score,
                    MaxScore = exam.MaxScore,
                    Passed = r.Score >= exam.MaxScore * 0.6m,
                    EligibleForFreeRetake = r.Score < exam.MaxScore * 0.6m && rate >= 0.9,
                    Note = r.Note
                });
            }
            return Ok(examResultDtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExamDto dto)
        {
            var exam = new Exam
            {
                ClassId = dto.ClassId,
                ExamName = dto.ExamName,
                ExamDate = dto.ExamDate,
                MaxScore = dto.MaxScore,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByClass), new { classId = dto.ClassId }, new { exam.Id });
        }

        [HttpPost("{id}/results")]
        public async Task<IActionResult> SubmitResults(int id, [FromBody] BulkExamResultDto dto)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return NotFound();

            foreach (var item in dto.Results)
            {
                var existing = await _context.ExamResults
                    .FirstOrDefaultAsync(r => r.ExamId == id && r.StudentId == item.StudentId);
                if (existing != null)
                {
                    existing.Score = item.Score;
                    existing.Note = item.Note;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.ExamResults.Add(new ExamResult
                    {
                        ExamId = id,
                        StudentId = item.StudentId,
                        Score = item.Score,
                        Note = item.Note,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Nhập điểm thành công.", count = dto.Results.Count });
        }
    }
}

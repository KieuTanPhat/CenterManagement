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
    public class ClassController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public ClassController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ClassStatus? status, [FromQuery] int? courseId)
        {
            var query = _context.Classes
                .Include(c => c.Course)
                .Include(c => c.TeacherClassRegistrations).ThenInclude(t => t.Teacher).ThenInclude(t => t.User)
                .Include(c => c.Enrollments)
                .AsQueryable();

            if (status.HasValue) query = query.Where(c => c.Status == status.Value);
            if (courseId.HasValue) query = query.Where(c => c.CourseId == courseId.Value);

            var classes = await query.Select(c => new ClassDto
            {
                Id = c.Id,
                CourseId = c.CourseId,
                CourseName = c.Course != null ? c.Course.CourseName : string.Empty,
                ClassName = c.ClassName,
                MaxStudents = c.MaxStudents,
                EnrolledCount = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending),
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status,
                MainTeacherName = c.TeacherClassRegistrations
                    .Where(t => t.IsMainTeacher)
                    .Select(t => t.Teacher.User.FullName)
                    .FirstOrDefault(),
                MainTeacherId = c.TeacherClassRegistrations
                    .Where(t => t.IsMainTeacher)
                    .Select(t => (int?)t.TeacherId)
                    .FirstOrDefault()
            }).ToListAsync();
            return Ok(classes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _context.Classes
                .Include(x => x.Course)
                .Include(x => x.TeacherClassRegistrations).ThenInclude(t => t.Teacher).ThenInclude(t => t.User)
                .Include(x => x.Enrollments).ThenInclude(e => e.Student)
                .Include(x => x.Schedules).ThenInclude(s => s.Room).ThenInclude(r => r.Branch)
                .Include(x => x.Schedules).ThenInclude(s => s.TimeSlot)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null) return NotFound();

            return Ok(new ClassDetailDto
            {
                Id = c.Id,
                CourseId = c.CourseId,
                CourseName = c.Course?.CourseName ?? string.Empty,
                ClassName = c.ClassName,
                MaxStudents = c.MaxStudents,
                EnrolledCount = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending),
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status,
                MainTeacherName = c.TeacherClassRegistrations.Where(t => t.IsMainTeacher).Select(t => t.Teacher.User.FullName).FirstOrDefault(),
                MainTeacherId = c.TeacherClassRegistrations.Where(t => t.IsMainTeacher).Select(t => (int?)t.TeacherId).FirstOrDefault(),
                Schedules = c.Schedules.Select(s => new ScheduleDto
                {
                    Id = s.Id,
                    ClassId = s.ClassId,
                    ClassName = c.ClassName,
                    RoomId = s.RoomId,
                    RoomName = s.Room.RoomName,
                    BranchName = s.Room.Branch.BranchName,
                    TimeSlotId = s.TimeSlotId,
                    TimeSlotName = s.TimeSlot.SlotName,
                    StartTime = s.TimeSlot.StartTime.ToString("HH:mm"),
                    EndTime = s.TimeSlot.EndTime.ToString("HH:mm"),
                    DayOfWeek = (DayOfWeek)s.DayOfWeek,
                    ScheduleDate = s.ScheduleDate
                }).ToList(),
                Enrollments = c.Enrollments.Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    ClassId = e.ClassId,
                    ClassName = c.ClassName,
                    CourseName = c.Course?.CourseName ?? string.Empty,
                    StudentId = e.StudentId,
                    StudentName = e.Student.FullName,
                    StudentCode = e.Student.StudentCode ?? string.Empty,
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
        {
            // Phải mở lớp trước ít nhất 3 tháng
            if (dto.StartDate < DateOnly.FromDateTime(DateTime.Today.AddMonths(3)))
                return BadRequest(new { message = "Phải mở lớp trước ít nhất 3 tháng so với ngày khai giảng." });

            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null) return BadRequest(new { message = "Khóa học không tồn tại." });

            var cls = new Class
            {
                CourseId = dto.CourseId,
                ClassName = dto.ClassName,
                MaxStudents = dto.MaxStudents,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = ClassStatus.Upcoming,
                CreatedAt = DateTime.UtcNow
            };
            _context.Classes.Add(cls);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = cls.Id }, new { cls.Id, cls.ClassName });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClassDto dto)
        {
            var cls = await _context.Classes.FindAsync(id);
            if (cls == null) return NotFound();
            cls.ClassName = dto.ClassName;
            cls.MaxStudents = dto.MaxStudents;
            cls.StartDate = dto.StartDate;
            cls.EndDate = dto.EndDate;
            cls.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/confirm-start")]
        public async Task<IActionResult> ConfirmStart(int id)
        {
            var cls = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Enrollments)
                .Include(c => c.TeacherClassRegistrations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cls == null) return NotFound();
            if (cls.Status != ClassStatus.Upcoming)
                return BadRequest(new { message = "Lớp học không ở trạng thái chờ khai giảng." });

            var enrolledCount = cls.Enrollments.Count(e => e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending);
            var minRequired = cls.Course?.MinStudents > 0 ? cls.Course.MinStudents : (int)Math.Ceiling(cls.MaxStudents * 0.5);

            if (enrolledCount < minRequired)
                return BadRequest(new { message = $"Chưa đủ sĩ số. Hiện tại: {enrolledCount}/{minRequired} học viên tối thiểu." });

            if (!cls.TeacherClassRegistrations.Any())
                return BadRequest(new { message = "Chưa có giáo viên được gán cho lớp." });

            cls.Status = ClassStatus.Active;
            cls.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Khai giảng thành công." });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelClass(int id)
        {
            var cls = await _context.Classes
                .Include(c => c.Enrollments).ThenInclude(e => e.Payments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cls == null) return NotFound();
            if (cls.Status == ClassStatus.Completed || cls.Status == ClassStatus.Cancelled)
                return BadRequest(new { message = "Lớp học đã kết thúc hoặc đã hủy." });

            cls.Status = ClassStatus.Cancelled;
            cls.UpdatedAt = DateTime.UtcNow;

            // Cập nhật trạng thái đăng ký thành Dropped
            foreach (var enrollment in cls.Enrollments.Where(e => e.Status != EnrollmentStatus.Dropped))
            {
                enrollment.Status = EnrollmentStatus.Dropped;
                enrollment.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Hủy lớp thành công. Vui lòng xử lý hoàn tiền cho học viên." });
        }

        [HttpPost("{id}/assign-teacher")]
        public async Task<IActionResult> AssignTeacher(int id, [FromBody] AssignTeacherDto dto)
        {
            var cls = await _context.Classes.FindAsync(id);
            if (cls == null) return NotFound();

            var teacher = await _context.Teachers.FindAsync(dto.TeacherId);
            if (teacher == null) return BadRequest(new { message = "Giáo viên không tồn tại." });

            // Kiểm tra GV đã dạy lớp này chưa
            var existing = await _context.TeacherClassRegistrations
                .FirstOrDefaultAsync(t => t.TeacherId == dto.TeacherId && t.ClassId == id);
            if (existing != null) return BadRequest(new { message = "Giáo viên đã được gán vào lớp này." });

            // Kiểm tra GV dạy tối đa 3 lớp song song (cùng thời điểm)
            var activeClassCount = await _context.TeacherClassRegistrations
                .Include(t => t.Class)
                .CountAsync(t => t.TeacherId == dto.TeacherId &&
                    (t.Class.Status == ClassStatus.Active || t.Class.Status == ClassStatus.Upcoming));
            if (activeClassCount >= 3)
                return BadRequest(new { message = "Giáo viên đã đạt giới hạn 3 lớp song song." });

            // Nếu đây là giáo viên chính, bỏ cờ IsMainTeacher của GV cũ
            if (dto.IsMainTeacher)
            {
                var oldMain = await _context.TeacherClassRegistrations
                    .FirstOrDefaultAsync(t => t.ClassId == id && t.IsMainTeacher);
                if (oldMain != null)
                {
                    oldMain.IsMainTeacher = false;
                    oldMain.UpdatedAt = DateTime.UtcNow;
                }
            }

            _context.TeacherClassRegistrations.Add(new TeacherClassRegistration
            {
                TeacherId = dto.TeacherId,
                ClassId = id,
                IsMainTeacher = dto.IsMainTeacher,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return Ok(new { message = "Gán giáo viên thành công." });
        }
    }

    public class AssignTeacherDto
    {
        public int TeacherId { get; set; }
        public bool IsMainTeacher { get; set; } = true;
    }
}

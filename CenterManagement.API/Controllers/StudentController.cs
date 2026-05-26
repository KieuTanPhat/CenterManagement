using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CenterManagement.DataAccess.Data;
using CenterManagement.Models.DTOs;
using CenterManagement.Models.Entities;

namespace CenterManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public StudentController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var query = _context.Students.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.FullName.Contains(search) || (s.Phone != null && s.Phone.Contains(search)) || (s.StudentCode != null && s.StudentCode.Contains(search)));

            var students = await query.Select(s => new StudentDto
            {
                Id = s.Id,
                StudentCode = s.StudentCode ?? string.Empty,
                FullName = s.FullName,
                Phone = s.Phone,
                Email = s.Email,
                DateOfBirth = s.DateOfBirth,
                ParentName = s.ParentName,
                ParentPhone = s.ParentPhone,
                CreatedAt = s.CreatedAt
            }).ToListAsync();
            return Ok(students);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _context.Students.FindAsync(id);
            if (s == null) return NotFound();
            return Ok(new StudentDto
            {
                Id = s.Id,
                StudentCode = s.StudentCode ?? string.Empty,
                FullName = s.FullName,
                Phone = s.Phone,
                Email = s.Email,
                DateOfBirth = s.DateOfBirth,
                ParentName = s.ParentName,
                ParentPhone = s.ParentPhone,
                CreatedAt = s.CreatedAt
            });
        }

        [HttpGet("{id}/enrollments")]
        public async Task<IActionResult> GetEnrollments(int id)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .Include(e => e.Payments)
                .Where(e => e.StudentId == id)
                .Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    ClassId = e.ClassId,
                    ClassName = e.Class.ClassName,
                    CourseName = e.Class.Course != null ? e.Class.Course.CourseName : string.Empty,
                    StudentId = e.StudentId,
                    StudentName = string.Empty,
                    StudentCode = string.Empty,
                    EnrollmentDate = e.EnrollmentDate,
                    Status = e.Status,
                    TotalPaid = e.Payments.Where(p => p.Status == Models.Enums.PaymentStatus.Completed).Sum(p => p.Amount),
                    TuitionFee = e.Class.Course != null ? (e.Class.Course.TuitionFee ?? 0) : 0
                }).ToListAsync();
            return Ok(enrollments);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
        {
            // Tự động tạo mã học sinh
            var lastCode = await _context.Students
                .OrderByDescending(s => s.Id)
                .Select(s => s.StudentCode)
                .FirstOrDefaultAsync();
            var nextNumber = 1;
            if (lastCode != null && lastCode.StartsWith("HV") && int.TryParse(lastCode[2..], out var n))
                nextNumber = n + 1;
            var studentCode = $"HV{nextNumber:D4}";

            var student = new Student
            {
                StudentCode = studentCode,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Email = dto.Email,
                DateOfBirth = dto.DateOfBirth,
                ParentName = dto.ParentName,
                ParentPhone = dto.ParentPhone,
                CreatedAt = DateTime.UtcNow
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, new StudentDto
            {
                Id = student.Id,
                StudentCode = student.StudentCode ?? string.Empty,
                FullName = student.FullName
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            student.FullName = dto.FullName;
            student.Phone = dto.Phone;
            student.Email = dto.Email;
            student.DateOfBirth = dto.DateOfBirth;
            student.ParentName = dto.ParentName;
            student.ParentPhone = dto.ParentPhone;
            student.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

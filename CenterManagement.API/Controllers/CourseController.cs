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
    public class CourseController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public CourseController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly)
        {
            var query = _context.Courses.AsQueryable();
            if (activeOnly == true)
                query = query.Where(c => c.IsActive);

            var courses = await query.Select(c => new CourseDto
            {
                Id = c.Id,
                CourseName = c.CourseName,
                CourseCode = c.CourseCode,
                Description = c.Description,
                TuitionFee = c.TuitionFee,
                ExamFee = c.ExamFee,
                TargetScore = c.TargetScore,
                DurationWeeks = c.DurationWeeks,
                MinStudents = c.MinStudents,
                MaxStudents = c.MaxStudents,
                IsActive = c.IsActive
            }).ToListAsync();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            return Ok(new CourseDto
            {
                Id = course.Id,
                CourseName = course.CourseName,
                CourseCode = course.CourseCode,
                Description = course.Description,
                TuitionFee = course.TuitionFee,
                ExamFee = course.ExamFee,
                TargetScore = course.TargetScore,
                DurationWeeks = course.DurationWeeks,
                MinStudents = course.MinStudents,
                MaxStudents = course.MaxStudents,
                IsActive = course.IsActive
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
        {
            if (await _context.Courses.AnyAsync(c => c.CourseCode == dto.CourseCode && dto.CourseCode != null))
                return BadRequest(new { message = "Mã khóa học đã tồn tại." });

            var course = new Course
            {
                CourseName = dto.CourseName,
                CourseCode = dto.CourseCode,
                Description = dto.Description,
                TuitionFee = dto.TuitionFee,
                ExamFee = dto.ExamFee,
                TargetScore = dto.TargetScore,
                DurationWeeks = dto.DurationWeeks,
                MinStudents = dto.MinStudents,
                MaxStudents = dto.MaxStudents,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseDto dto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.CourseName = dto.CourseName;
            course.CourseCode = dto.CourseCode;
            course.Description = dto.Description;
            course.TuitionFee = dto.TuitionFee;
            course.ExamFee = dto.ExamFee;
            course.TargetScore = dto.TargetScore;
            course.DurationWeeks = dto.DurationWeeks;
            course.MinStudents = dto.MinStudents;
            course.MaxStudents = dto.MaxStudents;
            course.IsActive = dto.IsActive;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

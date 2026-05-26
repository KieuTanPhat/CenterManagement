using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CenterManagement.DataAccess.Data;
using CenterManagement.Models.DTOs;
using CenterManagement.Models.Entities;
using CenterManagement.Models.Enums;
using BCrypt.Net;

namespace CenterManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeacherController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public TeacherController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var query = _context.Teachers
                .Include(t => t.User)
                .Include(t => t.TeacherClassRegistrations).ThenInclude(r => r.Class)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.User.FullName.Contains(search) ||
                                         t.User.Email.Contains(search) ||
                                         (t.User.PhoneNumber != null && t.User.PhoneNumber.Contains(search)));

            var teachers = await query.Select(t => new TeacherDto
            {
                Id = t.Id,
                UserId = t.UserId,
                FullName = t.User.FullName,
                Email = t.User.Email,
                Phone = t.User.PhoneNumber,
                Specialization = t.Specialization,
                Qualification = t.Qualification,
                YearsOfExperience = t.YearsOfExperience,
                Certificates = t.Certificates,
                Biography = t.Biography,
                DateOfBirth = t.DateOfBirth,
                Gender = t.Gender,
                NationalId = t.NationalId,
                Address = t.Address,
                City = t.City,
                BankAccount = t.BankAccount,
                BankAccountName = t.BankAccountName,
                BankName = t.BankName,
                TaxId = t.TaxId,
                ContractType = t.ContractType,
                ContractStartDate = t.ContractStartDate,
                ContractEndDate = t.ContractEndDate,
                IsActive = t.User.IsActive,
                ActiveClassCount = t.TeacherClassRegistrations.Count(r =>
                    r.Class.Status == ClassStatus.Active || r.Class.Status == ClassStatus.Upcoming),
                CreatedAt = t.CreatedAt
            }).ToListAsync();
            return Ok(teachers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var t = await _context.Teachers
                .Include(x => x.User)
                .Include(x => x.TeacherClassRegistrations).ThenInclude(r => r.Class)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return NotFound();

            return Ok(new TeacherDto
            {
                Id = t.Id,
                UserId = t.UserId,
                FullName = t.User.FullName,
                Email = t.User.Email,
                Phone = t.User.PhoneNumber,
                Specialization = t.Specialization,
                Qualification = t.Qualification,
                YearsOfExperience = t.YearsOfExperience,
                Certificates = t.Certificates,
                Biography = t.Biography,
                DateOfBirth = t.DateOfBirth,
                Gender = t.Gender,
                NationalId = t.NationalId,
                Address = t.Address,
                City = t.City,
                BankAccount = t.BankAccount,
                BankAccountName = t.BankAccountName,
                BankName = t.BankName,
                TaxId = t.TaxId,
                ContractType = t.ContractType,
                ContractStartDate = t.ContractStartDate,
                ContractEndDate = t.ContractEndDate,
                IsActive = t.User.IsActive,
                ActiveClassCount = t.TeacherClassRegistrations.Count(r =>
                    r.Class.Status == ClassStatus.Active || r.Class.Status == ClassStatus.Upcoming),
                CreatedAt = t.CreatedAt
            });
        }

        [HttpGet("{id}/schedule")]
        public async Task<IActionResult> GetSchedule(int id)
        {
            var schedules = await _context.TeacherClassRegistrations
                .Include(r => r.Class).ThenInclude(c => c.Course)
                .Include(r => r.Class).ThenInclude(c => c.Schedules).ThenInclude(s => s.Room).ThenInclude(r => r.Branch)
                .Include(r => r.Class).ThenInclude(c => c.Schedules).ThenInclude(s => s.TimeSlot)
                .Where(r => r.TeacherId == id && (r.Class.Status == ClassStatus.Active || r.Class.Status == ClassStatus.Upcoming))
                .SelectMany(r => r.Class.Schedules.Select(s => new TeacherScheduleDto
                {
                    ClassId = r.ClassId,
                    ClassName = r.Class.ClassName,
                    CourseName = r.Class.Course != null ? r.Class.Course.CourseName : string.Empty,
                    RoomName = s.Room.RoomName,
                    BranchName = s.Room.Branch.BranchName,
                    TimeSlotName = s.TimeSlot.SlotName,
                    DayOfWeek = s.DayOfWeek.ToString(),
                    ScheduleDate = s.ScheduleDate
                }))
                .ToListAsync();
            return Ok(schedules);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == dto.Username))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email đã được sử dụng." });

            // Giáo viên không được là nhân viên/quản lý
            var user = new User
            {
                UserName = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                RoleId = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var teacher = new Teacher
            {
                UserId = user.Id,
                Specialization = dto.Specialization,
                Qualification = dto.Qualification,
                YearsOfExperience = dto.YearsOfExperience,
                Certificates = dto.Certificates,
                Biography = dto.Biography,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                NationalId = dto.NationalId,
                Address = dto.Address,
                City = dto.City,
                BankAccount = dto.BankAccount,
                BankAccountName = dto.BankAccountName,
                BankName = dto.BankName,
                TaxId = dto.TaxId,
                ContractType = dto.ContractType,
                ContractStartDate = dto.ContractStartDate,
                ContractEndDate = dto.ContractEndDate,
                CreatedAt = DateTime.UtcNow
            };
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, new { teacher.Id, user.FullName });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTeacherDto dto)
        {
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return NotFound();

            teacher.User.FullName = dto.FullName;
            teacher.User.PhoneNumber = dto.PhoneNumber;
            teacher.User.IsActive = dto.IsActive;
            teacher.Specialization = dto.Specialization ?? teacher.Specialization;
            teacher.Qualification = dto.Qualification;
            teacher.YearsOfExperience = dto.YearsOfExperience;
            teacher.Certificates = dto.Certificates;
            teacher.Biography = dto.Biography;
            teacher.DateOfBirth = dto.DateOfBirth;
            teacher.Gender = dto.Gender;
            teacher.NationalId = dto.NationalId;
            teacher.Address = dto.Address;
            teacher.City = dto.City;
            teacher.BankAccount = dto.BankAccount;
            teacher.BankAccountName = dto.BankAccountName;
            teacher.BankName = dto.BankName;
            teacher.TaxId = dto.TaxId;
            teacher.ContractType = dto.ContractType;
            teacher.ContractStartDate = dto.ContractStartDate;
            teacher.ContractEndDate = dto.ContractEndDate;
            teacher.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return NotFound();
            teacher.User.IsActive = !teacher.User.IsActive;
            teacher.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { teacher.User.IsActive });
        }
    }
}

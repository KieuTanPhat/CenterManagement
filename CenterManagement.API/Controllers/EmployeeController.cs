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
    public class EmployeeController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public EmployeeController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? branchId)
        {
            var query = _context.Employees
                .Include(em => em.User).ThenInclude(u => u.Role)
                .Include(em => em.Branch)
                .AsQueryable();

            if (branchId.HasValue)
                query = query.Where(em => em.BranchId == branchId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(em =>
                    em.User.FullName.Contains(search) ||
                    em.User.Email.Contains(search) ||
                    (em.User.PhoneNumber != null && em.User.PhoneNumber.Contains(search)) ||
                    (em.Position != null && em.Position.Contains(search)));

            var employees = await query.Select(em => new EmployeeDto
            {
                Id = em.Id,
                UserId = em.UserId,
                BranchId = em.BranchId,
                BranchName = em.Branch != null ? em.Branch.BranchName : null,
                FullName = em.User.FullName,
                Email = em.User.Email,
                Phone = em.User.PhoneNumber,
                Position = em.Position,
                Department = em.Department,
                ContractType = em.ContractType,
                HireDate = em.HireDate,
                ContractStartDate = em.ContractStartDate,
                ContractEndDate = em.ContractEndDate,
                Salary = em.Salary,
                DateOfBirth = em.DateOfBirth,
                Gender = em.Gender,
                NationalId = em.NationalId,
                Address = em.Address,
                City = em.City,
                Education = em.Education,
                Major = em.Major,
                EmergencyContact = em.EmergencyContact,
                EmergencyPhone = em.EmergencyPhone,
                EmergencyRelationship = em.EmergencyRelationship,
                BankAccount = em.BankAccount,
                BankAccountName = em.BankAccountName,
                BankName = em.BankName,
                TaxId = em.TaxId,
                IsActive = em.User.IsActive,
                RoleId = em.User.RoleId,
                RoleName = em.User.Role.RoleName,
                CreatedAt = em.CreatedAt
            }).ToListAsync();

            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var em = await _context.Employees
                .Include(e => e.User).ThenInclude(u => u.Role)
                .Include(e => e.Branch)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (em == null) return NotFound();

            return Ok(new EmployeeDto
            {
                Id = em.Id, UserId = em.UserId, BranchId = em.BranchId,
                BranchName = em.Branch?.BranchName,
                FullName = em.User.FullName, Email = em.User.Email, Phone = em.User.PhoneNumber,
                Position = em.Position, Department = em.Department, ContractType = em.ContractType,
                HireDate = em.HireDate, ContractStartDate = em.ContractStartDate,
                ContractEndDate = em.ContractEndDate, Salary = em.Salary,
                DateOfBirth = em.DateOfBirth, Gender = em.Gender, NationalId = em.NationalId,
                Address = em.Address, City = em.City, Education = em.Education, Major = em.Major,
                EmergencyContact = em.EmergencyContact, EmergencyPhone = em.EmergencyPhone,
                EmergencyRelationship = em.EmergencyRelationship,
                BankAccount = em.BankAccount, BankAccountName = em.BankAccountName,
                BankName = em.BankName, TaxId = em.TaxId,
                IsActive = em.User.IsActive, RoleId = em.User.RoleId, RoleName = em.User.Role.RoleName,
                CreatedAt = em.CreatedAt
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            if (dto.RoleId is 4)
                return BadRequest(new { message = "Nhân viên/Quản lý không thể có role Giáo viên." });
            if (await _context.Users.AnyAsync(u => u.UserName == dto.Username))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email đã được sử dụng." });

            var user = new User
            {
                UserName = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = dto.Email, FullName = dto.FullName, PhoneNumber = dto.PhoneNumber,
                RoleId = dto.RoleId, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var employee = new Employee
            {
                UserId = user.Id, BranchId = dto.BranchId, Position = dto.Position,
                Department = dto.Department, ContractType = dto.ContractType,
                HireDate = dto.HireDate, ContractStartDate = dto.ContractStartDate,
                ContractEndDate = dto.ContractEndDate, Salary = dto.Salary,
                DateOfBirth = dto.DateOfBirth, Gender = dto.Gender, NationalId = dto.NationalId,
                Address = dto.Address, City = dto.City, Education = dto.Education, Major = dto.Major,
                EmergencyContact = dto.EmergencyContact, EmergencyPhone = dto.EmergencyPhone,
                EmergencyRelationship = dto.EmergencyRelationship,
                BankAccount = dto.BankAccount, BankAccountName = dto.BankAccountName,
                BankName = dto.BankName, TaxId = dto.TaxId, CreatedAt = DateTime.UtcNow
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, new { employee.Id, user.FullName });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var employee = await _context.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();

            employee.User.FullName = dto.FullName;
            employee.User.PhoneNumber = dto.PhoneNumber;
            employee.User.IsActive = dto.IsActive;
            employee.BranchId = dto.BranchId;
            employee.Position = dto.Position;
            employee.Department = dto.Department;
            employee.ContractType = dto.ContractType;
            employee.HireDate = dto.HireDate;
            employee.ContractStartDate = dto.ContractStartDate;
            employee.ContractEndDate = dto.ContractEndDate;
            employee.Salary = dto.Salary;
            employee.DateOfBirth = dto.DateOfBirth;
            employee.Gender = dto.Gender;
            employee.NationalId = dto.NationalId;
            employee.Address = dto.Address;
            employee.City = dto.City;
            employee.Education = dto.Education;
            employee.Major = dto.Major;
            employee.EmergencyContact = dto.EmergencyContact;
            employee.EmergencyPhone = dto.EmergencyPhone;
            employee.EmergencyRelationship = dto.EmergencyRelationship;
            employee.BankAccount = dto.BankAccount;
            employee.BankAccountName = dto.BankAccountName;
            employee.BankName = dto.BankName;
            employee.TaxId = dto.TaxId;
            employee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var employee = await _context.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();
            employee.User.IsActive = !employee.User.IsActive;
            employee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { employee.User.IsActive });
        }
    }
}

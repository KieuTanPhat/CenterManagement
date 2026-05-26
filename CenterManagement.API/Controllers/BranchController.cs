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
    public class BranchController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public BranchController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var branches = await _context.Branches
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    BranchName = b.BranchName,
                    City = b.City,
                    Address = b.Address,
                    Phone = b.Phone,
                    IsActive = b.IsActive
                }).ToListAsync();
            return Ok(branches);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound();
            return Ok(new BranchDto
            {
                Id = branch.Id,
                BranchName = branch.BranchName,
                City = branch.City,
                Address = branch.Address,
                Phone = branch.Phone,
                IsActive = branch.IsActive
            });
        }

        [HttpGet("{id}/rooms")]
        public async Task<IActionResult> GetRooms(int id)
        {
            var rooms = await _context.Rooms
                .Where(r => r.BranchId == id)
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    BranchId = r.BranchId,
                    BranchName = r.Branch.BranchName,
                    RoomName = r.RoomName,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity,
                    IsActive = r.IsActive
                }).ToListAsync();
            return Ok(rooms);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBranchDto dto)
        {
            var branch = new Branch
            {
                BranchName = dto.BranchName,
                City = dto.City,
                Address = dto.Address,
                Phone = dto.Phone,
                IsActive = true
            };
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = branch.Id }, new BranchDto
            {
                Id = branch.Id,
                BranchName = branch.BranchName,
                City = branch.City,
                Address = branch.Address,
                Phone = branch.Phone,
                IsActive = branch.IsActive
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchDto dto)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound();
            branch.BranchName = dto.BranchName;
            branch.City = dto.City;
            branch.Address = dto.Address;
            branch.Phone = dto.Phone;
            branch.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

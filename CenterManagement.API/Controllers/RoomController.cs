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
    public class RoomController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public RoomController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? branchId)
        {
            var query = _context.Rooms.Include(r => r.Branch).AsQueryable();
            if (branchId.HasValue)
                query = query.Where(r => r.BranchId == branchId.Value);

            var rooms = await query.Select(r => new RoomDto
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _context.Rooms.Include(r => r.Branch).FirstOrDefaultAsync(r => r.Id == id);
            if (room == null) return NotFound();
            return Ok(new RoomDto
            {
                Id = room.Id,
                BranchId = room.BranchId,
                BranchName = room.Branch.BranchName,
                RoomName = room.RoomName,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                IsActive = room.IsActive
            });
        }

        [HttpGet("{id}/availability")]
        public async Task<IActionResult> CheckAvailability(int id, [FromQuery] DateOnly date, [FromQuery] int timeSlotId)
        {
            var conflict = await _context.Schedules.AnyAsync(s =>
                s.RoomId == id && s.ScheduleDate == date && s.TimeSlotId == timeSlotId);
            return Ok(new { IsAvailable = !conflict });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
        {
            var room = new Room
            {
                BranchId = dto.BranchId,
                RoomName = dto.RoomName,
                RoomType = dto.RoomType,
                Capacity = dto.Capacity,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            room.RoomName = dto.RoomName;
            room.RoomType = dto.RoomType;
            room.Capacity = dto.Capacity;
            room.IsActive = dto.IsActive;
            room.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

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
    public class ScheduleController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public ScheduleController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? classId, [FromQuery] int? roomId, [FromQuery] DateOnly? date)
        {
            var query = _context.Schedules
                .Include(s => s.Class)
                .Include(s => s.Room).ThenInclude(r => r.Branch)
                .Include(s => s.TimeSlot)
                .AsQueryable();

            if (classId.HasValue) query = query.Where(s => s.ClassId == classId.Value);
            if (roomId.HasValue) query = query.Where(s => s.RoomId == roomId.Value);
            if (date.HasValue) query = query.Where(s => s.ScheduleDate == date.Value);

            var list = await query.Select(s => new ScheduleDto
            {
                Id = s.Id,
                ClassId = s.ClassId,
                ClassName = s.Class.ClassName,
                RoomId = s.RoomId,
                RoomName = s.Room.RoomName,
                BranchName = s.Room.Branch.BranchName,
                TimeSlotId = s.TimeSlotId,
                TimeSlotName = s.TimeSlot.SlotName,
                StartTime = s.TimeSlot.StartTime.ToString("HH:mm"),
                EndTime = s.TimeSlot.EndTime.ToString("HH:mm"),
                DayOfWeek = (DayOfWeek)s.DayOfWeek,
                ScheduleDate = s.ScheduleDate
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("timeslots")]
        public async Task<IActionResult> GetTimeSlots()
        {
            var slots = await _context.TimeSlots.Select(ts => new TimeSlotDto
            {
                Id = ts.Id,
                SlotName = ts.SlotName,
                StartTime = ts.StartTime,
                EndTime = ts.EndTime
            }).ToListAsync();
            return Ok(slots);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateScheduleDto dto)
        {
            // Kiểm tra xung đột phòng: không xếp 2 lớp cùng phòng + ca + ngày
            var conflict = await _context.Schedules.AnyAsync(s =>
                s.RoomId == dto.RoomId &&
                s.TimeSlotId == dto.TimeSlotId &&
                s.ScheduleDate == dto.ScheduleDate);

            if (conflict)
                return BadRequest(new { message = "Phòng học đã được sử dụng vào ca học và ngày này." });

            var schedule = new Schedule
            {
                ClassId = dto.ClassId,
                RoomId = dto.RoomId,
                TimeSlotId = dto.TimeSlotId,
                DayOfWeek = dto.DayOfWeek,
                ScheduleDate = dto.ScheduleDate,
                CreatedAt = DateTime.UtcNow
            };
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { classId = dto.ClassId }, new { schedule.Id });
        }
    }
}

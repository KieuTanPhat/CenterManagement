using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CenterManagement.DataAccess.Data;
using CenterManagement.Models.DTOs;
using CenterManagement.Models.Entities;

namespace CenterManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public NotificationController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? targetUserId)
        {
            var query = _context.Notifications
                .Include(n => n.CreatedBy)
                .Include(n => n.TargetUser)
                .AsQueryable();

            if (targetUserId.HasValue)
                query = query.Where(n => n.TargetUserId == null || n.TargetUserId == targetUserId);

            var list = await query.OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    NotificationType = n.NotificationType,
                    IsRead = n.IsRead,
                    TargetUserId = n.TargetUserId,
                    TargetUserName = n.TargetUser != null ? n.TargetUser.FullName : "Tất cả",
                    CreatedByUserId = n.CreatedByUserId,
                    CreatedByName = n.CreatedBy.FullName,
                    CreatedAt = n.CreatedAt
                }).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var notification = new Notification
            {
                Title = dto.Title,
                Message = dto.Message,
                NotificationType = dto.NotificationType ?? "System",
                TargetUserId = dto.TargetUserId,
                CreatedByUserId = userId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return Ok(new { notification.Id });
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var notif = await _context.Notifications.FindAsync(id);
            if (notif == null) return NotFound();
            notif.IsRead = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

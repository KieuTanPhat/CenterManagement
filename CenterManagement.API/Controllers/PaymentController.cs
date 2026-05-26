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
    public class PaymentController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public PaymentController(CenterManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? enrollmentId, [FromQuery] PaymentStatus? status)
        {
            var query = _context.Payments
                .Include(p => p.Enrollment).ThenInclude(e => e.Student)
                .Include(p => p.Enrollment).ThenInclude(e => e.Class)
                .AsQueryable();

            if (enrollmentId.HasValue) query = query.Where(p => p.EnrollmentId == enrollmentId.Value);
            if (status.HasValue) query = query.Where(p => p.Status == status.Value);

            var list = await query.OrderByDescending(p => p.PaymentDate).Select(p => new PaymentDto
            {
                Id = p.Id,
                EnrollmentId = p.EnrollmentId,
                StudentName = p.Enrollment.Student.FullName,
                ClassName = p.Enrollment.Class.ClassName,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                Note = p.Note,
                PaymentType = p.Note != null && p.Note.Contains("đặt cọc") ? "Đặt cọc" : "Học phí"
            }).ToListAsync();
            return Ok(list);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int? month, [FromQuery] int? year)
        {
            var query = _context.Payments.Where(p => p.Status == PaymentStatus.Completed).AsQueryable();
            if (month.HasValue) query = query.Where(p => p.PaymentDate.Month == month.Value);
            if (year.HasValue) query = query.Where(p => p.PaymentDate.Year == year.Value);

            var total = await query.SumAsync(p => p.Amount);
            var deposit = await query.Where(p => p.Note != null && p.Note.Contains("đặt cọc")).SumAsync(p => p.Amount);
            var count = await query.CountAsync();
            return Ok(new PaymentSummaryDto
            {
                TotalRevenue = total,
                TotalDeposit = deposit,
                TotalRemaining = total - deposit,
                TotalTransactions = count
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Class).ThenInclude(c => c.Course)
                .Include(e => e.Payments)
                .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId);

            if (enrollment == null) return BadRequest(new { message = "Đăng ký không tồn tại." });

            var tuitionFee = enrollment.Class.Course?.TuitionFee ?? 0;
            var totalPaid = enrollment.Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
            var remaining = tuitionFee - totalPaid;

            if (remaining <= 0)
                return BadRequest(new { message = "Học viên đã đóng đủ học phí." });
            if (dto.Amount > remaining)
                return BadRequest(new { message = $"Số tiền vượt quá số tiền còn lại ({remaining:N0} VNĐ)." });

            var isDeposit = dto.PaymentType == "Deposit" || (totalPaid == 0 && dto.Amount <= tuitionFee * 0.2m);
            var payment = new Payment
            {
                EnrollmentId = dto.EnrollmentId,
                Amount = dto.Amount,
                PaymentDate = DateOnly.FromDateTime(DateTime.Today),
                PaymentMethod = dto.PaymentMethod,
                Status = PaymentStatus.Completed,
                Note = isDeposit ? $"Tiền đặt cọc 20% - {dto.Note}" : $"Học phí - {dto.Note}",
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);

            // Khi đóng đủ tiền, cập nhật enrollment thành Active
            if (totalPaid + dto.Amount >= tuitionFee && enrollment.Status == EnrollmentStatus.Pending)
            {
                enrollment.Status = EnrollmentStatus.Active;
                enrollment.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { enrollmentId = dto.EnrollmentId }, new
            {
                payment.Id,
                payment.Amount,
                Remaining = remaining - dto.Amount,
                Message = "Thanh toán thành công."
            });
        }
    }
}

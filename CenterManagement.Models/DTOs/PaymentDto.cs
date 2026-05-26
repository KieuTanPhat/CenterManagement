using CenterManagement.Models.Enums;

namespace CenterManagement.Models.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateOnly PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public string? Note { get; set; }
        public string PaymentType { get; set; } = string.Empty;
    }

    public class CreatePaymentDto
    {
        public int EnrollmentId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? Note { get; set; }
        public string PaymentType { get; set; } = "Deposit";
    }

    public class PaymentSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalRemaining { get; set; }
        public int TotalTransactions { get; set; }
        public List<PaymentDto> Payments { get; set; } = new();
    }
}

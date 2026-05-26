using CenterManagement.Models.Enums;
using System;

namespace CenterManagement.Models.Entities
{
    public class Payment : BaseEntity
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? Note { get; set; }

        public Enrollment Enrollment { get; set; } = null!;
    }
}

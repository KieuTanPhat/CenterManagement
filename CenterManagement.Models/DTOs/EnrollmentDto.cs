using CenterManagement.Models.Enums;

namespace CenterManagement.Models.DTOs
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public DateOnly EnrollmentDate { get; set; }
        public EnrollmentStatus Status { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TuitionFee { get; set; }
    }

    public class CreateEnrollmentDto
    {
        public int ClassId { get; set; }
        public int StudentId { get; set; }
    }

    public class TransferEnrollmentDto
    {
        public int ToClassId { get; set; }
        public string? Reason { get; set; }
    }

    public class CancelEnrollmentDto
    {
        public string? Reason { get; set; }
    }
}

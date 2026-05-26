using System;

namespace CenterManagement.Models.Entities
{
    public class ExamResult : BaseEntity
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public decimal Score { get; set; }
        public string? Note { get; set; }

        public Exam Exam { get; set; } = null!;
        public Student Student { get; set; } = null!;
    }
}

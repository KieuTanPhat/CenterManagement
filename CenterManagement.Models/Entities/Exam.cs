using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class Exam : BaseEntity
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public decimal MaxScore { get; set; }
        public string? Description { get; set; }

        public Class Class { get; set; } = null!;
        public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    }
}

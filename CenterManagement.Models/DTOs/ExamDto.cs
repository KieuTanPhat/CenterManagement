namespace CenterManagement.Models.DTOs
{
    public class ExamDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string ExamName { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public decimal MaxScore { get; set; }
        public string? Description { get; set; }
        public int ResultCount { get; set; }
    }

    public class CreateExamDto
    {
        public int ClassId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public decimal MaxScore { get; set; }
        public string? Description { get; set; }
    }

    public class ExamResultDto
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public bool Passed { get; set; }
        public bool EligibleForFreeRetake { get; set; }
        public string? Note { get; set; }
    }

    public class BulkExamResultDto
    {
        public int ExamId { get; set; }
        public List<ExamResultItemDto> Results { get; set; } = new();
    }

    public class ExamResultItemDto
    {
        public int StudentId { get; set; }
        public decimal Score { get; set; }
        public string? Note { get; set; }
    }
}

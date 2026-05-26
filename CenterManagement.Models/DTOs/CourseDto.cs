namespace CenterManagement.Models.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string? Description { get; set; }
        public decimal? TuitionFee { get; set; }
        public decimal? ExamFee { get; set; }
        public string? TargetScore { get; set; }
        public int? DurationWeeks { get; set; }
        public int MinStudents { get; set; }
        public int MaxStudents { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCourseDto
    {
        public string CourseName { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string? Description { get; set; }
        public decimal? TuitionFee { get; set; }
        public decimal? ExamFee { get; set; }
        public string? TargetScore { get; set; }
        public int? DurationWeeks { get; set; }
        public int MinStudents { get; set; } = 5;
        public int MaxStudents { get; set; } = 30;
    }

    public class UpdateCourseDto : CreateCourseDto
    {
        public bool IsActive { get; set; } = true;
    }
}

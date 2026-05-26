using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class Course : BaseEntity
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string? Description { get; set; }
        public decimal? TuitionFee { get; set; }
        public int? DurationWeeks { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal? ExamFee { get; set; }
        public string? TargetScore { get; set; }
        public int MinStudents { get; set; } = 0;
        public int MaxStudents { get; set; } = 30;

        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}

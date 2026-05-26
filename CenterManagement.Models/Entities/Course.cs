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

        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}

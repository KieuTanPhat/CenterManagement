using CenterManagement.Models.Enums;

namespace CenterManagement.Models.DTOs
{
    public class ClassDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int MaxStudents { get; set; }
        public int EnrolledCount { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public ClassStatus Status { get; set; }
        public string? MainTeacherName { get; set; }
        public int? MainTeacherId { get; set; }
    }

    public class ClassDetailDto : ClassDto
    {
        public List<ScheduleDto> Schedules { get; set; } = new();
        public List<EnrollmentDto> Enrollments { get; set; } = new();
    }

    public class CreateClassDto
    {
        public int CourseId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int MaxStudents { get; set; } = 30;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

    public class UpdateClassDto
    {
        public string ClassName { get; set; } = string.Empty;
        public int MaxStudents { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public ClassStatus Status { get; set; }
    }
}

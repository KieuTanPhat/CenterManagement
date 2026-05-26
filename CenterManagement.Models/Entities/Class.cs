using CenterManagement.Models.Enums;
using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class Class : BaseEntity
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int MaxStudents { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public ClassStatus Status { get; set; } = ClassStatus.Upcoming;

        public Course? Course { get; set; }
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<TeacherClassRegistration> TeacherClassRegistrations { get; set; } = new List<TeacherClassRegistration>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<ClassTransfer> ClassTransfers { get; set; } = new List<ClassTransfer>();
        public ICollection<Exam> Exams { get; set; } = new List<Exam>();
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    }
}

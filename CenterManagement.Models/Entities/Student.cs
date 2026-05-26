using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class Student : BaseEntity
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<ClassTransfer> ClassTransfers { get; set; } = new List<ClassTransfer>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    }
}

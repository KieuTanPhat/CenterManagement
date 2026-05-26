using CenterManagement.Models.Enums;
using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class Enrollment : BaseEntity
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int StudentId { get; set; }
        public DateOnly EnrollmentDate { get; set; }
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

        public Class? Class { get; set; }
        public Student? Student { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}

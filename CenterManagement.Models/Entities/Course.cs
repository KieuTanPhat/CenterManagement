using System;
using System.Collections.Generic;

namespace CenterManagement.DataAccess;

public partial class Course
{
    public int Id { get; set; }

    public string CourseName { get; set; } = null!;

    public string Level { get; set; } = null!;

    public int DurationWeeks { get; set; }

    public decimal Fee { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}

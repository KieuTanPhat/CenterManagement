using System;
using System.Collections.Generic;

namespace CenterManagement.DataAccess;

public partial class Student
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public int? LeadId { get; set; }

    public int BranchId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual Lead? Lead { get; set; }

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}

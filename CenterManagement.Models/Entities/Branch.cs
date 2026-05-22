using System;
using System.Collections.Generic;

namespace CenterManagement.DataAccess;

public partial class Branch
{
    public int Id { get; set; }

    public string BranchName { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

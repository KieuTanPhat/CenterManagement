using System;
using System.Collections.Generic;

namespace CenterManagement.DataAccess;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int RoleId { get; set; }

    public int? BranchId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual Role Role { get; set; } = null!;
}

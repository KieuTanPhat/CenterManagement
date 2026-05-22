using System;
using System.Collections.Generic;

namespace CenterManagement.DataAccess;

public partial class Teacher
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Specialty { get; set; }

    public int BranchId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}

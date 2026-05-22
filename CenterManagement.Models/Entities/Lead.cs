using System;
using System.Collections.Generic;

namespace CenterManagement.DataAccess;

public partial class Lead
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string Source { get; set; } = null!;

    public string? Note { get; set; }

    public int StaffId { get; set; }

    public int BranchId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual User Staff { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}

using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class Employee : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Position { get; set; }
        public DateOnly? HireDate { get; set; }

        public User User { get; set; } = null!;
    }
}

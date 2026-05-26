using CenterManagement.Models.Enums;
using System;
using System.Collections.Generic;

namespace CenterManagement.Models.Entities
{
    public class Room : BaseEntity
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; } = true;

        public Branch Branch { get; set; } = null!;
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}

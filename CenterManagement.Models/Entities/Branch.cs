using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CenterManagement.Models.Entities
{
    public class Branch
    {
        public int Id { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;

        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}

using CenterManagement.Models.Enums;

namespace CenterManagement.Models.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateRoomDto
    {
        public int BranchId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public int Capacity { get; set; }
    }

    public class UpdateRoomDto
    {
        public string RoomName { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
    }
}

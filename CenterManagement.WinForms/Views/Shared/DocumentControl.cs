using CenterManagement.WinForms.Views.Shared;

namespace CenterManagement.WinForms.Views.Shared;

public class DocumentControl : StubControl
{
    public DocumentControl() : base("Tài liệu học tập", "Quản lý và chia sẻ tài liệu học tập theo lớp.", "🗂") { }
}

public class ComplaintControl : StubControl
{
    public ComplaintControl() : base("Phản hồi & Khiếu nại", "Tiếp nhận, theo dõi và xử lý phản hồi khiếu nại từ học viên.", "💬") { }
}

public class DebtControl : StubControl
{
    public DebtControl() : base("Theo dõi Công nợ", "Danh sách học viên còn nợ học phí và nhắc nhở tự động.", "📊") { }
}

public class MakeupControl : StubControl
{
    public MakeupControl(int roleId = 1) : base("Makeup Session", "Tạo và quản lý buổi học bù khi giáo viên vắng.", "📦") { }
}

public class AuditLogControl : StubControl
{
    public AuditLogControl() : base("Audit Log", "Lịch sử thao tác hệ thống: ai làm gì, khi nào.", "🗒") { }
}

public class StudentNotesControl : StubControl
{
    public StudentNotesControl() : base("Nhận xét học viên", "Ghi nhận xét và đánh giá học viên theo từng buổi hoặc từng giai đoạn.", "📝") { }
}

public class ProgressControl : StubControl
{
    public ProgressControl() : base("Tiến độ học tập", "Theo dõi tiến độ học tập và chuyên cần của từng học viên.", "📈") { }
}

public class AssignmentControl : StubControl
{
    public AssignmentControl() : base("Quản lý Bài tập", "Giao bài tập và theo dõi kết quả nộp bài của học viên.", "📋") { }
}

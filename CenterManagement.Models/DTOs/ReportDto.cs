namespace CenterManagement.Models.DTOs
{
    public class DashboardDto
    {
        public int ActiveClasses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int PendingLeaveRequests { get; set; }
        public int ClassesPendingConfirmation { get; set; }
        public List<MonthlyRevenueDto> RevenueChart { get; set; } = new();
        public List<ClassAlertDto> Alerts { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ClassAlertDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class RevenueReportDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal DepositRevenue { get; set; }
        public decimal RemainingRevenue { get; set; }
        public int TotalEnrollments { get; set; }
        public List<PaymentDto> Details { get; set; } = new();
    }

    public class TeacherSalaryDto
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public decimal TotalTuitionCollected { get; set; }
        public decimal BaseSalary { get; set; }
        public int PenaltySessionCount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal DisciplinePenalty { get; set; }
        public decimal FinalSalary { get; set; }
        public bool IsPaid { get; set; }
    }
}

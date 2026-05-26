using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CenterManagement.DataAccess.Data;
using CenterManagement.Models.Entities;
using CenterManagement.Models.Enums;

namespace CenterManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly CenterManagementDBContext _context;

        public SeedController(CenterManagementDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Seed toàn bộ dữ liệu mẫu. Bỏ qua nếu đã có dữ liệu.
        /// </summary>
        [HttpPost("run")]
        public async Task<IActionResult> Run()
        {
            if (await _context.Students.AnyAsync())
                return Ok(new { message = "Dữ liệu đã tồn tại. Bỏ qua seed.", seeded = false });

            await SeedBranches();
            await SeedTimeSlots();
            await SeedCourses();
            await SeedAdminAccounts();
            await SeedTeachers();
            await SeedEmployees();
            await SeedRooms();
            await SeedStudents();
            await SeedClassesAndEnrollments();

            return Ok(new { message = "Seed dữ liệu mẫu hoàn tất.", seeded = true });
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu và seed lại từ đầu (chỉ dùng cho môi trường dev).
        /// </summary>
        [HttpPost("reset")]
        public async Task<IActionResult> Reset()
        {
            // Xóa theo thứ tự FK
            _context.AuditLogs.RemoveRange(_context.AuditLogs);
            _context.Notifications.RemoveRange(_context.Notifications);
            _context.ExamResults.RemoveRange(_context.ExamResults);
            _context.Exams.RemoveRange(_context.Exams);
            _context.Attendances.RemoveRange(_context.Attendances);
            _context.LeaveRequests.RemoveRange(_context.LeaveRequests);
            _context.ClassTransfers.RemoveRange(_context.ClassTransfers);
            _context.Payments.RemoveRange(_context.Payments);
            _context.Enrollments.RemoveRange(_context.Enrollments);
            _context.Schedules.RemoveRange(_context.Schedules);
            _context.TeacherClassRegistrations.RemoveRange(_context.TeacherClassRegistrations);
            _context.Classes.RemoveRange(_context.Classes);
            _context.Students.RemoveRange(_context.Students);
            _context.Rooms.RemoveRange(_context.Rooms);
            _context.TimeSlots.RemoveRange(_context.TimeSlots);
            _context.Courses.RemoveRange(_context.Courses);
            _context.Branches.RemoveRange(_context.Branches);
            _context.Teachers.RemoveRange(_context.Teachers);
            _context.Employees.RemoveRange(_context.Employees);
            _context.RefreshTokens.RemoveRange(_context.RefreshTokens);
            _context.Users.RemoveRange(_context.Users);
            await _context.SaveChangesAsync();

            return await Run();
        }

        private async Task SeedBranches()
        {
            _context.Branches.AddRange(
                new Branch { BranchName = "EN-VN Quận 1",    City = "TP. Hồ Chí Minh", Address = "123 Nguyễn Huệ, Quận 1",            Phone = "028-3821-0001", IsActive = true },
                new Branch { BranchName = "EN-VN Hải Châu",  City = "Đà Nẵng",          Address = "45 Trần Phú, Quận Hải Châu",       Phone = "0236-382-0002", IsActive = true },
                new Branch { BranchName = "EN-VN Hoàn Kiếm", City = "Hà Nội",            Address = "78 Hàng Bài, Hoàn Kiếm",           Phone = "024-3825-0003", IsActive = true }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedTimeSlots()
        {
            _context.TimeSlots.AddRange(
                new TimeSlot { SlotName = "Ca 1 (13:00-14:45)", StartTime = new TimeOnly(13, 0),  EndTime = new TimeOnly(14, 45), CreatedAt = DateTime.UtcNow },
                new TimeSlot { SlotName = "Ca 2 (15:00-16:45)", StartTime = new TimeOnly(15, 0),  EndTime = new TimeOnly(16, 45), CreatedAt = DateTime.UtcNow },
                new TimeSlot { SlotName = "Ca 3 (17:00-18:45)", StartTime = new TimeOnly(17, 0),  EndTime = new TimeOnly(18, 45), CreatedAt = DateTime.UtcNow },
                new TimeSlot { SlotName = "Ca 4 (19:00-20:45)", StartTime = new TimeOnly(19, 0),  EndTime = new TimeOnly(20, 45), CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedCourses()
        {
            _context.Courses.AddRange(
                new Course { CourseName = "TOEIC 400", CourseCode = "TOEIC400", Description = "Khóa luyện thi TOEIC đạt 400 điểm. Phù hợp người mới bắt đầu.",        TuitionFee = 3_500_000, ExamFee = 200_000, TargetScore = "400+", DurationWeeks = 12, MinStudents = 10, MaxStudents = 25, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { CourseName = "TOEIC 600", CourseCode = "TOEIC600", Description = "Khóa luyện thi TOEIC đạt 600 điểm. Trung cấp.",                          TuitionFee = 4_200_000, ExamFee = 200_000, TargetScore = "600+", DurationWeeks = 16, MinStudents = 10, MaxStudents = 25, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { CourseName = "IELTS 5.0", CourseCode = "IELTS50",  Description = "Khóa luyện IELTS nền tảng đạt band 5.0.",                                TuitionFee = 5_500_000, ExamFee = 350_000, TargetScore = "5.0", DurationWeeks = 20, MinStudents = 8,  MaxStudents = 20, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { CourseName = "IELTS 6.5", CourseCode = "IELTS65",  Description = "Khóa luyện IELTS nâng cao đạt band 6.5+.",                               TuitionFee = 7_000_000, ExamFee = 350_000, TargetScore = "6.5", DurationWeeks = 24, MinStudents = 8,  MaxStudents = 18, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Course { CourseName = "Giao tiếp cơ bản", CourseCode = "GIAO_TIEP", Description = "Khóa tiếng Anh giao tiếp thực tế cho người đi làm.",            TuitionFee = 2_800_000, ExamFee = 150_000, TargetScore = "B1",  DurationWeeks = 10, MinStudents = 10, MaxStudents = 25, IsActive = true, CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedAdminAccounts()
        {
            var adminUser = new User
            {
                UserName = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Email = "admin@envncenter.vn", FullName = "Quản trị viên hệ thống",
                PhoneNumber = "0901000001", RoleId = 1, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
            _context.Employees.Add(new Employee
            {
                UserId = adminUser.Id, Position = "System Administrator", Department = "IT",
                HireDate = new DateOnly(2020, 1, 1), ContractType = "Hợp đồng lao động",
                City = "TP. Hồ Chí Minh", CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private async Task SeedTeachers()
        {
            var teacherData = new[]
            {
                new { Un = "giaovien01", Name = "Nguyễn Thị Lan Anh",   Email = "lananh@envncenter.vn",    Phone = "0901111001",
                      Spec = "IELTS, TOEFL", Qual = "Thạc sĩ Ngôn ngữ Anh",             Years = 8,  Certs = "IELTS 8.0, CELTA",
                      Gender = "Nữ",  City = "TP. Hồ Chí Minh", Contract = "Hợp đồng lao động", Bank = "Vietcombank", Tax = "8801111001" },
                new { Un = "giaovien02", Name = "Trần Quốc Bảo",         Email = "quocbao@envncenter.vn",   Phone = "0901111002",
                      Spec = "TOEIC, Business English", Qual = "Cử nhân Tiếng Anh",      Years = 5,  Certs = "TOEIC 990, TESOL",
                      Gender = "Nam", City = "Đà Nẵng",          Contract = "Cộng tác viên",       Bank = "Techcombank", Tax = "8801111002" },
                new { Un = "giaovien03", Name = "Phạm Thị Minh Châu",    Email = "minhchau@envncenter.vn",  Phone = "0901111003",
                      Spec = "IELTS, Giao tiếp", Qual = "Thạc sĩ Giảng dạy Tiếng Anh", Years = 10, Certs = "IELTS 7.5, Delta",
                      Gender = "Nữ",  City = "Hà Nội",            Contract = "Hợp đồng lao động", Bank = "BIDV",        Tax = "8801111003" },
                new { Un = "giaovien04", Name = "Lê Hữu Phát",           Email = "huuphat@envncenter.vn",   Phone = "0901111004",
                      Spec = "TOEIC, Giao tiếp", Qual = "Cử nhân Sư phạm Tiếng Anh",    Years = 3,  Certs = "TOEIC 850, TESOL",
                      Gender = "Nam", City = "TP. Hồ Chí Minh", Contract = "Cộng tác viên",       Bank = "MB Bank",     Tax = "8801111004" },
            };
            foreach (var (t, i) in teacherData.Select((t, i) => (t, i)))
            {
                var user = new User
                {
                    UserName = t.Un, PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                    Email = t.Email, FullName = t.Name, PhoneNumber = t.Phone,
                    RoleId = 4, IsActive = true, CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _context.Teachers.Add(new Teacher
                {
                    UserId = user.Id, Specialization = t.Spec, Qualification = t.Qual,
                    YearsOfExperience = t.Years, Certificates = t.Certs,
                    Biography = $"{t.Name} có {t.Years} năm kinh nghiệm giảng dạy tiếng Anh. Chuyên môn: {t.Spec}.",
                    Gender = t.Gender, DateOfBirth = new DateOnly(1985 + i, 3 + i, 10 + i),
                    NationalId = $"07900000{i + 1}", City = t.City,
                    Address = $"Số {10 + i * 5}, đường Nguyễn Văn A, {t.City}",
                    BankAccount = $"190000000{i + 1:D3}", BankAccountName = t.Name.ToUpperInvariant(),
                    BankName = t.Bank, TaxId = t.Tax, ContractType = t.Contract,
                    ContractStartDate = new DateOnly(2020 + i, 1, 1), CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedEmployees()
        {
            var branches = await _context.Branches.ToListAsync();
            var employeeData = new[]
            {
                new { Un = "manager01", Name = "Lê Văn Quản",    Email = "manager01@envncenter.vn", Phone = "0902000001", Role = 2, BranchIdx = 0, Pos = "Quản lý chi nhánh",       Dept = "Ban Giám đốc",  Sal = 15_000_000m },
                new { Un = "manager02", Name = "Đinh Thị Hoa",   Email = "manager02@envncenter.vn", Phone = "0902000002", Role = 2, BranchIdx = 1, Pos = "Quản lý chi nhánh",       Dept = "Ban Giám đốc",  Sal = 15_000_000m },
                new { Un = "manager03", Name = "Hoàng Minh Tuấn",Email = "manager03@envncenter.vn", Phone = "0902000003", Role = 2, BranchIdx = 2, Pos = "Quản lý chi nhánh",       Dept = "Ban Giám đốc",  Sal = 15_000_000m },
                new { Un = "staff01",   Name = "Nguyễn Thị Thu", Email = "staff01@envncenter.vn",   Phone = "0903000001", Role = 3, BranchIdx = 0, Pos = "Chuyên viên tuyển sinh",  Dept = "Tuyển sinh",    Sal = 10_000_000m },
                new { Un = "staff02",   Name = "Bùi Văn Hùng",   Email = "staff02@envncenter.vn",   Phone = "0903000002", Role = 3, BranchIdx = 0, Pos = "Chuyên viên học vụ",      Dept = "Học vụ",        Sal = 10_000_000m },
                new { Un = "staff03",   Name = "Phan Thị Yến",   Email = "staff03@envncenter.vn",   Phone = "0903000003", Role = 3, BranchIdx = 1, Pos = "Chuyên viên tuyển sinh",  Dept = "Tuyển sinh",    Sal = 10_000_000m },
                new { Un = "staff04",   Name = "Trần Văn Minh",  Email = "staff04@envncenter.vn",   Phone = "0903000004", Role = 3, BranchIdx = 2, Pos = "Chuyên viên thu ngân",    Dept = "Kế toán",       Sal = 9_000_000m  },
            };
            foreach (var (em, i) in employeeData.Select((em, i) => (em, i)))
            {
                var user = new User
                {
                    UserName = em.Un, PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                    Email = em.Email, FullName = em.Name, PhoneNumber = em.Phone,
                    RoleId = em.Role, IsActive = true, CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                var branchId = em.BranchIdx < branches.Count ? branches[em.BranchIdx].Id : (int?)null;
                _context.Employees.Add(new Employee
                {
                    UserId = user.Id, BranchId = branchId, Position = em.Pos, Department = em.Dept,
                    ContractType = "Hợp đồng lao động",
                    HireDate = new DateOnly(2021 + (i % 3), 1 + (i % 12), 1),
                    ContractStartDate = new DateOnly(2021 + (i % 3), 1 + (i % 12), 1),
                    Salary = em.Sal,
                    Gender = i % 2 == 0 ? "Nữ" : "Nam",
                    DateOfBirth = new DateOnly(1990 + i, 5 + (i % 7), 15),
                    NationalId = $"07900010{i + 1}", City = "TP. Hồ Chí Minh",
                    Education = "Đại học", Major = "Quản trị kinh doanh",
                    EmergencyContact = $"Người thân của {em.Name}",
                    EmergencyPhone = $"090300{i + 10:D4}", EmergencyRelationship = "Vợ/Chồng",
                    BankAccount = $"100000{i + 1:D5}", BankAccountName = em.Name.ToUpperInvariant(),
                    BankName = "Vietcombank", TaxId = $"880200{i + 1:D4}", CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedRooms()
        {
            foreach (var branch in await _context.Branches.ToListAsync())
            {
                _context.Rooms.AddRange(
                    new Room { BranchId = branch.Id, RoomName = "P.101",  RoomType = RoomType.Standard,   Capacity = 25, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Room { BranchId = branch.Id, RoomName = "P.102",  RoomType = RoomType.Standard,   Capacity = 20, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Room { BranchId = branch.Id, RoomName = "P.201",  RoomType = RoomType.Projector,  Capacity = 25, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Room { BranchId = branch.Id, RoomName = "Lab.01", RoomType = RoomType.Lab,        Capacity = 18, IsActive = true, CreatedAt = DateTime.UtcNow }
                );
            }
            await _context.SaveChangesAsync();
        }

        private async Task SeedStudents()
        {
            var lastNames  = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý" };
            var midNames   = new[] { "Văn", "Thị", "Minh", "Quốc", "Hoàng", "Thanh", "Kim", "Anh", "Đình", "Hữu" };
            var firstNames = new[]
            {
                "An", "Bình", "Cường", "Dung", "Đức", "Giang", "Hải", "Hoa", "Hùng", "Lan",
                "Long", "Mai", "Nam", "Nga", "Phúc", "Quân", "Sơn", "Thảo", "Tú", "Vinh",
                "Xuân", "Ý", "Thắng", "Linh", "Dương", "Huy", "Khoa", "Lộc", "My", "Nhật",
                "Phương", "Quỳnh", "Rạng", "Sáng", "Thiện", "Uyên", "Vân", "Xuyên", "Yến", "Dũng",
                "Bảo", "Châu", "Diệu", "Gia", "Hiếu", "Khánh", "Lâm", "Ngọc", "Phát", "Quang",
                "Thành", "Toàn", "Trung", "Tuấn", "Uyên"
            };
            var parentNames = new[] { "Nguyễn Văn Bình", "Trần Thị Hoa", "Lê Văn Mạnh", "Phạm Thị Lan", "Hoàng Văn Đức", "Vũ Thị Thu", "Đặng Văn Nam" };
            var rand = new Random(42);
            for (int i = 1; i <= 55; i++)
            {
                var fn = firstNames[(i - 1) % firstNames.Length];
                _context.Students.Add(new Student
                {
                    StudentCode  = $"HV{i:D4}",
                    FullName     = $"{lastNames[rand.Next(lastNames.Length)]} {midNames[rand.Next(midNames.Length)]} {fn}",
                    Phone        = $"09{rand.Next(10_000_000, 99_999_999):D8}",
                    Email        = $"hv{i:D4}@gmail.com",
                    DateOfBirth  = new DateOnly(2000 + rand.Next(10), rand.Next(1, 13), rand.Next(1, 28)),
                    ParentName   = parentNames[rand.Next(parentNames.Length)],
                    ParentPhone  = $"09{rand.Next(10_000_000, 99_999_999):D8}",
                    CreatedAt    = DateTime.UtcNow.AddDays(-rand.Next(30, 400))
                });
            }
            await _context.SaveChangesAsync();
        }

        private async Task SeedClassesAndEnrollments()
        {
            var courses   = await _context.Courses.ToListAsync();
            var rooms     = await _context.Rooms.Where(r => r.IsActive).ToListAsync();
            var teachers  = await _context.Teachers.ToListAsync();
            var students  = await _context.Students.ToListAsync();
            var timeSlots = await _context.TimeSlots.ToListAsync();
            if (!courses.Any() || !rooms.Any() || !teachers.Any() || !students.Any()) return;

            var slot3 = timeSlots.FirstOrDefault(ts => ts.SlotName.Contains("Ca 3")) ?? timeSlots.First();
            var slot2 = timeSlots.FirstOrDefault(ts => ts.SlotName.Contains("Ca 2")) ?? timeSlots.First();

            var classDefs = new[]
            {
                new { Code = "TOEIC400", Name = "TOEIC400-HCM-01", Start = new DateOnly(2026, 3, 1),  RoomIdx = 0,  TIdx = 0, Status = ClassStatus.Active,    Slot = slot3 },
                new { Code = "TOEIC600", Name = "TOEIC600-HCM-01", Start = new DateOnly(2026, 4, 1),  RoomIdx = 1,  TIdx = 1, Status = ClassStatus.Active,    Slot = slot3 },
                new { Code = "IELTS50",  Name = "IELTS50-DN-01",   Start = new DateOnly(2026, 5, 1),  RoomIdx = 4,  TIdx = 2, Status = ClassStatus.Active,    Slot = slot2 },
                new { Code = "IELTS65",  Name = "IELTS65-HN-01",   Start = new DateOnly(2026, 6, 1),  RoomIdx = 8,  TIdx = 0, Status = ClassStatus.Upcoming,  Slot = slot3 },
                new { Code = "TOEIC400", Name = "TOEIC400-HCM-02", Start = new DateOnly(2026, 7, 1),  RoomIdx = 2,  TIdx = 3, Status = ClassStatus.Upcoming,  Slot = slot2 },
                new { Code = "GIAO_TIEP",Name = "GIAO_TIEP-HCM-01",Start = new DateOnly(2026, 3, 15), RoomIdx = 0,  TIdx = 1, Status = ClassStatus.Active,    Slot = slot2 },
            };

            var createdClasses = new List<Class>();
            foreach (var cd in classDefs)
            {
                var course  = courses.FirstOrDefault(c => c.CourseCode == cd.Code);
                if (course == null) continue;
                var room    = rooms.ElementAtOrDefault(cd.RoomIdx) ?? rooms.First();
                var teacher = teachers.ElementAtOrDefault(cd.TIdx) ?? teachers.First();

                var cls = new Class
                {
                    CourseId = course.Id, ClassName = cd.Name, MaxStudents = course.MaxStudents,
                    StartDate = cd.Start, EndDate = cd.Start.AddDays((course.DurationWeeks ?? 12) * 7),
                    Status = cd.Status, CreatedAt = DateTime.UtcNow
                };
                _context.Classes.Add(cls);
                await _context.SaveChangesAsync();
                createdClasses.Add(cls);

                _context.TeacherClassRegistrations.Add(new TeacherClassRegistration
                {
                    TeacherId = teacher.Id, ClassId = cls.Id, IsMainTeacher = true, CreatedAt = DateTime.UtcNow
                });

                // Lịch học: thứ 2, 4 mỗi tuần cho toàn bộ khóa
                int totalWeeks = course.DurationWeeks ?? 12;
                for (int week = 0; week < totalWeeks; week++)
                {
                    foreach (var dow in new[] { DayOfWeek.Monday, DayOfWeek.Wednesday })
                    {
                        int daysOffset = (int)dow - (int)DayOfWeek.Monday;
                        if (daysOffset < 0) daysOffset += 7;
                        var schedDate = cd.Start.AddDays(week * 7 + daysOffset);
                        _context.Schedules.Add(new Schedule
                        {
                            ClassId = cls.Id, RoomId = room.Id, TimeSlotId = cd.Slot.Id,
                            DayOfWeek = dow, ScheduleDate = schedDate, CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // ── Ghi danh học sinh ── phân phối đảm bảo tất cả 55 HS đều có ít nhất 1 lớp
            var rand = new Random(42);
            var classEnrollCounts = new[] { 18, 16, 15, 12, 10, 14 }; // số HS mỗi lớp
            var activeClassIds = new HashSet<int>();

            for (int ci = 0; ci < createdClasses.Count; ci++)
            {
                var cls    = createdClasses[ci];
                var course = courses.First(c => c.Id == cls.CourseId);
                int enCount = classEnrollCounts[ci];
                // phân phối: lấy nhóm students theo chỉ mục vòng tròn
                int start = ci * 8;
                var batch = new List<Student>();
                for (int k = 0; k < enCount; k++)
                    batch.Add(students[(start + k) % students.Count]);

                if (cls.Status == ClassStatus.Active) activeClassIds.Add(cls.Id);

                foreach (var student in batch.Distinct())
                {
                    var already = await _context.Enrollments.AnyAsync(e => e.ClassId == cls.Id && e.StudentId == student.Id);
                    if (already) continue;

                    var isActive = cls.Status == ClassStatus.Active;
                    var enrollment = new Enrollment
                    {
                        ClassId = cls.Id, StudentId = student.Id,
                        EnrollmentDate = cls.StartDate.AddDays(-rand.Next(5, 40)),
                        Status = isActive ? EnrollmentStatus.Active : EnrollmentStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Enrollments.Add(enrollment);
                    await _context.SaveChangesAsync();

                    // Đặt cọc 20%
                    decimal tuition = course.TuitionFee ?? 3_500_000;
                    _context.Payments.Add(new Payment
                    {
                        EnrollmentId = enrollment.Id,
                        Amount = Math.Round(tuition * 0.2m, -3),
                        PaymentDate = enrollment.EnrollmentDate,
                        PaymentMethod = rand.Next(2) == 0 ? PaymentMethod.Cash : PaymentMethod.BankTransfer,
                        Status = PaymentStatus.Completed,
                        Note = $"Đặt cọc 20% học phí lớp {cls.ClassName}",
                        CreatedAt = DateTime.UtcNow
                    });

                    // Thanh toán 80% còn lại cho lớp đang hoạt động
                    if (isActive)
                    {
                        _context.Payments.Add(new Payment
                        {
                            EnrollmentId = enrollment.Id,
                            Amount = Math.Round(tuition * 0.8m, -3),
                            PaymentDate = cls.StartDate.AddDays(-rand.Next(1, 5)),
                            PaymentMethod = PaymentMethod.BankTransfer,
                            Status = PaymentStatus.Completed,
                            Note = $"Học phí 80% còn lại lớp {cls.ClassName}",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    await _context.SaveChangesAsync();
                }
            }

            // ── Đảm bảo 100% học sinh được ghi danh ít nhất 1 lớp ──
            var enrolledIds = await _context.Enrollments.Select(e => e.StudentId).Distinct().ToListAsync();
            var unenrolled  = students.Where(s => !enrolledIds.Contains(s.Id)).ToList();
            var activeClsList = createdClasses.Where(c => activeClassIds.Contains(c.Id)).ToList();

            foreach (var (stu, idx) in unenrolled.Select((s, i) => (s, i)))
            {
                var cls    = activeClsList[idx % activeClsList.Count];
                var course = courses.First(c => c.Id == cls.CourseId);
                decimal tuition = course.TuitionFee ?? 3_500_000;

                var enrollment = new Enrollment
                {
                    ClassId = cls.Id, StudentId = stu.Id,
                    EnrollmentDate = cls.StartDate.AddDays(-rand.Next(5, 20)),
                    Status = EnrollmentStatus.Active, CreatedAt = DateTime.UtcNow
                };
                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                _context.Payments.Add(new Payment
                {
                    EnrollmentId = enrollment.Id,
                    Amount = Math.Round(tuition * 0.2m, -3),
                    PaymentDate = enrollment.EnrollmentDate,
                    PaymentMethod = PaymentMethod.Cash,
                    Status = PaymentStatus.Completed,
                    Note = $"Đặt cọc 20% học phí lớp {cls.ClassName}",
                    CreatedAt = DateTime.UtcNow
                });
                _context.Payments.Add(new Payment
                {
                    EnrollmentId = enrollment.Id,
                    Amount = Math.Round(tuition * 0.8m, -3),
                    PaymentDate = cls.StartDate.AddDays(-2),
                    PaymentMethod = PaymentMethod.BankTransfer,
                    Status = PaymentStatus.Completed,
                    Note = $"Học phí 80% còn lại lớp {cls.ClassName}",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            // ── Seed điểm danh cho các buổi học đã qua (lớp Active) ──
            var today = DateOnly.FromDateTime(DateTime.Today);
            var pastSchedules = await _context.Schedules
                .Where(s => activeClassIds.Contains(s.ClassId) && s.ScheduleDate <= today)
                .ToListAsync();

            var attendRand = new Random(99);
            var attendStatuses = new[] { AttendanceStatus.Present, AttendanceStatus.Present, AttendanceStatus.Present,
                AttendanceStatus.Present, AttendanceStatus.Present, AttendanceStatus.Present, AttendanceStatus.Present,
                AttendanceStatus.Late, AttendanceStatus.Late, AttendanceStatus.Absent, AttendanceStatus.Excused };

            foreach (var sched in pastSchedules)
            {
                var classEnrollments = await _context.Enrollments
                    .Where(e => e.ClassId == sched.ClassId && e.Status == EnrollmentStatus.Active)
                    .Select(e => e.StudentId)
                    .ToListAsync();
                foreach (var stuId in classEnrollments)
                {
                    var already = await _context.Attendances.AnyAsync(a => a.ScheduleId == sched.Id && a.StudentId == stuId);
                    if (already) continue;
                    var st = attendStatuses[attendRand.Next(attendStatuses.Length)];
                    _context.Attendances.Add(new Attendance
                    {
                        ScheduleId = sched.Id, StudentId = stuId, Status = st,
                        Note = st == AttendanceStatus.Absent ? "Vắng không phép" :
                               st == AttendanceStatus.Late   ? "Đến trễ 10 phút" :
                               st == AttendanceStatus.Excused ? "Bận công việc" : null,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}

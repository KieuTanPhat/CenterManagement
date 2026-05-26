using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CenterManagement.DataAccess.Data;

public class CenterManagementDBContextFactory : IDesignTimeDbContextFactory<CenterManagementDBContext>
{
    public CenterManagementDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CenterManagementDBContext>();
        optionsBuilder.UseSqlServer("Server=KIEUPHAT;Database=CenterManagement_DB;User Id=sa;Password=123;TrustServerCertificate=True");

        return new CenterManagementDBContext(optionsBuilder.Options);
    }
}
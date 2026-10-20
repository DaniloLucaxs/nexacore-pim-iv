using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace PIM_3sem_backend.Data;
public class DesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? "Server=localhost;Database=NexaCore;Integrated Security=true;TrustServerCertificate=true").Options);
}

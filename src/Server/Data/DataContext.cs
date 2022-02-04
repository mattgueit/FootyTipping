using FootyTipping.Server.Entitites;
using Microsoft.EntityFrameworkCore;

namespace FootyTipping.Server.Data
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration _configuration;

        public DataContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sql server database
            options.UseSqlServer(_configuration.GetConnectionString("Default"));
        }
    }
}

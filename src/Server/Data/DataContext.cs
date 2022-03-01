using FootyTipping.Server.Entitites;
using Microsoft.EntityFrameworkCore;

namespace FootyTipping.Server.Data
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration _configuration;

        public DataContext() { }

        public DataContext(IConfiguration configuration, DbContextOptions<DataContext> options) 
            : base(options)
        {
            _configuration = configuration;
        }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sql server database
            options.UseSqlServer(_configuration.GetConnectionString("Default"));
        }
    }
}

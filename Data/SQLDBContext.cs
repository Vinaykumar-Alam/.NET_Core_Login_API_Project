using Flutter_Backed.Models;
using Microsoft.EntityFrameworkCore;

namespace Flutter_Backed.Data
{
    public class SQLDBContext : DbContext
    {
        public SQLDBContext(DbContextOptions<SQLDBContext> options) : base(options)
        {
           
        }
        public virtual DbSet<Login> LoginTbl { get; set; }
        public virtual DbSet<RefreshToken> RefreshToken_Tbl{ get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Login>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<Login>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Login>().Property(e => e.Role).HasDefaultValue(UserRoles.USER);
        }
    }
}

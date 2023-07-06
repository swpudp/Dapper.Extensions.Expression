using Microsoft.EntityFrameworkCore;


namespace EfTest
{
    public class TestDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(MySqlServerVersion.AutoDetect("server=127.0.0.1;port=3306;database=dapper_extension;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8"));

            optionsBuilder .UseSnakeCaseNamingConvention();



            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(e =>
            {
                e.ToTable("order");
                e.HasKey(x => x.Id);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}

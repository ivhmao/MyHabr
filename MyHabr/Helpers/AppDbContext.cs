using Microsoft.EntityFrameworkCore;
using MyHabr.Entities;
using MyHabr.Enums;

namespace MyHabr.Helpers
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Article> Articles { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
       // public AppDbContext() : base()
        {
            // пересоздадим базу данных
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        // The following configures EF to create a Sqlite database file 
        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //    => options.UseSqlite("Data Source=myhabr.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Role adminRole = RoleEnum.Admin;
            Role moderatorRole = RoleEnum.Moderator;
            Role writerRole = RoleEnum.Writer;
            Role readerRole = RoleEnum.Reader;

            modelBuilder.Entity<Role>().HasData(
                 adminRole,
                 moderatorRole,
                 writerRole,
                 readerRole
                 );

            var admin = new User
            {
                Id = 1,
                Name = "Admin",
                Login = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin")
            };
            modelBuilder.Entity<User>().HasData(admin);

            modelBuilder.Entity<Role>()
                .HasMany(p => p.Users)
                .WithMany(p => p.Roles)
                .UsingEntity(j => j.HasData(new[] { new { RolesId = 1, UsersId = 1 },
                                                   new { RolesId = moderatorRole.Id, UsersId = admin.Id },
                                                   new { RolesId = writerRole.Id, UsersId = admin.Id },
                                                   new { RolesId = readerRole.Id, UsersId = admin.Id }}));
        }

    }
}

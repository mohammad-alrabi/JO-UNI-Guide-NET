using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace JO_UNI_Guide.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : base(options) { }

        //(DbSets) الجداول 
        // to convert the models to tables in DB
        public DbSet<University> Universities { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Course> Courses  { get; set; }
        public DbSet<SiteStatistic> SiteStatistics { get; set; }
        public DbSet <ContactMessage> ContactMessages { get; set; }

        //Flunt Api (use in Relations)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //للتأكد من عدم تكرار الاسماء وغيره 
            modelBuilder.Entity<University>()
                .HasIndex(u => u.Name).IsUnique();
            // في حال بدنا نحذف جامعة فينحذف معها كل اقسامها وتخصصاتها تلقائيا 
            // Cascade delte
            modelBuilder.Entity<Faculty>()
                .HasOne(f => f.University)
                .WithMany(f => f.Faculties)
                .HasForeignKey(f =>f.University_ID)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}

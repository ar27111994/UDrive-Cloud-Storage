using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;


namespace WebApplication5.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
       
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {

            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
       
        public bool ConfirmedEmail { get; set; }
        public DbSet<Feedback> feedback { get; set; }
        
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
       
        public ApplicationDbContext()
            : base("conn", throwIfV1Schema: false)
        {
        }
       
        public DbSet<FilesHash> FileHash { get; set; }
        public DbSet<UserManager> UserManagers { get; set; }
        public DbSet<UserFiles> UserFiless { get; set; }

        public DbSet<Package> packages { get; set; }
        public DbSet<General> General { get; set; }
        public DbSet<ADLog> ADLogs { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<StorageUpdate>StorageUpdate { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<WebApplication5.Models.Feedback> Feedbacks { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

          
               
            modelBuilder.Entity<UserFiles>()
                .HasKey(t => new { t.Email, t.id });


            modelBuilder.Entity<UserFiles>()
                .HasRequired(pt => pt.User)
                .WithMany(p => p.Files)
                .HasForeignKey(pt => pt.Email);
            modelBuilder.Entity<UserFiles>()
                .HasRequired(pt => pt.File)
                .WithMany(t => t.User)
                .HasForeignKey(pt => pt.id);


        }
        //public System.Data.Entity.DbSet<WebApplication5.Models.ApplicationUser> ApplicationUsers { get; set; }

        //  public System.Data.Entity.DbSet<WebApplication5.Models.ApplicationUser> ApplicationUsers { get; set; }
    }
}
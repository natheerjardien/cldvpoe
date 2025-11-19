using Microsoft.EntityFrameworkCore;
using System.Data.Entity;
using System.Reflection.Emit;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace CLDV6212_POE_ST10435542.Models.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public Microsoft.EntityFrameworkCore.DbSet<User> Users { get; set; }
    }
}

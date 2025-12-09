
using Excid.Staas.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Excid.Staas.Data
{
    public class StassDbContext: DbContext
    {
        public StassDbContext(DbContextOptions<StassDbContext> options)
                : base(options)
        { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
           => optionsBuilder.LogTo(Console.WriteLine, LogLevel.Warning);
        public DbSet<SignedItem> SignedItems { get; set; }
        public DbSet<APIToken> APITokens { get; set; }
    }
}

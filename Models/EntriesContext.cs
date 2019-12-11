using Microsoft.EntityFrameworkCore;

namespace YENISOZLUK.Models
{
    public class EntriesContext : DbContext
    {
        public EntriesContext(DbContextOptions<EntriesContext> options) : base(options)
        {

        }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Title> Titles { get; set; }

    }
}

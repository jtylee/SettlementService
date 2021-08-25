using Microsoft.EntityFrameworkCore;

namespace SettlementService.Models
{
    public class SettlementContext : DbContext
    {
        public SettlementContext()
        {

        }

        public SettlementContext(DbContextOptions<SettlementContext> options) : base(options)
        {

        }

        public DbSet<Booking> Bookings { get; set; }
    }
}

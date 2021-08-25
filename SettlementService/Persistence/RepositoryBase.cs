using SettlementService.Models;

namespace SettlementService.Persistence
{
    public class RepositoryBase
    {
        protected SettlementContext _ctx;
        public RepositoryBase(SettlementContext ctx)
        {
            _ctx = ctx;
            _ctx.Database.EnsureCreated(); // TODO Remove this seeding when hooked up to a real database
        }
    }
}

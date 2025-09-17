using Flutter_Backed.Models;
using Flutter_Backed.Data;
using HotChocolate.Data;
using HotChocolate.AspNetCore;
using HotChocolate.Authorization;
namespace Flutter_Backed.GraphQL
{
    public class Query
    {
        private readonly SQLDBContext _dbContext;
        public Query(SQLDBContext sQLDBContext)
        {
            _dbContext = sQLDBContext;
        }
        public IQueryable<Login> Login() => _dbContext.LoginTbl;
        public IQueryable<RefreshToken> Refresh() => _dbContext.RefreshToken_Tbl;

        [Authorize]
        public async Task<IEnumerable<Notification>> Notifications()
        {
            return _sampleNotifications;
        }
        private readonly List<Notification> _sampleNotifications = new List<Notification>
        {
            new Notification { Id = 1, Message = "Your ride to Airport is confirmed!", Time = DateTime.Now.AddHours(-2), IsRead = false },
            new Notification { Id = 2, Message = "Payment for Client Office ride is pending.", Time = DateTime.Now.AddHours(-1), IsRead = true }
        };
    }
}
using Microsoft.AspNetCore.Mvc.Filters;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace Graph.Farm.Website.Controllers
{
    public class AuthFilter : Attribute, IAsyncAuthorizationFilter
    {
        private readonly bool requireGame;

        public AuthFilter(bool requireGame = false)
        {
            this.requireGame = requireGame;
        }

        public static string ContextKey = "acctid";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var key = context.HttpContext.Request.Headers["X-ACCT-TOKEN"].SingleOrDefault();
            if (key == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            using (var db = new MySqlConnection(Settings.MysqlConnectionString))
            {
                await db.OpenAsync();
                var id = (await db.QueryAsync<int>("SELECT Id FROM Accounts WHERE ApiKeyHash = @hash", new { hash = APIController.sha(key) }))
                    .FirstOrDefault();
                if (id == 0)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
                context.HttpContext.Items[ContextKey] = id;
            }
        }
    }
}

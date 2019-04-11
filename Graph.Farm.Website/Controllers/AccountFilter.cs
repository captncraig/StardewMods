using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Graph.Farm.Website.Controllers
{
    public class AccountFilter : Attribute, IAuthorizationFilter
    {
        private bool _requireGame;

        public AccountFilter(bool requireGame = false)
        {
            _requireGame = requireGame;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using RazorWeb.Models;
using System.Security.Claims;

namespace RazorWeb.Security.Requirements
{
    public class AppAuthorizationHandler : IAuthorizationHandler
    {
        private readonly ILogger<AppAuthorizationHandler> _logger;
        private readonly UserManager<AppUser> _userManager;

        public AppAuthorizationHandler(ILogger<AppAuthorizationHandler> logger, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var requirements = context.PendingRequirements.ToList();
            _logger.LogInformation("context.Resource ~ " + context.Resource?.GetType().Name);
            foreach (var requirement in requirements)
            {
                if (requirement is GenZRequirement)
                {
                    if (IsGenZ(context.User, (GenZRequirement) requirement))
                    {
                        context.Succeed(requirement);
                    }      
                }

                if (requirement is ArticleUpdateRequirement)
                {
                    bool canUpdate = CanUpdateArticle(context.User, context.Resource, (ArticleUpdateRequirement)requirement);

                    if (canUpdate) context.Succeed(requirement);
                }
                //context.Succeed(requirement);

                //if (requirement is OtherRequirement)
                //{
                //    //context.Succeed(requirement);
                //}
            }
            return Task.CompletedTask;
        }

        private bool CanUpdateArticle(ClaimsPrincipal user, object resource, ArticleUpdateRequirement requirement)
        {
            if (user.IsInRole("Admin"))
            {
                _logger.LogInformation($"{user.Identity.Name} la Admin, co the update article");
                return true;
            }

            var article = resource as Article;
            var dateCreated = article.Created;
            var dateCanUpdate = new DateTime(requirement.Year, requirement.Month, requirement.Day);
            
            if (dateCreated < dateCanUpdate)
            {
                _logger.LogWarning($"{user.Identity.Name} khong the update article vi article da qua han");
                return false;
            }

            return true;
        }

        private bool IsGenZ(ClaimsPrincipal user, GenZRequirement requirement)
        {
            var appUserTask = _userManager.GetUserAsync(user);
            Task.WaitAll(appUserTask);
            var appUser = appUserTask.Result;   

            if (appUser.DateOfBirth == null)
            {
                _logger.LogWarning($"{appUser.UserName} khong co ngay sinh, khong thoa man GenZRequirement");
                return false;
            }

            int year = appUser.DateOfBirth.Value.Year;

            var success = year >= requirement.FromYear && year <= requirement.ToYear;

            if (success)
            {
                _logger.LogInformation($"{appUser.UserName} thoa man GenZRequirement");
            }
            else
            {
                _logger.LogWarning($"{appUser.UserName} khong thoa man GenZRequirement");
            }

            return success;
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorWeb.Models;

namespace RazorWeb.Areas.Admin.Pages.User
{
    [Authorize(Roles = "Admin")]
    public class AddRoleModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        //private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _myBlogContext;

        public AddRoleModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext myBlogContext)
        {
            _userManager = userManager;
            //_signInManager = signInManager;
            _roleManager = roleManager;
            _myBlogContext = myBlogContext;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
 
        public AppUser user { get; set; }

        public SelectList allRoles { get; set; }

        [BindProperty]
        [DisplayName("Các vai trò gán cho user")]
        public string[] RoleNames { get; set; }

        public List<IdentityRoleClaim<string>> claimsInRole { get; set; }
        public List<IdentityUserClaim<string>> claimsInUserClaim { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Không tìm thấy user.");
            }

            user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound($"Không thể tải lên user có ID = {id}.");
            }

            RoleNames = (await _userManager.GetRolesAsync(user)).ToArray<string>();

            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).OrderBy(name => name).ToListAsync();

            allRoles = new SelectList(roleNames);
          
            await GetClaims(id);

            return Page();
        }

        public async Task GetClaims(string id)
        {
            var listRoles = from r in _myBlogContext.Roles
                            join ur in _myBlogContext.UserRoles on r.Id equals ur.RoleId
                            where ur.UserId == id
                            select r;

            var _claimsInRole = from c in _myBlogContext.RoleClaims
                                join r in listRoles on c.RoleId equals r.Id
                                select c;

            claimsInRole = await _claimsInRole.ToListAsync();

            claimsInUserClaim = await (from c in _myBlogContext.UserClaims
                                        where c.UserId == id
                                        select c).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Không tìm thấy user.");
            }

            user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound($"Không thể tải lên user có ID = {id}.");
            }

            // RoleNames
            await GetClaims(id);

            var oldRoleNames = (await _userManager.GetRolesAsync(user)).ToArray<string>();
            var deletedRoles = oldRoleNames.Where(r => !RoleNames.Contains(r));
            var addRoles = RoleNames.Where(r => !oldRoleNames.Contains(r));

            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            allRoles = new SelectList(roleNames);

            // Remove old roles from the user
            var resultDelete = await _userManager.RemoveFromRolesAsync(user, deletedRoles);
            if (!resultDelete.Succeeded)
            {
                resultDelete.Errors.ToList().ForEach(e => ModelState.AddModelError(string.Empty, e.Description));
                return Page();
            }

            // Add new roles to the user
            var resultAdd = await _userManager.AddToRolesAsync(user, addRoles);
            if (!resultAdd.Succeeded)
            {
                resultAdd.Errors.ToList().ForEach(e => ModelState.AddModelError(string.Empty, e.Description));
                return Page();
            }

            StatusMessage = $"Vai trò của bạn vừa được cập nhật cho user: {user.UserName}";

            return RedirectToPage("./Index");
        }
    }
}

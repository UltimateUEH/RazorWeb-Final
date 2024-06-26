using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace RazorWeb.Areas.Admin.Pages.Role
{
    // Policy: Tạo ra các policy -> AllowEditRole
    [Authorize(Policy = "AllowEditRole")]
    public class EditModel : RolePageModel
    {
        public EditModel(RoleManager<IdentityRole> roleManager, AppDbContext myBlogContext) : base(roleManager, myBlogContext)
        {
        }

        public class InputModel
        {
            [Display(Name = "Tên của vai trò")]
            [Required(ErrorMessage = "{0} không được để trống")]
            [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} phải có ít nhất {2} và nhiều nhất {1} ký tự")]
            public string Name { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<IdentityRoleClaim<string>> Claims { get; set; }

        public IdentityRole role { get; set; }

        public async Task<IActionResult> OnGet(string roleid)
        {
            if (roleid == null) return NotFound("Không tìm thấy vai trò này");

            role = await _roleManager.FindByIdAsync(roleid);

            if (role != null)
            {
                Input = new InputModel
                {
                    Name = role.Name
                };

                Claims = await _myBlogContext.RoleClaims.Where(rc => rc.RoleId == roleid).ToListAsync();
                return Page();
            }

            return NotFound("Không tìm thấy vai trò này");
        }

        public async Task<IActionResult> OnPostAsync(string roleid)
        {
            if (roleid == null) return NotFound("Không tìm thấy vai trò này");
            role = await _roleManager.FindByIdAsync(roleid);

            if (role == null) return NotFound("Không tìm thấy vai trò này");
            Claims = await _myBlogContext.RoleClaims.Where(rc => rc.RoleId == roleid).ToListAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            role.Name = Input.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa đổi tên vai trò: {Input.Name}";
                return RedirectToPage("./Index");
            }
            else
            {
                result.Errors.ToList().ForEach(error =>
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                });
            }

            return Page();
        }
    }
}

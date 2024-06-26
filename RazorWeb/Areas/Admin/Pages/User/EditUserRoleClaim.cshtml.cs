using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace RazorWeb.Areas.Admin.Pages.User
{
    public class EditUserRoleClaimModel : PageModel
    {
        private readonly AppDbContext _myBlogContext;
        private readonly UserManager<AppUser> _userManager;

        public EditUserRoleClaimModel(AppDbContext myBlogContext, UserManager<AppUser> userManager)
        {
            _myBlogContext = myBlogContext;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Display(Name = "Tên của đặc tính")]
            [Required(ErrorMessage = "{0} không được để trống")]
            [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} phải có ít nhất {2} và nhiều nhất {1} ký tự")]
            public string ClaimType { get; set; }

            [Display(Name = "Giá trị của đặc tính")]
            [Required(ErrorMessage = "{0} không được để trống")]
            [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} phải có ít nhất {2} và nhiều nhất {1} ký tự")]
            public string ClaimValue { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public AppUser user { get; set; }
        public IdentityUserClaim<string> userclaim { get; set; }

        public NotFoundObjectResult OnGet() => NotFound("Không được truy cập");

        public async Task<IActionResult> OnGetAddClaimAsync(string userid)
        {
            user = await _userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return NotFound("Không tìm thấy user");
            }

            return Page();
        }
        
        public async Task<IActionResult> OnPostAddClaimAsync(string userid)
        {
            user = await _userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return NotFound("Không tìm thấy user");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var claims = _myBlogContext.UserClaims.Where(c => c.UserId == user.Id);
            if (claims.Any(c => c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "Đặc tính đã tồn tại");
                return Page();
            }

            var result = await _userManager.AddClaimAsync(user, new Claim(Input.ClaimType, Input.ClaimValue));
            StatusMessage = "Đã thêm đặc tính cho user";

            return RedirectToPage("./AddRole", new { Id = user.Id });
        }

        public async Task<IActionResult> OnGetEditClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Không tìm thấy user");

            userclaim = _myBlogContext.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userclaim.UserId);

            if (user == null) return NotFound("Không tìm thấy user");

            Input = new InputModel
            {
                ClaimType = userclaim.ClaimType,
                ClaimValue = userclaim.ClaimValue
            };

            return Page();
        }
        
        public async Task<IActionResult> OnPostEditClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Không tìm thấy user");

            userclaim = _myBlogContext.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userclaim.UserId);

            if (user == null) return NotFound("Không tìm thấy user");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (_myBlogContext.UserClaims.Any(c => c.UserId == user.Id 
                                            && c.ClaimType == Input.ClaimType 
                                            && c.ClaimValue == Input.ClaimValue 
                                            && c.Id == claimid))
            {
                ModelState.AddModelError(string.Empty, "Đặc tính này đã tồn tại");
                return Page();
            }

            userclaim.ClaimType = Input.ClaimType;
            userclaim.ClaimValue = Input.ClaimValue;

            await _myBlogContext.SaveChangesAsync();
            StatusMessage = "Đã cập nhật đặc tính cho user: " + user.UserName;

            return RedirectToPage("./AddRole", new { Id = user.Id});
        }
        
        public async Task<IActionResult> OnPostDeleteAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Không tìm thấy user");

            userclaim = _myBlogContext.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            user = await _userManager.FindByIdAsync(userclaim.UserId);

            if (user == null) return NotFound("Không tìm thấy user");

            await _userManager.RemoveClaimAsync(user, new Claim(userclaim.ClaimType, userclaim.ClaimValue));

            StatusMessage = "Đã xoá đặc tính cho user: " + user.UserName;

            return RedirectToPage("./AddRole", new { Id = user.Id});
        }
    }
}

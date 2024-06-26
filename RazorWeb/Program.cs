using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using NuGet.Configuration;
using RazorWeb.Models;
using RazorWeb.Security.Requirements;
using RazorWeb.Services;
using System.Security.Claims;
using System.Text;

namespace RazorWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddOptions();

            var mailSettings = builder.Configuration.GetSection("MailSettings");
            builder.Services.Configure<MailSettings>(mailSettings);
            builder.Services.AddSingleton<IEmailSender, SendMailService>();

            builder.Services.AddRazorPages();
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyBlogContext"));
            });

            // Đăng ký Identity
            //builder.Services.AddDefaultIdentity<AppUser>()
            //    .AddEntityFrameworkStores<MyBlogContext>()
            //    .AddDefaultTokenProviders();
            
            builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Truy cập IdentityOptions
            builder.Services.Configure<IdentityOptions>(options => {
                // Thiết lập về Password
                options.Password.RequireDigit = false; // Không bắt phải có số
                options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
                options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
                options.Password.RequireUppercase = false; // Không bắt buộc chữ in
                options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
                options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

                // Cấu hình Lockout - khóa user
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
                options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lần thì khóa
                options.Lockout.AllowedForNewUsers = true;

                // Cấu hình về User.
                options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;  // Email là duy nhất

                // Cấu hình đăng nhập.
                options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
                options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
                options.SignIn.RequireConfirmedAccount = false;          // Xác thực tài khoản

            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login/";
                options.LogoutPath = "/logout/";
                options.AccessDeniedPath = "/access-denied.html";
            });

            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    var googleConfig = builder.Configuration.GetSection("Authentication:Google");
                    options.ClientId = googleConfig["ClientId"];
                    options.ClientSecret = googleConfig["ClientSecret"];
                    // http://localhost:5171/signin-google
                    // https://localhost:7008/signin-google
                    options.CallbackPath = "/signin-google";
                })
                .AddFacebook(options =>
                {
                    var facebookConfig = builder.Configuration.GetSection("Authentication:Facebook");
                    options.AppId = facebookConfig["AppId"];
                    options.AppSecret = facebookConfig["AppSecret"];
                    // https://localhost:7008/signin-facebook
                    options.CallbackPath = "/signin-facebook";
                })
                //.AddMicrosoftAccount()
                //.AddTwitter()
                ;

            builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();
            builder.Services.AddTransient<IAuthorizationHandler, AppAuthorizationHandler>();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AllowEditRole", policyBuilder =>
                {
                    // Điều kiện của Policy
                    policyBuilder.RequireAuthenticatedUser(); // Đã đăng nhập
                    //policyBuilder.RequireRole("Admin", "Editor"); // Có Role Admin hoặc Editor

                    //policyBuilder.RequireClaim("manage.role", "add", "update");
                    policyBuilder.RequireClaim("canedit", "user");

                    // Điều kiện của Claim
                    //policyBuilder.RequireClaim("Ten Claim", "gia_tri_1", "gia_tri_2"); // Có Claim Edit Role
                    //policyBuilder.RequireClaim("Ten Claim", new string[]
                    //{
                    //    "gia_tri_1", 
                    //    "gia_tri_2"
                    //});

                    //IdentityRoleClaim<string> claim1; -> DbContext
                    //IdentityUserClaim<string> claim2; -> DbContext
                    //Claim claim3; -> từ dịch vụ của Identity                    
                });

                options.AddPolicy("InGenZ", policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.Requirements.Add(new GenZRequirement()); // GenZRequirement

                    // new GenZRequirement()
                });

                options.AddPolicy("ShowAdminMenu", policyBuilder =>
                {
                    policyBuilder.RequireRole("Admin");
                    
                });

                options.AddPolicy("CanUpdateArticle", policyBuilder =>
                {
                    policyBuilder.Requirements.Add(new ArticleUpdateRequirement());
                });
            });

            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}

/*  CREATE, READ, UPDATE, DELETE (CRUD)
 *  
 *  dotnet aspnet-codegenerator razorpage Index --model RazorWeb.Models.Article --dataContext RazorWeb.Models.MyBlogContext -outDir Pages/Blog  -udl --referenceScriptLibraries
 * 
 *  Identity:
 *      - Authentication: Xác định danh tính người dùng -> Login, Register, Logout
 *      
 *      - Authorization: Xác định quyền hạn của người dùng -> Role, Policy
 *          + Role-based Authorization - xác thực quyền dựa trên Role
 *              * Role: Admin, Editor, Manager, Member, Guest, ...
 *           
 *              /Areas/Admin/Pages/Role
 *              Index
 *              Create
 *              Edit
 *              Delete
 *           
 *              dotnet new page -n Index -o /Areas/Admin/Pages/Role -na RazorWeb.Areas.Admin.Pages.Role
 *              dotnet new page - n Create - o / Areas / Admin / Pages / Role - na RazorWeb.Areas.Admin.Pages.Role
 *           
 *           * Policy-based Authorization - xác thực quyền dựa trên Policy
 *           * Claim-based Authorization - xác thực quyền dựa trên Claim
 *              Claim -> Đặc tính, tính chất của đối tượng (User)
 *              
 *              Bằng lái B2 (Role) -> được lái 4 chỗ
 *              - Ngày sinh -> claim
 *              - Nơi sinh -> claim
 *              
 *              Mua rượu ( >18 tuổi)
 *              - Kiểm tra ngày sinh: Claim-based Authorization
 *              
 *           [Authorize] ~ Controller, Action, PageModel -> Đăng nhập
 *      
 *      - Quản lý User: Sign Up, User, Role, Policy,...
 *      
 *  /Identity/Account/Login
 *  /Identity/Account/Manage
 *  
 *  dotnet aspnet-codegenerator identity -dc RazorWeb.Models.MyBlogContext
 *  
 *  
 */

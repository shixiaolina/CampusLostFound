using CampusLostAndFound.Data;
using CampusLostAndFound.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CampusLostAndFound.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl = returnUrl ?? Url.Action("Index", "Items");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"用户 {model.Email} 登录成功，重定向到 {returnUrl}");
                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    ModelState.AddModelError(string.Empty, "请启用两步验证功能");
                    return View(model);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"用户 {model.Email} 账户已锁定");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "登录失败，请检查邮箱和密码");
                    return View(model);
                }
            }

            return View(model);
        }

        // GET: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("用户已登出");
            return RedirectToAction("Login", "Account"); // 修改重定向目标为登录页面
        }

        // GET: /Account/Lockout
        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        // 注册功能（保持不变）
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // 保持原有注册逻辑不变...
            if (ModelState.IsValid)
            {
                // 检查学工号是否已存在
                if (await _context.Users.AnyAsync(u => u.StudentId == model.StudentId))
                {
                    ModelState.AddModelError("StudentId", "该学工号已注册");
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    StudentId = model.StudentId,
                    UserType = model.UserType
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // 根据用户类型分配角色
                    string roleName = model.UserType switch
                    {
                        UserType.Student => "Student",
                        UserType.Staff => "Staff",
                        UserType.Admin => "Admin",
                        _ => "Student"
                    };

                    // 确保角色存在
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                    }

                    await _userManager.AddToRoleAsync(user, roleName);

                    // 自动登录
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Items");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}
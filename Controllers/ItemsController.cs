using CampusLostAndFound.Data;
using CampusLostAndFound.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using Microsoft.Extensions.Logging; // 添加日志命名空间

namespace CampusLostAndFound.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ItemsController> _logger; // 添加日志记录器

        public ItemsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<ItemsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: 所有物品
        public async Task<IActionResult> Index(string searchString, ItemType? type, ItemStatus? status)
        {
            var items = _context.Items.Include(i => i.User).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                items = items.Where(i => i.Name.Contains(searchString) || i.Description.Contains(searchString));
            }

            if (type.HasValue)
            {
                items = items.Where(i => i.Type == type.Value);
            }

            if (status.HasValue)
            {
                items = items.Where(i => i.Status == status.Value);
            }

            return View(await items.OrderByDescending(i => i.CreatedAt).ToListAsync());
        }

        // GET: 创建物品
        public IActionResult Create()
        {
            return View();
        }

        // POST: 创建物品
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Location,Type,ImageFile")] Item item)
        {
            if (!ModelState.IsValid) // 直接使用 ModelState
            {
                _logger.LogWarning("模型验证失败: {Errors}", ModelState.Values);
                return View(item);
            }

            // 确保用户已登录
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("未授权的用户尝试提交失物信息");
                return Unauthorized();
            }

            // 设置用户和状态
            item.UserId = user.Id;
            item.Status = await _userManager.IsInRoleAsync(user, "Staff") ||
                          await _userManager.IsInRoleAsync(user, "Admin")
                ? ItemStatus.Published
                : ItemStatus.Pending;

            // 设置时间戳
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            // 处理文件上传
            if (item.ImageFile != null && item.ImageFile.Length > 0)
            {
                try
                {
                    // 验证文件类型和大小
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(item.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFile", "只允许上传 JPG, PNG 或 GIF 格式的图片");
                        _logger.LogWarning("用户尝试上传不支持的文件类型: {FileName}", item.ImageFile.FileName);
                        return View(item);
                    }

                    if (item.ImageFile.Length > 5 * 1024 * 1024) // 5MB 限制
                    {
                        ModelState.AddModelError("ImageFile", "图片大小不能超过 5MB");
                        _logger.LogWarning("用户尝试上传超过大小限制的文件: {FileName}", item.ImageFile.FileName);
                        return View(item);
                    }

                    // 保存文件
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                        _logger.LogInformation("创建上传文件夹: {FolderPath}", uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + item.ImageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await item.ImageFile.CopyToAsync(fileStream);
                    }

                    item.ImagePath = "/uploads/" + uniqueFileName;
                    _logger.LogInformation("文件上传成功: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "文件上传失败");
                    ModelState.AddModelError("", "文件上传失败，请稍后重试");
                    return View(item);
                }
            }

            try
            {
                // 保存到数据库
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                _logger.LogInformation("失物信息已保存，ID: {ItemId}", item.Id);

                // 添加成功消息并重定向
                TempData["SuccessMessage"] = "失物信息已成功提交！" +
                    (item.Status == ItemStatus.Pending ? "审核通过后将显示在列表中。" : "");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存失物信息到数据库失败");
                ModelState.AddModelError("", "服务器错误，请稍后重试");
                return View(item);
            }
        }

        // 其他控制器方法...
    }
}
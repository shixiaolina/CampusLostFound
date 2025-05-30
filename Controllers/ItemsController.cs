﻿using CampusLostAndFound.Data;
using CampusLostAndFound.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;

namespace CampusLostAndFound.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ItemsController> _logger;
        private readonly IWebHostEnvironment _env;

        public ItemsController(ApplicationDbContext context,
                              UserManager<ApplicationUser> userManager,
                              ILogger<ItemsController> logger,
                              IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _env = env;
        }

        // GET: 所有物品
        public async Task<IActionResult> Index(string searchString, ItemType? type, ItemStatus? status)
        {
            var items = _context.Items
                .Include(i => i.User)
                .AsQueryable();

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

        // GET: Items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("请求的物品ID为空");
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
            {
                _logger.LogWarning("未找到ID为 {Id} 的物品", id);
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("未找到当前登录用户");
                return Unauthorized();
            }

            if (item.UserId != currentUser.Id &&
                !await _userManager.IsInRoleAsync(currentUser, "Admin") &&
                !await _userManager.IsInRoleAsync(currentUser, "Staff"))
            {
                _logger.LogWarning("用户 {UserId} 尝试访问不属于自己的物品详情", currentUser.Id);
                return Forbid();
            }

            return View(item);
        }

        // GET: 创建物品
        public IActionResult Create()
        {
            return View();
        }

        // POST: 创建物品
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Location,Type,ContactPhone,Notes,ImageFile")] Item item)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("模型验证失败: {Errors}", ModelState.Values);
                return View(item);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("未授权的用户尝试提交失物信息");
                return Unauthorized();
            }

            item.UserId = currentUser.Id;
            item.Status = await _userManager.IsInRoleAsync(currentUser, "Staff") ||
                          await _userManager.IsInRoleAsync(currentUser, "Admin")
                ? ItemStatus.Published
                : ItemStatus.Pending;

            item.CreatedAt = DateTime.Now;
            item.UpdatedAt = DateTime.Now;

            if (item.ImageFile != null && item.ImageFile.Length > 0)
            {
                try
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(item.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFile", "只允许上传 JPG, PNG 或 GIF 格式的图片");
                        _logger.LogWarning("用户 {UserId} 尝试上传不支持的文件类型: {FileName}",
                                           currentUser.Id, item.ImageFile.FileName);
                        return View(item);
                    }

                    if (item.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "图片大小不能超过 5MB");
                        _logger.LogWarning("用户 {UserId} 尝试上传超过大小限制的文件: {FileName}",
                                           currentUser.Id, item.ImageFile.FileName);
                        return View(item);
                    }

                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
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
                    _logger.LogInformation("用户 {UserId} 成功上传文件: {FilePath}", currentUser.Id, filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "用户 {UserId} 文件上传失败", currentUser.Id);
                    ModelState.AddModelError("", "文件上传失败，请稍后重试");
                    return View(item);
                }
            }

            try
            {
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                _logger.LogInformation("用户 {UserId} 成功提交失物信息，ID: {ItemId}", currentUser.Id, item.Id);

                TempData["SuccessMessage"] = "失物信息已成功提交！" +
                    (item.Status == ItemStatus.Pending ? "审核通过后将显示在列表中。" : "已直接发布到列表。");

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "用户 {UserId} 保存失物信息到数据库失败", currentUser.Id);
                ModelState.AddModelError("", "保存失败，请检查输入信息或稍后重试");
                return View(item);
            }
        }

        // GET: Items/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("请求的编辑物品ID为空");
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("未找到ID为 {Id} 的物品进行编辑", id);
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("未找到当前登录用户");
                return Unauthorized();
            }

            if (item.UserId != currentUser.Id &&
                !await _userManager.IsInRoleAsync(currentUser, "Admin") &&
                !await _userManager.IsInRoleAsync(currentUser, "Staff"))
            {
                _logger.LogWarning("用户 {UserId} 尝试编辑不属于自己的物品", currentUser.Id);
                return Forbid();
            }

            return View(item);
        }

        // POST: Items/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Location,Type,ContactPhone,Notes,Status,ImageFile")] Item item)
        {
            if (id != item.Id)
            {
                _logger.LogWarning("编辑请求的ID与物品ID不匹配: 请求ID={RequestId}, 物品ID={ItemId}", id, item.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("编辑时模型验证失败: {Errors}", ModelState.Values);
                return View(item);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("未找到当前登录用户");
                return Unauthorized();
            }

            try
            {
                var originalItem = await _context.Items.FindAsync(id);
                if (originalItem == null)
                {
                    _logger.LogWarning("未找到ID为 {Id} 的物品进行编辑", id);
                    return NotFound();
                }

                bool isAdminOrStaff = await _userManager.IsInRoleAsync(currentUser, "Admin") ||
                                      await _userManager.IsInRoleAsync(currentUser, "Staff");

                if (originalItem.UserId != currentUser.Id && !isAdminOrStaff)
                {
                    _logger.LogWarning("用户 {UserId} 尝试编辑不属于自己的物品", currentUser.Id);
                    return Forbid();
                }

                originalItem.Name = item.Name;
                originalItem.Description = item.Description;
                originalItem.Location = item.Location;
                originalItem.Type = item.Type;
                originalItem.ContactPhone = item.ContactPhone;
                originalItem.Notes = item.Notes;
                originalItem.UpdatedAt = DateTime.Now;

                if (isAdminOrStaff)
                {
                    originalItem.Status = item.Status;

                    if (originalItem.Status == ItemStatus.Published &&
                        (item.Status == ItemStatus.Pending || item.Status == ItemStatus.Rejected))
                    {
                        originalItem.PublishedAt = DateTime.Now;
                    }
                }

                if (item.ImageFile != null && item.ImageFile.Length > 0)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(originalItem.ImagePath))
                        {
                            var oldImagePath = Path.Combine(_env.WebRootPath, originalItem.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                                _logger.LogInformation("删除旧图片: {FilePath}", oldImagePath);
                            }
                        }

                        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + item.ImageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await item.ImageFile.CopyToAsync(fileStream);
                        }

                        originalItem.ImagePath = "/uploads/" + uniqueFileName;
                        _logger.LogInformation("用户 {UserId} 成功更新图片: {FilePath}", currentUser.Id, filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "用户 {UserId} 更新图片失败", currentUser.Id);
                        ModelState.AddModelError("", "图片更新失败，请稍后重试");
                        return View(item);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("用户 {UserId} 成功更新失物信息，ID: {ItemId}", currentUser.Id, id);

                TempData["SuccessMessage"] = "失物信息已成功更新！";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "用户 {UserId} 更新失物信息到数据库失败", currentUser.Id);
                ModelState.AddModelError("", "更新失败，请检查输入信息或稍后重试");
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户 {UserId} 编辑物品时发生未知错误", currentUser.Id);
                ModelState.AddModelError("", "发生未知错误，请稍后重试");
                return View(item);
            }
        }

        // GET: Items/Claim/5 - 显示认领表单
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Claim(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("请求的认领物品ID为空");
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("未找到当前登录用户");
                return Unauthorized();
            }

            var item = await _context.Items
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.Status == ItemStatus.Published && !m.IsClaimed);

            if (item == null)
            {
                _logger.LogWarning("未找到ID为 {Id} 的可认领物品", id);
                return NotFound();
            }

            var viewModel = new ClaimViewModel
            {
                ItemId = item.Id,
                ItemName = item.Name,
                Name = "",
                Contact = "",
                Reason = ""
            };

            return View(viewModel);
        }

        // POST: Items/Claim/5 - 处理认领提交
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Claim(int id, [Bind("ItemId,ItemName,Name,Contact,Reason")] ClaimViewModel viewModel)
        {
            if (id != viewModel.ItemId)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("未找到当前登录用户");
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var item = await _context.Items.FindAsync(id);
                    if (item == null || item.Status != ItemStatus.Published || item.IsClaimed)
                    {
                        return NotFound();
                    }

                    item.IsClaimed = true;
                    item.ClaimerId = currentUser.Id;
                    item.ClaimerName = viewModel.Name;
                    item.ClaimerContact = viewModel.Contact;
                    item.ClaimReason = viewModel.Reason;
                    item.ClaimDate = DateTime.Now;
                    item.Status = ItemStatus.Archived;

                    _context.Update(item);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("用户 {UserId} 成功认领物品 {ItemId}", currentUser.Id, id);
                    TempData["SuccessMessage"] = "物品已成功认领并归档";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(viewModel);
        }

        // GET: Items/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            try
            {
                // 删除关联图片文件
                if (!string.IsNullOrEmpty(item.ImagePath))
                {
                    var imagePath = Path.Combine(_env.WebRootPath, item.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                        _logger.LogInformation("删除物品图片: {ImagePath}", imagePath);
                    }
                }

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                _logger.LogInformation("管理员 {UserId} 删除了物品 {ItemId}",
                                      User.FindFirstValue(ClaimTypes.NameIdentifier), id);

                TempData["SuccessMessage"] = "物品已成功删除！";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除物品失败: {ItemId}", id);
                TempData["ErrorMessage"] = "删除物品时发生错误，请稍后重试。";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}
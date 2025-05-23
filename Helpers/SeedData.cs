// Helpers/SeedData.cs
using CampusLostAndFound.Data;
using CampusLostAndFound.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusLostAndFound.Helpers
{
    public static class SeedData
    {
        private static ApplicationUser adminUser;

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var config = services.GetRequiredService<IConfiguration>();

            // 创建角色
            string[] roleNames = { "Admin", "Staff", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 创建管理员
            var adminEmail = config["AdminConfig:Email"];
            var adminPassword = config["AdminConfig:Password"];

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "系统管理员",
                    StudentId = "ADMIN001",
                    UserType = UserType.Admin,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 创建测试物品
            if (!await context.Items.AnyAsync())
            {
                var student = await userManager.FindByEmailAsync("student@test.com") ?? new ApplicationUser
                {
                    UserName = "student@test.com",
                    Email = "student@test.com",
                    FullName = "测试学生",
                    StudentId = "STU20230001",
                    UserType = UserType.Student,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(student, "Student@123");

                var items = new List<Item>
                {
                    new() {
                        Name = "笔记本电脑",
                        Description = "黑色MacBook Pro 15寸",
                        Location = "图书馆三楼",
                        Type = ItemType.Lost,
                        Status = ItemStatus.Published,
                        User = student
                    },
                    new() {
                        Name = "钱包",
                        Description = "棕色皮质钱包，内含学生证",
                        Location = "食堂二楼",
                        Type = ItemType.Found,
                        Status = ItemStatus.Published,
                        User = adminUser
                    }
                };

                await context.Items.AddRangeAsync(items);
                await context.SaveChangesAsync();
            }
        }
    }
}
// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CampusLostAndFound.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "姓名")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "学工号")]
        public string StudentId { get; set; }

        [Display(Name = "用户类型")]
        public UserType UserType { get; set; } = UserType.Student;

        public virtual ICollection<Item> PostedItems { get; set; }
    }

    public enum UserType
    {
        [Display(Name = "学生")]
        Student,
        [Display(Name = "工作人员")]
        Staff,
        [Display(Name = "管理员")]
        Admin
    }
}
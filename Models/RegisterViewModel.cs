using System.ComponentModel.DataAnnotations;
using CampusLostAndFound.Models;

namespace CampusLostAndFound.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "请选择用户类型")]
        [Display(Name = "用户类型")]
        public UserType UserType { get; set; }

        [Required(ErrorMessage = "请输入学工号")]
        [Display(Name = "学工号")]
        [StringLength(20, ErrorMessage = "学工号长度不能超过20个字符")]
        public string StudentId { get; set; }

        [Required(ErrorMessage = "请输入姓名")]
        [Display(Name = "姓名")]
        [StringLength(50, ErrorMessage = "姓名长度不能超过50个字符")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "请输入邮箱")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [Display(Name = "邮箱")]
        public string Email { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "密码长度至少为6位", MinimumLength = 6)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "两次密码不一致")]
        public string ConfirmPassword { get; set; }
    }
}
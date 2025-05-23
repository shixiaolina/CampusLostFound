using System.ComponentModel.DataAnnotations;

namespace CampusLostAndFound.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "请输入电子邮箱")]
        [EmailAddress(ErrorMessage = "请输入有效的电子邮箱地址")]
        [Display(Name = "电子邮箱")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@campus\.com$",
            ErrorMessage = "请使用校园邮箱（格式：xxx@campus.com）")]
        public string Email { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        [StringLength(100, ErrorMessage = "{0}长度至少需要{2}个字符", MinimumLength = 6)]
        public string Password { get; set; }

        [Display(Name = "记住我")]
        public bool RememberMe { get; set; }
    }
}
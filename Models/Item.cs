using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace CampusLostAndFound.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "物品名称")]
        public string Name { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "日期")]
        [DataType(DataType.DateTime)]
        public DateTime ItemDate { get; set; } = DateTime.Now;

        [Display(Name = "位置")]
        public string Location { get; set; }

        [Display(Name = "图片路径")]
        public string? ImagePath { get; set; }

        [NotMapped] // 这个属性不会存入数据库
        [Display(Name = "上传图片")]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFile { get; set; }

        // 关联用户ID（提交者）
        [Display(Name = "提交者")]
        public string? UserId { get; set; }

        // 导航属性：提交物品的用户
        [Display(Name = "提交者信息")]
        public virtual ApplicationUser? User { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "更新时间")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Display(Name = "状态")]
        public ItemStatus Status { get; set; } = ItemStatus.Pending;

        [Display(Name = "物品类型")]
        public ItemType Type { get; set; }

        // 添加联系信息（用于失主或拾获者联系方式）
        [Display(Name = "联系电话")]
        public string? ContactPhone { get; set; }

        // 添加备注信息
        [Display(Name = "备注")]
        public string? Notes { get; set; }

        [Display(Name = "是否已认领")]
        public bool IsClaimed { get; set; } = false;

        [Display(Name = "认领人姓名")]
        public string? ClaimerName { get; set; }

        [Display(Name = "认领日期")]
        [DataType(DataType.DateTime)]
        public DateTime? ClaimDate { get; set; }

        [Display(Name = "认领理由")]
        public string? ClaimReason { get; set; }
        // 认领人ID
        [Display(Name = "认领人ID")]
        public string? ClaimerId { get; set; }
        // 认领人联系信息
        [Display(Name = "认领人联系信息")]
        public string? ClaimerContact { get; set; }

    }

    public enum ItemStatus
    {
        [Display(Name = "待审核")]
        Pending,
        [Display(Name = "已发布")]
        Published,
        [Display(Name = "已认领")]
        Claimed,
        [Display(Name = "已归档")]
        Archived
    }

    public enum ItemType
    {
        [Display(Name = "丢失物品")]
        Lost,
        [Display(Name = "拾获物品")]
        Found
    }
}
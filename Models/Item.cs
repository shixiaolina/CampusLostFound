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

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "更新时间")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now; // 添加更新时间属性并设置默认值

        [Display(Name = "状态")]
        public ItemStatus Status { get; set; } = ItemStatus.Pending;

        [Display(Name = "物品类型")]
        public ItemType Type { get; set; }
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
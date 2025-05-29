// 在 Models/ClaimViewModel.cs 中添加以下代码
namespace CampusLostAndFound.Models
{
    public class ClaimViewModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }  // 新增属性
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Reason { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SORMS.FE.Pages
{
    public class IndexModel : PageModel
    {
        public int TotalResidents { get; set; } = 156;
        public int OccupiedRooms { get; set; } = 89;
        public int PendingBills { get; set; } = 23;
        public int ActiveServices { get; set; } = 45;
        
        public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();

        public void OnGet()
        {
            // Initialize recent activities
            RecentActivities = new List<RecentActivity>
            {
                new RecentActivity
                {
                    Title = "Nguyễn Văn A đã đăng ký phòng mới",
                    TimeAgo = "5 phút trước",
                    IconClass = "fas fa-user-plus",
                    Status = "Hoàn thành",
                    StatusClass = "active"
                },
                new RecentActivity
                {
                    Title = "Hóa đơn #BILL-001 đã được thanh toán",
                    TimeAgo = "15 phút trước",
                    IconClass = "fas fa-check-circle",
                    Status = "Hoàn thành",
                    StatusClass = "active"
                },
                new RecentActivity
                {
                    Title = "Yêu cầu dịch vụ dọn dẹp phòng 201",
                    TimeAgo = "30 phút trước",
                    IconClass = "fas fa-concierge-bell",
                    Status = "Đang xử lý",
                    StatusClass = "pending"
                },
                new RecentActivity
                {
                    Title = "Cập nhật thông tin phòng 105",
                    TimeAgo = "1 giờ trước",
                    IconClass = "fas fa-edit",
                    Status = "Hoàn thành",
                    StatusClass = "active"
                },
                new RecentActivity
                {
                    Title = "Thông báo mới về quy định văn phòng",
                    TimeAgo = "2 giờ trước",
                    IconClass = "fas fa-bell",
                    Status = "Đã gửi",
                    StatusClass = "active"
                }
            };
        }
    }

    public class RecentActivity
    {
        public string Title { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
    }
}




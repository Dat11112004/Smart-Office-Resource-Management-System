namespace SORMS.API.Services
{
    using Microsoft.EntityFrameworkCore;
    using SORMS.API.Data;
    using SORMS.API.DTOs;
    using SORMS.API.Interfaces;
    using SORMS.API.Models;

    public class CheckInService : ICheckInService
    {
        private readonly SormsDbContext _context;
        private readonly INotificationService _notificationService;

        public CheckInService(SormsDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // Resident tạo yêu cầu check-in vào phòng
        public async Task<CheckInRecordDto> CreateCheckInRequestAsync(int residentId, int roomId)
        {
            // Kiểm tra resident có tồn tại không
            var resident = await _context.Residents.FindAsync(residentId);
            if (resident == null)
                throw new Exception("Resident không tồn tại");

            // Kiểm tra resident đã có yêu cầu pending hoặc đang ở phòng chưa
            var existingRecord = await _context.CheckInRecords
                .Where(r => r.ResidentId == residentId && 
                       (r.Status == "PendingCheckIn" || r.Status == "CheckedIn"))
                .FirstOrDefaultAsync();

            if (existingRecord != null)
                throw new Exception("Bạn đã có yêu cầu đang chờ hoặc đang ở trong phòng");

            // Kiểm tra phòng có tồn tại và còn trống không
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
                throw new Exception("Phòng không tồn tại");

            if (room.IsOccupied || !room.IsAvailable)
                throw new Exception("Phòng đã có người ở hoặc không khả dụng");

            // Tạo yêu cầu check-in
            var checkInRequest = new CheckInRecord
            {
                ResidentId = residentId,
                RoomId = roomId,
                RequestTime = DateTime.UtcNow,
                Status = "PendingCheckIn",
                RequestType = "CheckIn"
            };

            _context.CheckInRecords.Add(checkInRequest);
            await _context.SaveChangesAsync();

            // Gửi thông báo cho tất cả Staff và Admin
            await SendNotificationToStaffAndAdminAsync(
                $"Yêu cầu check-in mới từ {resident.FullName} vào phòng {room.RoomNumber}",
                checkInRequest.Id
            );

            return await MapToDto(checkInRequest);
        }

        // Resident tạo yêu cầu check-out khỏi phòng
        public async Task<CheckInRecordDto> CreateCheckOutRequestAsync(int residentId, int checkInRecordId)
        {
            // Kiểm tra record có tồn tại và thuộc về resident này không
            var record = await _context.CheckInRecords
                .Include(r => r.Room)
                .Include(r => r.Resident)
                .FirstOrDefaultAsync(r => r.Id == checkInRecordId && r.ResidentId == residentId);

            if (record == null)
                throw new Exception("Không tìm thấy thông tin check-in");

            if (record.Status != "CheckedIn")
                throw new Exception("Bạn chưa check-in vào phòng này");

            // Kiểm tra đã có yêu cầu check-out chưa
            if (record.Status == "PendingCheckOut")
                throw new Exception("Đã có yêu cầu check-out đang chờ xử lý");

            // Cập nhật trạng thái sang PendingCheckOut
            record.CheckOutRequestTime = DateTime.UtcNow;
            record.Status = "PendingCheckOut";
            record.RequestType = "CheckOut";

            await _context.SaveChangesAsync();

            // Gửi thông báo cho Staff và Admin
            await SendNotificationToStaffAndAdminAsync(
                $"Yêu cầu check-out từ {record.Resident.FullName} khỏi phòng {record.Room.RoomNumber}",
                record.Id
            );

            return await MapToDto(record);
        }

        // Staff/Admin phê duyệt yêu cầu check-in
        public async Task<bool> ApproveCheckInRequestAsync(int requestId, int approverId, bool isApproved, string? rejectReason)
        {
            var record = await _context.CheckInRecords
                .Include(r => r.Room)
                .Include(r => r.Resident)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (record == null)
                throw new Exception("Không tìm thấy yêu cầu");

            if (record.Status != "PendingCheckIn")
                throw new Exception("Yêu cầu này không ở trạng thái chờ phê duyệt");

            record.ApprovedBy = approverId;
            record.ApprovedTime = DateTime.UtcNow;

            if (isApproved)
            {
                // Phê duyệt - cho phép check-in
                record.Status = "CheckedIn";
                record.CheckInTime = DateTime.UtcNow;
                
                // Cập nhật trạng thái phòng
                record.Room.IsOccupied = true;
                record.Room.IsAvailable = false; // ✨ Đồng bộ IsAvailable
                record.Room.CurrentResident = record.Resident.FullName; // Cập nhật tên cư dân hiện tại
                
                // Cập nhật thông tin resident
                record.Resident.RoomId = record.RoomId;
                record.Resident.CheckInDate = DateTime.UtcNow;

                // Gửi thông báo cho resident
                await _notificationService.CreateNotificationAsync(new NotificationDto
                {
                    ResidentId = record.ResidentId,
                    Message = $"Yêu cầu check-in vào phòng {record.Room.RoomNumber} đã được phê duyệt",
                    SentDate = DateTime.UtcNow,
                    IsRead = false
                });
            }
            else
            {
                // Từ chối
                record.Status = "Rejected";
                record.RejectReason = rejectReason ?? "Không đủ điều kiện";

                // Gửi thông báo cho resident
                await _notificationService.CreateNotificationAsync(new NotificationDto
                {
                    ResidentId = record.ResidentId,
                    Message = $"Yêu cầu check-in vào phòng {record.Room.RoomNumber} đã bị từ chối. Lý do: {record.RejectReason}",
                    SentDate = DateTime.UtcNow,
                    IsRead = false
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Staff/Admin phê duyệt yêu cầu check-out
        public async Task<bool> ApproveCheckOutRequestAsync(int requestId, int approverId, bool isApproved, string? rejectReason)
        {
            var record = await _context.CheckInRecords
                .Include(r => r.Room)
                .Include(r => r.Resident)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (record == null)
                throw new Exception("Không tìm thấy yêu cầu");

            if (record.Status != "PendingCheckOut")
                throw new Exception("Yêu cầu này không ở trạng thái chờ phê duyệt check-out");

            record.ApprovedBy = approverId;
            record.ApprovedTime = DateTime.UtcNow;

            if (isApproved)
            {
                // Phê duyệt - cho phép check-out
                record.Status = "CheckedOut";
                record.CheckOutTime = DateTime.UtcNow;
                
                // Cập nhật trạng thái phòng
                record.Room.IsOccupied = false;
                record.Room.IsAvailable = true; // ✨ Đồng bộ IsAvailable
                record.Room.CurrentResident = null; // Xóa tên cư dân hiện tại
                
                // Cập nhật thông tin resident
                record.Resident.RoomId = null;
                record.Resident.CheckOutDate = DateTime.UtcNow;

                // Gửi thông báo cho resident
                await _notificationService.CreateNotificationAsync(new NotificationDto
                {
                    ResidentId = record.ResidentId,
                    Message = $"Yêu cầu check-out khỏi phòng {record.Room.RoomNumber} đã được phê duyệt",
                    SentDate = DateTime.UtcNow,
                    IsRead = false
                });
            }
            else
            {
                // Từ chối - giữ nguyên trạng thái CheckedIn
                record.Status = "CheckedIn";
                record.CheckOutRequestTime = null;
                record.RejectReason = rejectReason ?? "Không đủ điều kiện";

                // Gửi thông báo cho resident
                await _notificationService.CreateNotificationAsync(new NotificationDto
                {
                    ResidentId = record.ResidentId,
                    Message = $"Yêu cầu check-out khỏi phòng {record.Room.RoomNumber} đã bị từ chối. Lý do: {record.RejectReason}",
                    SentDate = DateTime.UtcNow,
                    IsRead = false
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Lấy danh sách yêu cầu check-in chờ phê duyệt
        public async Task<IEnumerable<CheckInRecordDto>> GetPendingCheckInRequestsAsync()
        {
            var records = await _context.CheckInRecords
                .Include(r => r.Resident)
                .Include(r => r.Room)
                .Where(r => r.Status == "PendingCheckIn")
                .OrderByDescending(r => r.RequestTime)
                .ToListAsync();

            return records.Select(r => MapToDto(r).Result);
        }

        // Lấy danh sách yêu cầu check-out chờ phê duyệt
        public async Task<IEnumerable<CheckInRecordDto>> GetPendingCheckOutRequestsAsync()
        {
            var records = await _context.CheckInRecords
                .Include(r => r.Resident)
                .Include(r => r.Room)
                .Where(r => r.Status == "PendingCheckOut")
                .OrderByDescending(r => r.CheckOutRequestTime)
                .ToListAsync();

            return records.Select(r => MapToDto(r).Result);
        }

        // Lấy trạng thái check-in hiện tại của resident
        public async Task<CheckInRecordDto?> GetCurrentCheckInStatusAsync(int residentId)
        {
            var record = await _context.CheckInRecords
                .Include(r => r.Resident)
                .Include(r => r.Room)
                .Include(r => r.ApprovedByUser)
                .Where(r => r.ResidentId == residentId && 
                       (r.Status == "PendingCheckIn" || r.Status == "CheckedIn" || r.Status == "PendingCheckOut"))
                .OrderByDescending(r => r.RequestTime)
                .FirstOrDefaultAsync();

            if (record == null)
                return null;

            return await MapToDto(record);
        }

        // Lấy lịch sử check-in của resident
        public async Task<IEnumerable<CheckInRecordDto>> GetCheckInHistoryAsync(int residentId)
        {
            var records = await _context.CheckInRecords
                .Include(r => r.Resident)
                .Include(r => r.Room)
                .Include(r => r.ApprovedByUser)
                .Where(r => r.ResidentId == residentId)
                .OrderByDescending(r => r.RequestTime)
                .ToListAsync();

            return records.Select(r => MapToDto(r).Result);
        }

        // Lấy tất cả records (cho Staff/Admin)
        public async Task<IEnumerable<CheckInRecordDto>> GetAllCheckInRecordsAsync()
        {
            var records = await _context.CheckInRecords
                .Include(r => r.Resident)
                .Include(r => r.Room)
                .Include(r => r.ApprovedByUser)
                .OrderByDescending(r => r.RequestTime)
                .ToListAsync();

            return records.Select(r => MapToDto(r).Result);
        }

        // Helper method: Gửi thông báo cho tất cả Staff và Admin
        private async Task SendNotificationToStaffAndAdminAsync(string message, int checkInRecordId)
        {
            // Lấy tất cả Staff và Admin
            var staffAndAdmins = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.Name == "Admin" || u.Role.Name == "Staff")
                .ToListAsync();

            // Tìm Resident liên kết với mỗi User
            foreach (var user in staffAndAdmins)
            {
                var resident = await _context.Residents
                    .FirstOrDefaultAsync(r => r.UserId == user.Id);

                if (resident != null)
                {
                    await _notificationService.CreateNotificationAsync(new NotificationDto
                    {
                        ResidentId = resident.Id,
                        Message = message,
                        SentDate = DateTime.UtcNow,
                        IsRead = false
                    });
                }
            }
        }

        // Helper method: Map CheckInRecord sang DTO
        private async Task<CheckInRecordDto> MapToDto(CheckInRecord record)
        {
            if (record.Resident == null)
                record.Resident = await _context.Residents.FindAsync(record.ResidentId);
            
            if (record.Room == null)
                record.Room = await _context.Rooms.FindAsync(record.RoomId);

            if (record.ApprovedBy.HasValue && record.ApprovedByUser == null)
                record.ApprovedByUser = await _context.Users.FindAsync(record.ApprovedBy.Value);

            return new CheckInRecordDto
            {
                Id = record.Id,
                ResidentId = record.ResidentId,
                ResidentName = record.Resident?.FullName ?? "N/A",
                RoomId = record.RoomId,
                RoomNumber = record.Room?.RoomNumber ?? "N/A",
                RequestTime = record.RequestTime,
                ApprovedTime = record.ApprovedTime,
                CheckInTime = record.CheckInTime,
                CheckOutRequestTime = record.CheckOutRequestTime,
                CheckOutTime = record.CheckOutTime,
                Status = record.Status,
                RejectReason = record.RejectReason,
                ApprovedBy = record.ApprovedBy,
                ApprovedByName = record.ApprovedByUser?.Username ?? null,
                RequestType = record.RequestType
            };
        }
    }
}

using SORMS.API.DTOs;

namespace SORMS.API.Interfaces
{
public interface ICheckInService
{
    Task<bool> CheckInAsync(int residentId, string qrCodeData);
    Task<bool> CheckOutAsync(int residentId, string qrCodeData);
    Task<CheckInRecordDto> GetLatestCheckInStatusAsync(int residentId);
    Task<IEnumerable<CheckInRecordDto>> GetCheckInHistoryAsync(int residentId);
}

}

using SORMS.API.DTOs;

namespace SORMS.API.Interfaces
{
    public interface IServiceRequestService
    {
        Task<IEnumerable<ServiceRequestDto>> GetAllRequestsAsync();
        Task<ServiceRequestDto> GetRequestByIdAsync(int id);
        Task<ServiceRequestDto> CreateRequestAsync(ServiceRequestDto requestDto);
        Task<bool> UpdateRequestStatusAsync(int id, string status);
        Task<bool> DeleteRequestAsync(int id);


    }

}

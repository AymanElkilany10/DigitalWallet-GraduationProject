using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Admin;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;

namespace DigitalWallet.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResult<IEnumerable<UserManagementDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllAsync();
                var userDtos = _mapper.Map<IEnumerable<UserManagementDto>>(users);
                return ServiceResult<IEnumerable<UserManagementDto>>.Success(userDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<UserManagementDto>>.Failure(
                    $"Error retrieving users: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<FraudLogDto>>> GetFraudLogsAsync()
        {
            try
            {
                var logs = await _unitOfWork.FraudLogs.GetRecentLogsAsync(24);
                var logDtos = _mapper.Map<IEnumerable<FraudLogDto>>(logs);
                return ServiceResult<IEnumerable<FraudLogDto>>.Success(logDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<FraudLogDto>>.Failure(
                    $"Error retrieving fraud logs: {ex.Message}");
            }
        }
    }
}
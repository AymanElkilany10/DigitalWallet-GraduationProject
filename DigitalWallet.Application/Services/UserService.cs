using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Admin;
using DigitalWallet.Application.DTOs.Auth;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;

namespace DigitalWallet.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResult<UserManagementDto>> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<UserManagementDto>.Failure("User not found");

                var userDto = _mapper.Map<UserManagementDto>(user);
                return ServiceResult<UserManagementDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserManagementDto>.Failure($"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<ServiceResult<UserManagementDto>> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(email);
                if (user == null)
                    return ServiceResult<UserDto>.Failure("User not found");

                var userDto = _mapper.Map<UserDto>(user);
                return ServiceResult<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.Failure($"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<ServiceResult<UserDto>> GetUserByPhoneAsync(string phone)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByPhoneNumberAsync(phone);
                if (user == null)
                    return ServiceResult<UserManagementDto>.Failure("User not found");

                var userDto = _mapper.Map<UserManagementDto>(user);
                return ServiceResult<UserManagementDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserManagementDto>.Failure($"Error retrieving user: {ex.Message}");
            }
        }
    }
}
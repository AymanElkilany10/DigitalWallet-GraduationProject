using System.Reflection;
using System;
using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Auth;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Domain.Entities;
using DigitalWallet.Domain.Enums;
using DigitalWallet.Application.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DigitalWallet.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly JwtTokenGenerator _jwtTokenGenerator;

        public AuthService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            JwtTokenGenerator jwtTokenGenerator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<ServiceResult<LoginResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
                    return ServiceResult<LoginResponseDto>.Failure("Email already registered");

                if (await _unitOfWork.Users.PhoneExistsAsync(request.PhoneNumber))
                    return ServiceResult<LoginResponseDto>.Failure("Phone number already registered");

                // Use PasswordHasher helper
                var salt = PasswordHasher.GenerateSalt();
                var passwordHash = PasswordHasher.HashPassword(request.Password, salt);

                var user = _mapper.Map<User>(request);
                user.PasswordHash = passwordHash;
                user.Salt = salt;
                user.KycLevel = KycLevel.Basic;
                user.Status = UserStatus.Active;

                await _unitOfWork.Users.AddAsync(user);

                var wallet = new Wallet
                {
                    UserId = user.Id,
                    CurrencyCode = "EGP",
                    Balance = 0,
                    DailyLimit = 5000,
                    MonthlyLimit = 20000
                };

                await _unitOfWork.Wallets.AddAsync(wallet);

                var fakeBankAccount = new FakeBankAccount
                {
                    UserId = user.Id,
                    AccountNumber = OtpGenerator.GenerateAccountNumber(), // Use helper
                    Balance = 10000
                };

                await _unitOfWork.FakeBankAccounts.AddAsync(fakeBankAccount);
                await _unitOfWork.SaveChangesAsync();

                // Use JwtTokenGenerator helper
                var token = _jwtTokenGenerator.GenerateToken(user);
                var refreshToken = OtpGenerator.GenerateRefreshToken(); // Use helper

                var response = new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    RequiresOtp = false
                };

                return ServiceResult<LoginResponseDto>.Success(response, "Registration successful");
            }
            catch (Exception ex)
            {
                return ServiceResult<LoginResponseDto>.Failure($"Registration failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                User? user = null;

                if (request.EmailOrPhone.Contains("@"))
                    user = await _unitOfWork.Users.GetByEmailAsync(request.EmailOrPhone);
                else
                    user = await _unitOfWork.Users.GetByPhoneNumberAsync(request.EmailOrPhone);

                if (user == null)
                    return ServiceResult<LoginResponseDto>.Failure("Invalid credentials");

                // Use PasswordHasher helper
                if (!PasswordHasher.VerifyPassword(request.Password, user.Salt, user.PasswordHash))
                    return ServiceResult<LoginResponseDto>.Failure("Invalid credentials");

                if (user.Status != UserStatus.Active)
                    return ServiceResult<LoginResponseDto>.Failure("Account is suspended");

                // Use OtpGenerator helper
                var otpCode = OtpGenerator.GenerateOtpCode();
                var otp = new OtpCode
                {
                    UserId = user.Id,
                    Code = otpCode,
                    Type = OtpType.Login,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false
                };

                await _unitOfWork.OtpCodes.AddAsync(otp);
                await _unitOfWork.SaveChangesAsync();

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    RequiresOtp = true,
                    Token = string.Empty,
                    RefreshToken = string.Empty
                };

                return ServiceResult<LoginResponseDto>.Success(
                    response,
                    $"OTP sent to your phone. Code: {otpCode}");
            }
            catch (Exception ex)
            {
                return ServiceResult<LoginResponseDto>.Failure($"Login failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> VerifyOtpAsync(VerifyOtpRequestDto request)
        {
            try
            {
                var otp = await _unitOfWork.OtpCodes.GetValidOtpAsync(
                    request.UserId,
                    request.Code,
                    OtpType.Login);

                if (otp == null)
                    return ServiceResult<bool>.Failure("Invalid or expired OTP");

                await _unitOfWork.OtpCodes.MarkAsUsedAsync(otp.Id);

                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _unitOfWork.Users.UpdateAsync(user);
                }

                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(true, "OTP verified successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"OTP verification failed: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> SendOtpAsync(Guid userId, string otpType)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<bool>.Failure("User not found");

                // Use OtpGenerator helper
                var otpCode = OtpGenerator.GenerateOtpCode();
                var type = Enum.Parse<OtpType>(otpType, true);

                var otp = new OtpCode
                {
                    UserId = userId,
                    Code = otpCode,
                    Type = type,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false
                };

                await _unitOfWork.OtpCodes.AddAsync(otp);
                await _unitOfWork.SaveChangesAsync();

                return ServiceResult<bool>.Success(
                    true,
                    $"OTP sent successfully. Code: {otpCode}");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Failed to send OTP: {ex.Message}");
            }
        }
    }
}
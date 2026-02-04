using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Wallet;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WalletService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResult<WalletDto>> CreateWalletAsync(CreateWalletRequestDto request)
        {
            try
            {
                // Check if user exists
                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
                if (user == null)
                    return ServiceResult<WalletDto>.Failure("User not found");

                // Check if wallet already exists for this currency
                var existingWallet = await _unitOfWork.Wallets.GetByUserIdAndCurrencyAsync(
                    request.UserId,
                    request.CurrencyCode);

                if (existingWallet != null)
                    return ServiceResult<WalletDto>.Failure(
                        $"Wallet with currency {request.CurrencyCode} already exists");

                var wallet = new Wallet
                {
                    UserId = request.UserId,
                    CurrencyCode = request.CurrencyCode,
                    Balance = 0,
                    DailyLimit = 5000,
                    MonthlyLimit = 20000
                };

                await _unitOfWork.Wallets.AddAsync(wallet);
                await _unitOfWork.SaveChangesAsync();

                var walletDto = _mapper.Map<WalletDto>(wallet);
                return ServiceResult<WalletDto>.Success(walletDto, "Wallet created successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<WalletDto>.Failure($"Error creating wallet: {ex.Message}");
            }
        }

        public async Task<ServiceResult<WalletDto>> GetWalletByIdAsync(Guid walletId)
        {
            try
            {
                var wallet = await _unitOfWork.Wallets.GetByIdAsync(walletId);
                if (wallet == null)
                    return ServiceResult<WalletDto>.Failure("Wallet not found");

                var walletDto = _mapper.Map<WalletDto>(wallet);
                return ServiceResult<WalletDto>.Success(walletDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<WalletDto>.Failure($"Error retrieving wallet: {ex.Message}");
            }
        }

        public async Task<ServiceResult<IEnumerable<WalletDto>>> GetUserWalletsAsync(Guid userId)
        {
            try
            {
                var wallets = await _unitOfWork.Wallets.GetAllByUserIdAsync(userId);
                var walletDtos = _mapper.Map<IEnumerable<WalletDto>>(wallets);
                return ServiceResult<IEnumerable<WalletDto>>.Success(walletDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<WalletDto>>.Failure(
                    $"Error retrieving wallets: {ex.Message}");
            }
        }

        public async Task<ServiceResult<WalletBalanceDto>> GetWalletBalanceAsync(Guid walletId)
        {
            try
            {
                var wallet = await _unitOfWork.Wallets.GetByIdAsync(walletId);
                if (wallet == null)
                    return ServiceResult<WalletBalanceDto>.Failure("Wallet not found");

                var balanceDto = _mapper.Map<WalletBalanceDto>(wallet);
                return ServiceResult<WalletBalanceDto>.Success(balanceDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<WalletBalanceDto>.Failure(
                    $"Error retrieving balance: {ex.Message}");
            }
        }
    }
}
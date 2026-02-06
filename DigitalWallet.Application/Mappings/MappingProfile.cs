using AutoMapper;
using DigitalWallet.Application.DTOs.Admin;
using DigitalWallet.Application.DTOs.Auth;
using DigitalWallet.Application.DTOs.BillPayment;
using DigitalWallet.Application.DTOs.FakeBank;
using DigitalWallet.Application.DTOs.MoneyRequest;
using DigitalWallet.Application.DTOs.Notification;
using DigitalWallet.Application.DTOs.Transaction;
using DigitalWallet.Application.DTOs.Transfer;
using DigitalWallet.Application.DTOs.Wallet;
using DigitalWallet.Domain.Entities;

namespace DigitalWallet.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //// User
            //CreateMap<User, UserDto>();

            CreateMap<User, UserManagementDto>()
                .ForMember(dest => dest.KycLevel, opt => opt.MapFrom(src => src.KycLevel.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<RegisterRequestDto, User>();

            // Wallet mappings
            CreateMap<Wallet, WalletDto>();
            CreateMap<Wallet, WalletBalanceDto>();
            CreateMap<CreateWalletRequestDto, Wallet>();

            CreateMap<Wallet, WalletManagementDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<Domain.Entities.Transaction, TransactionDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Domain.Entities.Transaction, TransactionHistoryDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Transfer mappings
            CreateMap<Transfer, TransferDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Transfer, TransferResponseDto>()
                .ForMember(dest => dest.TransferId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ReceiverName, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.TransferredAt, opt => opt.MapFrom(src => src.CreatedAt));

            // Money Request mappings
            CreateMap<MoneyRequest, MoneyRequestDto>()
                .ForMember(dest => dest.FromUserName, opt => opt.MapFrom(src => src.FromUser.FullName))
                .ForMember(dest => dest.ToUserName, opt => opt.MapFrom(src => src.ToUser.FullName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Fake Bank mappings
            CreateMap<FakeBankTransaction, FakeBankTransactionDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Bill Payment mappings
            CreateMap<Biller, BillerDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()));

            CreateMap<BillPayment, BillPaymentDto>()
                .ForMember(dest => dest.BillerName, opt => opt.MapFrom(src => src.Biller.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Notification mappings
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            // Fraud Log mappings
            CreateMap<FraudLog, FraudLogDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        }
    }
}
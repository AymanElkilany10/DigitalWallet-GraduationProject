namespace DigitalWallet.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IWalletRepository Wallets { get; }
        ITransactionRepository Transactions { get; }
        ITransferRepository Transfers { get; }
        IMoneyRequestRepository MoneyRequests { get; }
        IOtpCodeRepository OtpCodes { get; }
        IFakeBankAccountRepository FakeBankAccounts { get; }
        IFakeBankTransactionRepository FakeBankTransactions { get; }
        IBillerRepository Billers { get; }
        IBillPaymentRepository BillPayments { get; }
        INotificationRepository Notifications { get; }
        IFraudLogRepository FraudLogs { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
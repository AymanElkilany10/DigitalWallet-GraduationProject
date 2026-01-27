namespace DigitalWallet.Domain.Enums
{
    public enum FraudType
    {
        SuspiciousTransfer = 1,
        TooManyAttempts = 2,
        LoginWarning = 3,
        UnusualAmount = 4
    }
}
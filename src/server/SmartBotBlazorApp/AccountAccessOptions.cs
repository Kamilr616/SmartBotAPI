namespace SmartBotBlazorApp;

public sealed class AccountAccessOptions
{
    public const string SectionName = "AccountAccess";

    public bool AllowRegistration { get; set; }

    public bool ShowSelfConfirmationLink { get; set; }
}

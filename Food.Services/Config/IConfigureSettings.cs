using Food.Services.Config.Sections;

namespace Food.Services.Config
{
    public interface IConfigureSettings
    {
        Email Email { get; set; }
        AuthToken AuthToken { get; set; }
        AppLogging AppLogging { get; set; }
        Feedback Feedback { get; set; }
        ConnectionStrings ConnectionStrings { get; set; }
        string Environment { get; set; }
        string SiteName { get; set; }
        string ClientApplication { get; set; }
        string ClientDomain { get; set; }
        string NotificationServiceSms { get; set; }
        string NotificationServiceSmsSecretKey { get; set; }
        string NotificationServiceSmsApplicationId { get; set; }
    }
}
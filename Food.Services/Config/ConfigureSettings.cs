using Food.Services.Config.Sections;

namespace Food.Services.Config
{
    public class ConfigureSettings : IConfigureSettings
    {
        public AuthToken AuthToken { get; set; }
        public Email Email { get; set; }
        public AppLogging AppLogging { get; set; }
        public Feedback Feedback { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
        public string Environment { get; set; }
        public string SiteName { get; set; }
        public string ClientApplication { get; set; }
        public string ClientDomain { get; set; }
        public string NotificationServiceSms { get; set; }
        public string NotificationServiceSmsSecretKey { get; set; }
        public string NotificationServiceSmsApplicationId { get; set; }
    }
}

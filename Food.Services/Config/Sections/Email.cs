namespace Food.Services.Config.Sections
{
    public class Email
    {
        private string _fromAddress;

        public string Host { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public string DisplayName { get; set; }
        public string FromAddress {
            get => _fromAddress ?? Login;
            set => _fromAddress = value;
        }
    }
}

using Microsoft.Extensions.Configuration;

namespace SmartBotBlazorApp.Services
{
    public class SignalRService
    {
        private readonly string _hubUrl;

        public SignalRService(IConfiguration configuration)
        {
            // Pobierz URL z konfiguracji
            _hubUrl = configuration["SignalR:HubUrl"] ?? throw new ArgumentNullException("HubUrl not found in configuration");
        }

        public string GetHubUrl()
        {
            return _hubUrl;
        }
    }

}


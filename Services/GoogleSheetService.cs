
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;


namespace BotBaoKhach.Services
{
    public interface IGoogleSheetService
    {
        Task<SheetsService> GetServiceAsync(string credentialsUrl);
    }

    public class GoogleSheetService : IGoogleSheetService
    {
        private SheetsService? _service;
        private string? _currentCredentialsUrl;

        public async Task InitializeAsync(string credentialsUrl)
        {
            if (_service != null && _currentCredentialsUrl == credentialsUrl)
            {
                return;
            }

            using var httpClient = new HttpClient();
            string credentialsJson = await httpClient.GetStringAsync(credentialsUrl);

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credentialsJson));
            var credential = GoogleCredential.FromStream(stream)
                .CreateScoped(SheetsService.Scope.Spreadsheets);

            _service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Sheets API .NET"
            });

            _currentCredentialsUrl = credentialsUrl;
        }

        public async Task<SheetsService> GetServiceAsync(string credentialsUrl)
        {
            if (_service == null || _currentCredentialsUrl != credentialsUrl)
            {
                await InitializeAsync(credentialsUrl);
            }
            return _service!;
        }
    }


}

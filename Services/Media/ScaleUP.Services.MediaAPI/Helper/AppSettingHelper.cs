using Microsoft.Extensions.Configuration;

namespace ScaleUP.Services.MediaAPI.Helper
{
    public static class AppSettingHelper
    {
        private static IConfigurationRoot _configuration;
        static AppSettingHelper() {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public static string? ThisHostBaseURL()
        {
            return _configuration.GetSection("AppSettings").GetSection("ThisHostBaseURL").Value;
        }
    }
}

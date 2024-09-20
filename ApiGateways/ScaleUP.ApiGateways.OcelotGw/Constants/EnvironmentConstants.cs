namespace ScaleUP.ApiGateways.OcelotGw.Constants
{
    internal static class EnvironmentConstants
    {
        internal static string RabbitMQUrl = Environment.GetEnvironmentVariable("RabbitMQUrl");
        internal static string RabbitUser = Environment.GetEnvironmentVariable("RabbitUser");
        internal static string RabbitPwd = Environment.GetEnvironmentVariable("RabbitPwd");
        internal static string RabbitVHost = Environment.GetEnvironmentVariable("RabbitVHost");
        internal static string RabbitPort = Environment.GetEnvironmentVariable("RabbitPort");
        internal static string EnvironmentName = Environment.GetEnvironmentVariable("EnvironmentName");
        internal static string ElasticUrl = Environment.GetEnvironmentVariable("ElasticUrl");
        internal static string ElasticUser = Environment.GetEnvironmentVariable("ElasticUser");
        internal static string ElasticPwd = Environment.GetEnvironmentVariable("ElasticPwd");
        internal static string CollectorUrl = Environment.GetEnvironmentVariable("CollectorUrl");
        internal static string ASPNETCORE_HTTPS_PORTS = Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORTS");
        internal static string CertLocation = Environment.GetEnvironmentVariable("CertLocation");
        internal static string CertPassword = Environment.GetEnvironmentVariable("CertPassword");
        internal static string JaegerUrl = Environment.GetEnvironmentVariable("JaegerUrl");
        internal static string LokiURL = Environment.GetEnvironmentVariable("LokiURL");
        internal static string IdentityUrl = Environment.GetEnvironmentVariable("IdentityUrl");


    }
}

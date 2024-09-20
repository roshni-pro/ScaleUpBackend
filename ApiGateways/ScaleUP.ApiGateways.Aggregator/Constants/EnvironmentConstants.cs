namespace ScaleUP.ApiGateways.Aggregator.Constants
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
        internal static string ProductUrl = Environment.GetEnvironmentVariable("ProductUrl");
        internal static string CompanyUrl = Environment.GetEnvironmentVariable("CompanyUrl");
        internal static string LocationUrl = Environment.GetEnvironmentVariable("LocationUrl");
        internal static string LeadUrl = Environment.GetEnvironmentVariable("LeadUrl");
        internal static string KYCUrl = Environment.GetEnvironmentVariable("KYCUrl");
        internal static string CommunicationUrl = Environment.GetEnvironmentVariable("CommunicationUrl");
        internal static string IdentityUrl = Environment.GetEnvironmentVariable("IdentityUrl");
        internal static string MediaUrl = Environment.GetEnvironmentVariable("MediaUrl");
        internal static string JaegerUrl = Environment.GetEnvironmentVariable("JaegerUrl");
        internal static string LokiURL = Environment.GetEnvironmentVariable("LokiURL");
        internal static string NBFCUrl = Environment.GetEnvironmentVariable("NBFCUrl");
        internal static string LoanAccountUrl = Environment.GetEnvironmentVariable("LoanAccountUrl");
        internal static string LedgerUrl = Environment.GetEnvironmentVariable("LedgerUrl");
        internal static string GSTURL = Environment.GetEnvironmentVariable("GSTURL");
        internal static string DbContext = Environment.GetEnvironmentVariable("DbContext");
        internal static string AggregatorUrl = Environment.GetEnvironmentVariable("AggregatorUrl");


    }
}

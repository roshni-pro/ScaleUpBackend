using Newtonsoft.Json;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace ScaleUP.Services.LeadDTO.FinBox
{
    public class CreateSessionPost
    {
        public string link_id { get; set; }
        public string api_key { get; set; }

    }
    public class CreateSessionResponse
    {
        public string session_id { get; set; }
        public string redirect_url { get; set; }
        public string code { get; set; }
        public string message { get; set; }
    }

    public class finboxConfig
    {
        public string ApiURL { get; set; }
        public string APIKey { get; set; }
        public string ServerHash { get; set; }
    }

    #region Upload_Session
    public class UploadSessionPost
    {
        public string file { get; set; }
        public string bank_name { get; set; }
        public string session_id { get; set; }
        public string upload_type { get; set; }
        public string pdf_password { get; set; }
    }

    public class DateRange
    {
        public string from_date { get; set; }
        public string to_date { get; set; }
    }

    public class ExtractedDateRange
    {
        public string from_date { get; set; }
        public string to_date { get; set; }
    }

    public class Identity
    {
        public string account_number { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string ifsc { get; set; }
        public string micr { get; set; }
        public string account_category { get; set; }
        public int credit_limit { get; set; }
        public int od_limit { get; set; }
        public string account_id { get; set; }
        public string bank_name { get; set; }
    }

    public class MetadataAnalysis
    {
        public List<object> name_matches { get; set; }
    }

    public class UploadSessionResponse
    {
        public string bank_name { get; set; }
        public string statement_id { get; set; }
        public int page_count { get; set; }
        public Identity identity { get; set; }
        public DateRange date_range { get; set; }
        public object opening_date { get; set; }
        public object opening_bal { get; set; }
        public object closing_bal { get; set; }
        public bool is_fraud { get; set; }
        public object fraud_type { get; set; }
        public MetadataAnalysis metadata_analysis { get; set; }
        public string country_code { get; set; }
        public string currency_code { get; set; }
        public ExtractedDateRange extracted_date_range { get; set; }
        public string account_id { get; set; }
        public string masked_account_number { get; set; }
        public List<string> months { get; set; }
        public List<string> missing_months { get; set; }
        public int status { get; set; }
        public string session_id { get; set; }

        //Error
        public string code { get; set; }
        public string message { get; set; }
    }

    public class ErrorRes
    {
        public Error error { get; set; }
    }
    public class Error
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class UploadSessionError
    {
        public string session_id { get; set; }
        public string statement_id { get; set; }
        public Error error { get; set; }
    }
    #endregion





    #region session_upload_status
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Account
    {
        public string account_id { get; set; }
        public string account_number { get; set; }
        public string bank_name { get; set; }
        public string account_status { get; set; }
        public List<object> months { get; set; }
        public string created_at { get; set; }
        public string last_updated_at { get; set; }
        public List<Statement> statements { get; set; }
    }

    public class SessionUploadStatusResponse
    {
        public string session_id { get; set; }
        public SessionDateRange session_date_range { get; set; }
        public string upload_status { get; set; }
        public List<Account> accounts { get; set; }

        public string code { get; set; }
        public string message { get; set; }
    }

    public class SessionDateRange
    {
        public string from_date { get; set; }
        public string to_date { get; set; }
    }

    public class Statement
    {
        public string statement_id { get; set; }
        public string statement_status { get; set; }
        public string error_code { get; set; }
        public string error_message { get; set; }
        public string source { get; set; }
        public StatementDateRange statement_date_range { get; set; }
        public DateTime created_at { get; set; }
    }

    public class StatementDateRange
    {
        public string from_date { get; set; }
        public string to_date { get; set; }
    }


    #endregion

    #region initiate_processing
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class InitiateProcessingResponse
    {
        public string session_id { get; set; }
        public string message { get; set; }

        public string code { get; set; }
    }
    #endregion

    #region progress_status
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Progress
    {
        public string identity_status { get; set; }
        public string transaction_status { get; set; }
        public string processing_status { get; set; }
        public string fraud_status { get; set; }
        public string statement_id { get; set; }
        public object message { get; set; }
        public string source { get; set; }
    }

    public class ProcessingStatusResponse
    {
        public string session_id { get; set; }
        public string session_progress { get; set; }
        public List<Progress> progress { get; set; }
        public string code { get; set; }
        public string message { get; set; }

    }
    #endregion

    #region session_status

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class SessionAccountStatus
    {
        public string bank_name { get; set; }
        public object account_number { get; set; }
        public string account_id { get; set; }
        public string account_status { get; set; }
        public object error_code { get; set; }
        public object error_message { get; set; }
        public object created_at { get; set; }
        public object last_updated_at { get; set; }
        public List<SessionStatementStatus> statements { get; set; }
    }

    public class SessionStatusResponse
    {
        public string session_id { get; set; }
        public List<SessionAccountStatus> accounts { get; set; }
        public string code { get; set; }
        public string message { get; set; }
    }

    public class SessionStatementStatus
    {
        public object statement_id { get; set; }
        public object statement_status { get; set; }
        public object error_code { get; set; }
        public object error_message { get; set; }
        public object source { get; set; }
        public object created_at { get; set; }
    }

    #endregion

    #region Insights

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AccountInsite
    {
        public string account_id { get; set; }
        public Data data { get; set; }
    }

    public class AccountDetails
    {
        public string account_category { get; set; }
        public string account_number { get; set; }
        public object account_opening_date { get; set; }
        public string bank { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string credit_limit { get; set; }
        public object ifsc { get; set; }
        public object micr { get; set; }
        public List<MissingDatum> missing_data { get; set; }
        public string od_limit { get; set; }
        public object salary_confidence { get; set; }
        public List<string> statements { get; set; }
        public List<string> months { get; set; }
        public string country_code { get; set; }
        public string currency_code { get; set; }
        public MetadataAnalysis metadata_analysis { get; set; }
    }

    public class ClosingBalance
    {
        [JsonProperty("Oct-2023")]
        public double Oct2023 { get; set; }

        [JsonProperty("Nov-2023")]
        public double Nov2023 { get; set; }

        [JsonProperty("Dec-2023")]
        public double Dec2023 { get; set; }

        [JsonProperty("Jan-2024")]
        public double Jan2024 { get; set; }

        [JsonProperty("Feb-2024")]
        public double Feb2024 { get; set; }
    }

    public class Data
    {
        public AccountDetails account_details { get; set; }
        public List<Fraud> fraud { get; set; }
        public List<Transaction> transactions { get; set; }
        public List<SalaryTransaction> salary_transactions { get; set; }
        public List<TopCreditsDebit> top_credits_debits { get; set; }
        public MonthlyAnalysis monthly_analysis { get; set; }
        public Predictors predictors { get; set; }
        public EodBalances eod_balances { get; set; }
        public string xlsx_report_url { get; set; }
        public string xml_report_url { get; set; }
        public string month { get; set; }
        public List<Datum> data { get; set; }
        public string transaction_type { get; set; }
        public string transaction_note { get; set; }
        public object chq_num { get; set; }
        public double amount { get; set; }
        public double balance { get; set; }
        public string date { get; set; }
        public string hash { get; set; }
        public string category { get; set; }
    }

    public class EodBalances
    {
        public List<string> Months_order { get; set; }
        public List<string> start_date { get; set; }

        [JsonProperty("Nov-23")]
        public List<double> Nov23 { get; set; }
    }

    public class Fraud
    {
        public string statement_id { get; set; }
        public string fraud_type { get; set; }
        public object transaction_hash { get; set; }
        public string fraud_category { get; set; }
    }



    public class MissingDatum
    {
        public string from_date { get; set; }
        public string to_date { get; set; }
    }

    public class MonthlyAnalysis
    {
        public OpeningBalance opening_balance { get; set; }
        public ClosingBalance closing_balance { get; set; }
    }

    public class OpeningBalance
    {
        [JsonProperty("Oct-2023")]
        public double Oct2023 { get; set; }

        [JsonProperty("Nov-2023")]
        public double Nov2023 { get; set; }

        [JsonProperty("Dec-2023")]
        public double Dec2023 { get; set; }

        [JsonProperty("Jan-2024")]
        public double Jan2024 { get; set; }

        [JsonProperty("Feb-2024")]
        public double Feb2024 { get; set; }
    }

    public class Predictors
    {
        public string accountnumber { get; set; }
        public string bank_name { get; set; }
        public string ifsc_code { get; set; }
        public string customer_name { get; set; }
        public string account_type { get; set; }
        public string ccod_limit { get; set; }
        public string month_0 { get; set; }
        public string month_1 { get; set; }
        public string month_2 { get; set; }
        public string month_3 { get; set; }
        public string month_4 { get; set; }
        public string month_5 { get; set; }
        public string month_6 { get; set; }
        public string month_7 { get; set; }
        public string month_8 { get; set; }
        public string month_9 { get; set; }
        public string month_10 { get; set; }
        public string month_11 { get; set; }
        public string expense_0 { get; set; }
        public string expense_1 { get; set; }
        public string expense_2 { get; set; }
        public string expense_3 { get; set; }
        public string expense_4 { get; set; }
        public string expense_5 { get; set; }
        public string expense_6 { get; set; }
        public string expense_7 { get; set; }
        public string expense_8 { get; set; }
        public string expense_9 { get; set; }
        public string expense_10 { get; set; }
        public string expense_11 { get; set; }
        public string total_inward_chq_bounces_insuff_fund_0 { get; set; }
    }

    public class InsightsResponse
    {
        public string session_id { get; set; }
        public List<AccountInsite> accounts { get; set; }
        public string message { get; set; }
        public string code { get; set; }
    }

    public class SalaryTransaction
    {
        public string transaction_type { get; set; }
        public string transaction_note { get; set; }
        public object chq_num { get; set; }
        public double amount { get; set; }
        public double balance { get; set; }
        public string date { get; set; }
        public string hash { get; set; }
        public string category { get; set; }
        public object employer_name { get; set; }
        public string salary_month { get; set; }
    }

    public class TopCreditsDebit
    {
        public string type { get; set; }
        public List<Datum> data { get; set; }
    }

    public class Transaction
    {
        public string transaction_type { get; set; }
        public string transaction_note { get; set; }
        public object chq_num { get; set; }
        public double amount { get; set; }
        public double balance { get; set; }
        public string date { get; set; }
        public string hash { get; set; }
        public string category { get; set; }
    }




    #endregion


}

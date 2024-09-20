using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class KarzaPanOcrResDTO
    {
        public string requestId { get; set; }
        public List<Result2> result { get; set; }
        public int statusCode { get; set; }
        public string error { get; set; }
        public KarzaPANDTO OtherInfo { get; set; }
        //KarzaPANDTO
    }
    public class Result2
    {
        public Details details { get; set; }
        public string type { get; set; }
    }
    public class Details
    {
        public Date date { get; set; }
        public DateOfIssue dateOfIssue { get; set; }
        public Father father { get; set; }
        public Name name { get; set; }
        public PanNo panNo { get; set; }
        public string? pan_type { get; set; }
        public bool? id_scanned { get; set; }
        public bool? minor { get; set; }
    }
    public class Date
    {
        public string value { get; set; }
    }
    public class DateOfIssue
    {
        public string value { get; set; }
    }

    public class Father
    {
        public string value { get; set; }
    }

    public class Name
    {
        public string value { get; set; }
    }

    public class PanNo
    {
        public string value { get; set; }
    }

    public class PanOcrOtherInfoDc
    {
        public int? age { get; set; }
        public string date_of_birth { get; set; }
        public string date_of_issue { get; set; }
        public string fathers_name { get; set; }
        public string id_number { get; set; }
        public bool id_scanned { get; set; }
        public bool minor { get; set; }
        public string name_on_card { get; set; }
        public string pan_type { get; set; }
    }

    public class OcrPostDc
    {
        public string url { get; set; }
        public string maskAadhaar { get; set; }
        public string hideAadhaar { get; set; }
        public bool conf { get; set; }
        public string docType { set; get; }
    }

    public class KarzaReultDTO
    {
        public ValidAuthenticationPanResDTO validAuthenticationPanResDTO { get; set; }
        public KarzaPanOcrResDTO karzaPanOcrResDTO { get; set; }
    }

}

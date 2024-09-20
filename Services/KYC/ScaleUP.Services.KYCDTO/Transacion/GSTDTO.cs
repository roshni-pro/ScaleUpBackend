using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.KYCDTO.Transacion
{
    public class AppyFlowGSTDTO
    {
        public string BusinessName { get; set; }
        public string LandingName { get; set; }
        public string Address { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string ZipCode { get; set; }
        public string GSTNumber { get; set; }
    }
    public class AppyFlowGSTResDTO
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public AppyFlowGSTDTO GSTInfo { get; set; }
    }
    public class AppyFlowGSTResponseDTO
    {
        public bool error { get; set; }
        public string message { get; set; }
        public Compliance compliance { get; set; }
        public TaxpayerInfo taxpayerInfo { get; set; }
    }
    public class Compliance
    {
        public object filingFrequency { get; set; }
    }
    public class TaxpayerInfo
    {
        public string gstin { get; set; }
        public string tradeNam { get; set; }
        public string dty { get; set; }
        public string sts { get; set; }
        public List<string> nba { get; set; }
        public string lstupdt { get; set; }
        public string ctjCd { get; set; }
        public string ctb { get; set; }
        public string ctj { get; set; }
        public string rgdt { get; set; }
        public Pradr pradr { get; set; }
        public string stjCd { get; set; }
        public string cxdt { get; set; }
        public List<object> adadr { get; set; }
        public string lgnm { get; set; }
        public string stj { get; set; }
    }
    public class Pradr
    {
        public Addr addr { get; set; }
        public string ntr { get; set; }
    }
    public class Addr
    {
        public string st { get; set; }
        public string lt { get; set; }
        public string bnm { get; set; }
        public string lg { get; set; }
        public string stcd { get; set; }
        public string bno { get; set; }
        public string loc { get; set; }
        public string flno { get; set; }
        public string city { get; set; }
        public string dst { get; set; }
        public string pncd { get; set; }
    }
}

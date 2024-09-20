using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.ApiGateways.AggregatorDTO.ConsumerAppHome
{

    #region new DCs
    public class AppHomeDC
    {
        public long AppHomeId { get; set; }
        public string AppHomeHeading { get; set; }
        public List<AppHomeItemList> appHomeItemLists { get; set; }
    }

    public class AppHomeItemList
    {
        public long AppHomeItemId { get; set; }
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public List<AppHomeItemContent> AppHomeItemContent { get; set; }
    }

    public class AppHomeItemContent
    {
        public long AppHomeItemContentId { get; set; }
        public string ImageUrl { get; set; }
        public int Sequence { get; set; }
        public string CallBackUrl { get; set; }
        public long? AppHomeFnId { get; set; }
    }

    public class AppHomelist
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    #endregion

















    #region old DCs

    public class AppHomeInput
    {
        public string Name { get; set; }
        public long? AppHomeFunctionId { get; set; }
        public string CallBackUrl { get; set; }
        public string ImageUrl { get; set; }
        public string ItemType { get; set; }
    }

    public class AppHomeReturnList
    {
        public string Name { get; set; }
        public long? AppHomeFunctionId { get; set; }
        public string? CallBackUrl { get; set; }
        public string ImageUrl { get; set; }
        public string ItemType { get; set; }
        public string? FunctionName { get; set; }

    }

    public class AppHomeFunctionListDc
    {
        public long AppHomeFunctionId { get; set; }
        public string FunctionName { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
    }

    #endregion

}

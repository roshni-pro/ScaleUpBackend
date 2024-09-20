using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _2sept2024_GetTATData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

Create or ALTER Procedure [dbo].[GetTATData]
		--declare
		@ProductType varchar(max) = '',
		@CityId dbo.Intvalues readonly,        
		@AnchorId dbo.Intvalues readonly,
		@FromDate datetime ='2024-01-01',        
		@ToDate datetime ='2024-08-09'	
		--insert into @AnchorId values(2)
		--insert into @CityId values(0)
As
Begin
			if(OBJECT_ID('tempdb..#LeadActivityPrgress') is not null) 
		drop table #LeadActivityPrgress  
		select l.Id,l.Created initiateDate,l.SubmittedDate
		,lp.LastModified as LastModifiedDate,lp.ActivityMasterName,lp.SubActivityMasterName,lp.IsActive,lp.IsDeleted,lp.IsCompleted,lp.IsApproved
	   ,l.Status,l.ProductId,l.ProductCode into #LeadActivityPrgress  
		from Leads l with(nolock)
		join LeadActivityMasterProgresses lp with(nolock) on l.Id=lp.LeadMasterId  and l.IsActive =1 and l.IsDeleted=0
		inner join CompanyLead cl with(nolock) on l.Id = cl.LeadId  
   	    inner join @AnchorId a on  a.IntValue = cl.CompanyId  
		inner join @CityId c on l.CityId = (case when c.IntValue=0 or c.IntValue=null then l.CityId else c.IntValue end)
		where ( cast(l.Created as date) between cast(@FromDate as date) and  cast(@ToDate as date))
  if(OBJECT_ID('tempdb..#temp') is not null)
   drop table #temp  	   
	  SELECT l.Id
	  ,l.Status,l.ProductId,l.ProductCode,l.initiateDate initiateDate,l.SubmittedDate SubmittedDate
	  --,case when (l.ActivityMasterName in('KYC','PersonalInfo','BusinessInfo','Bank Detail','MSME') and l.IsCompleted =1) then max(l.LastModifiedDate) end as SubmittedDate

	  ,case when (l.ActivityMasterName in('KYC','PersonalInfo','BusinessInfo','Bank Detail','MSME') and l.SubActivityMasterName in('Pan','Selfie','Aadhar') and l.IsApproved=1 and l.IsCompleted =1  and l.IsApproved =1) then max(l.LastModifiedDate)
	  end as AllApprovedDate
	  ,case when (l.ActivityMasterName in('Inprogress')) and l.IsCompleted=1 then max(l.LastModifiedDate) end as SendToLosDate
	  ,case when (l.ActivityMasterName in('Show Offer') or l.ActivityMasterName in ('Arthmate Show Offer')) and l.IsCompleted=1 then max(l.LastModifiedDate) end as OfferDate
	  ,case when l.SubActivityMasterName in('PrepareAgreement') and l.IsCompleted =1 then max(l.LastModifiedDate) end as PrepareAgreementDate
	  ,case when l.SubActivityMasterName in('AgreementEsign') and l.IsCompleted =1 then max(l.LastModifiedDate) end as AgreementAcceptDate
	  ,ActivityMasterName,IsCompleted,IsApproved,LastModifiedDate,l.SubActivityMasterName
	  into #temp  
	  from #LeadActivityPrgress l
	  group by l.Id,l.initiateDate,l.SubmittedDate
	  ,l.Status,l.ProductId,l.ProductCode,l.LastModifiedDate,l.IsCompleted,l.ActivityMasterName,l.SubActivityMasterName,l.IsApproved
	if(OBJECT_ID('tempdb..#temp1') is not null)
   drop table #temp1 
	 select tt.Id,tt.initiateDate,tt.ProductCode,tt.ProductId,tt.Status
	 --,max(tt.SubmittedDate) SubmittedDate
	 ,tt.SubmittedDate SubmittedDate
	 --,MAX(tt.AllApprovedDate) AllApprovedDate,MAX(SendToLosDate) SendToLosDate,MAX(OfferDate) OfferDate,MAX(PrepareAgreementDate) PrepareAgreementDate, MAX(AgreementAcceptDate) AgreementAcceptDate
	 , CASE 
        WHEN 
            SUM(CASE 
                    WHEN tt.ActivityMasterName IN ('KYC', 'PersonalInfo', 'BusinessInfo', 'Bank Detail') 
                         AND tt.IsApproved = 1 
                         AND tt.IsCompleted = 1 
                    THEN 1 
                    ELSE 0 
                END) = 
            SUM(CASE 
                    WHEN tt.ActivityMasterName IN ('KYC', 'PersonalInfo', 'BusinessInfo', 'Bank Detail') 
                    THEN 1 
                    ELSE 0 
                END)
        THEN 
            MAX(CASE 
                    WHEN tt.ActivityMasterName IN ('KYC', 'PersonalInfo', 'BusinessInfo', 'Bank Detail') 
                         AND tt.IsApproved = 1 
                         AND tt.IsCompleted = 1 
                    THEN tt.LastModifiedDate 
                    ELSE NULL 
                END)
        ELSE NULL 
    END AS AllApprovedDate
	 --,MAX(SendToLosDate) SendToLosDate,MAX(OfferDate) OfferDate,MAX(PrepareAgreementDate) PrepareAgreementDate, MAX(AgreementAcceptDate) AgreementAcceptDate
	 ,CASE 
        WHEN 
            SUM(CASE 
                    WHEN tt.ActivityMasterName = 'Inprogress' 
                         AND tt.IsCompleted = 1 
                    THEN 1 
                    ELSE 0 
                END) = 
            SUM(CASE 
                    WHEN tt.ActivityMasterName = 'Inprogress' 
                    THEN 1 
                    ELSE 0 
                END)
        THEN 
            MAX(CASE 
                    WHEN tt.ActivityMasterName = 'Inprogress' 
                         AND tt.IsCompleted = 1 
                    THEN tt.LastModifiedDate 
                    ELSE NULL 
                END)
        ELSE NULL 
    END AS SendToLosDate,
    
    -- OfferDate: When the activity is 'Show Offer' or 'Arthmate Show Offer' and completed
    CASE 
        WHEN 
            SUM(CASE 
                    WHEN tt.ActivityMasterName IN ('Show Offer', 'Arthmate Show Offer') 
                         AND tt.IsCompleted = 1 
                    THEN 1 
                    ELSE 0 
                END) = 
            SUM(CASE 
                    WHEN tt.ActivityMasterName IN ('Show Offer', 'Arthmate Show Offer') 
                    THEN 1 
                    ELSE 0 
                END)
        THEN 
            MAX(CASE 
                    WHEN tt.ActivityMasterName IN ('Show Offer', 'Arthmate Show Offer') 
                         AND tt.IsCompleted = 1 
                    THEN tt.LastModifiedDate 
                    ELSE NULL 
                END)
        ELSE NULL 
    END AS OfferDate,
    
    -- PrepareAgreementDate: When the sub-activity is 'PrepareAgreement' and completed
    CASE 
        WHEN 
            SUM(CASE 
                    WHEN tt.SubActivityMasterName = 'PrepareAgreement' 
                         AND tt.IsCompleted = 1 
                    THEN 1 
                    ELSE 0 
                END) = 
            SUM(CASE 
                    WHEN tt.SubActivityMasterName = 'PrepareAgreement' 
                    THEN 1 
                    ELSE 0 
                END)
        THEN 
            MAX(CASE 
                    WHEN tt.SubActivityMasterName = 'PrepareAgreement' 
                         AND tt.IsCompleted = 1 
                    THEN tt.LastModifiedDate 
                    ELSE NULL 
                END)
        ELSE NULL 
    END AS PrepareAgreementDate,
    
    -- AgreementAcceptDate: When the sub-activity is 'AgreementEsign' and completed
    CASE 
        WHEN 
            SUM(CASE 
                    WHEN tt.SubActivityMasterName = 'AgreementEsign' 
                         AND tt.IsCompleted = 1 
                    THEN 1 
                    ELSE 0 
                END) = 
            SUM(CASE 
                    WHEN tt.SubActivityMasterName = 'AgreementEsign' 
                    THEN 1 
                    ELSE 0 
                END)
        THEN 
            MAX(CASE 
                    WHEN tt.SubActivityMasterName = 'AgreementEsign' 
                         AND tt.IsCompleted = 1 
                    THEN tt.LastModifiedDate 
                    ELSE NULL 
                END)
        ELSE NULL 
    END AS AgreementAcceptDate
	 into #temp1
	 from #temp tt
	 group by tt.Id,tt.initiateDate,tt.SubmittedDate
	 ,tt.ProductCode,tt.ProductId,tt.Status
SELECT 
    isnull(CAST(FLOOR(InitiateSubmittedTime / (24 * 60)) AS VARCHAR(100)) + ' days ' + 
    CAST(FLOOR((InitiateSubmittedTime % (24 * 60)) / 60) AS VARCHAR(100)) + ' hours ' + 
    CAST(FLOOR(InitiateSubmittedTime % 60) AS VARCHAR(100)) + ' minutes' ,'') AS InitiateSubmittedTime,
    isnull(CAST(FLOOR(SubmittedToAllApprovedTime / (24 * 60)) AS VARCHAR(100)) + ' days ' + 
    CAST(FLOOR((SubmittedToAllApprovedTime % (24 * 60)) / 60) AS VARCHAR(100)) + ' hours ' + 
    CAST(FLOOR(SubmittedToAllApprovedTime % 60) AS VARCHAR(100)) + ' minutes','') AS SubmittedToAllApprovedTime,
    isnull(CAST(FLOOR(AllApprovedToSendToLosTime / (24 * 60)) AS VARCHAR(100)) + ' days ' + 
    CAST(FLOOR((AllApprovedToSendToLosTime % (24 * 60)) / 60) AS VARCHAR(100)) + ' hours ' + 
    CAST(FLOOR(AllApprovedToSendToLosTime % 60) AS VARCHAR(100)) + ' minutes','') AS AllApprovedToSendToLosTime,
    isnull(CAST(FLOOR(SendToLosToOfferAcceptTime / (24 * 60)) AS VARCHAR(100)) + ' days ' + 
    CAST(FLOOR((SendToLosToOfferAcceptTime % (24 * 60)) / 60) AS VARCHAR(100)) + ' hours ' + 
    CAST(FLOOR(SendToLosToOfferAcceptTime % 60) AS VARCHAR(100)) + ' minutes','') AS SendToLosToOfferAcceptTime,
    isnull(CAST(FLOOR(OfferAcceptToAgreementTime / (24 * 60)) AS VARCHAR(100)) + ' days ' + 
    CAST(FLOOR((OfferAcceptToAgreementTime % (24 * 60)) / 60) AS VARCHAR(100)) + ' hours ' + 
    CAST(FLOOR(OfferAcceptToAgreementTime % 60) AS VARCHAR(100)) + ' minutes','') AS OfferAcceptToAgreementTime,
    isnull(CAST(FLOOR(AgreementToAgreementAcceptTime / (24 * 60)) AS VARCHAR(100)) + ' days ' + 
    CAST(FLOOR((AgreementToAgreementAcceptTime % (24 * 60)) / 60) AS VARCHAR(100)) + ' hours ' + 
    CAST(FLOOR(AgreementToAgreementAcceptTime % 60) AS VARCHAR(100)) + ' minutes','') AS AgreementToAgreementAcceptTime
    ,isnull(cast(InitiateSubmittedTime / 60  as float) ,0) AS InitiateSubmittedTimeInHours,
	isnull(cast(SubmittedToAllApprovedTime / 60 as float),0) AS SubmittedToAllApprovedTimeInHours,
	isnull(cast(AllApprovedToSendToLosTime / 60 as float),0) AS AllApprovedToSendToLosTimeInHours,
	isnull(cast(SendToLosToOfferAcceptTime / 60 as float),0) AS SendToLosToOfferAcceptTimeInHours,
	isnull(cast(OfferAcceptToAgreementTime / 60 as float),0) AS OfferAcceptToAgreementTimeInHours,
	isnull(cast(AgreementToAgreementAcceptTime / 60 as float),0) AS AgreementToAgreementAcceptTimeInHours
FROM
(
    SELECT 
        AVG(CAST(DATEDIFF(minute, initiateDate, SubmittedDate) AS NUMERIC(19,2))) AS InitiateSubmittedTime,
        AVG(CAST(DATEDIFF(minute, SubmittedDate, AllApprovedDate) AS NUMERIC(19,2))) AS SubmittedToAllApprovedTime,
        AVG(CAST(DATEDIFF(minute, AllApprovedDate, SendToLosDate) AS NUMERIC(19,2))) AS AllApprovedToSendToLosTime,
        AVG(CAST(DATEDIFF(minute, SendToLosDate, OfferDate) AS NUMERIC(19,2))) AS SendToLosToOfferAcceptTime,
        AVG(CAST(DATEDIFF(minute, OfferDate, PrepareAgreementDate) AS NUMERIC(19,2))) AS OfferAcceptToAgreementTime,
        AVG(CAST(DATEDIFF(minute, PrepareAgreementDate, AgreementAcceptDate) AS NUMERIC(19,2))) AS AgreementToAgreementAcceptTime
    FROM #temp1
) t;
End	 ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

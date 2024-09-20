using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _20mayStampDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER   procedure [dbo].[ScaleupleadDashboardDetails]  
	--declare  
	 @CityId dbo.Intvalues readonly,          
	 @AnchorId dbo.Intvalues readonly,  
	 @FromDate datetime ='2023-12-01',          
	 @ToDate datetime ='2024-05-13',  
	 @ProductId bigint =2
as  
begin   
--insert into @CityId values(1)       
--insert into @AnchorId values(2)  

	 declare  
	 @TotalLeads int,  
	 @ApprovedLeads int,  
	 @RejectedLeads int,  
	 @PendingLeads int,  @AvgValue int   
	 if(OBJECT_ID('tempdb..#temp') is not null) drop table #temp  
	 (  
	  SELECT l.Id,l.Created initiateDate,y.LastModified offerDate,l.Status,l.ProductId,l.ProductCode  into #temp  
	  FROM leads l  with(nolock)  
	  inner join CompanyLead cl with(nolock) on l.Id = cl.LeadId  
	  inner join @cityid c on  c.IntValue = l.CityId  
	  inner join @AnchorId a on  a.IntValue = cl.CompanyId  
	  OUTER APPLY(select LastModified from LeadActivityMasterProgresses lap where (lap.ActivityMasterName in('Show Offer') or lap.ActivityMasterName in ('Arthmate Show Offer')) and l.Id = lap.LeadMasterId)y  
	  WHERE NOT EXISTS (SELECT 1 FROM LeadOffers with(nolock) WHERE l.Id = LeadOffers.LeadId)  
	  and l.ProductId = @ProductId  
	  and ( ( cast(l.Created as date) between cast(@FromDate as date) and  cast(@ToDate as date)) )  
	 )  
	 --select * from #temp

		set @AvgValue = (select   
		 avg(isnull(cast(cast ((DATEDIFF(minute, t.initiateDate, t.offerDate)/60) as VARCHAR(100)) +'.'+cast( (DATEDIFF(minute, t.initiateDate, t.offerDate)%60) as VARCHAR(max)) as NUMERIC(19,2)),0)) AS hr_min   
		 from #temp t)  

		set @TotalLeads = (SELECT count(*) from #temp)  

		set @ApprovedLeads =   
		(select count(*) from  
			(  
				SELECT lap.LeadMasterId   
				FROM LeadActivityMasterProgresses lap  with(nolock)  
				inner join #temp temp on temp.Id = lap.LeadMasterId   
				WHERE  
				(
					(temp.ProductCode='CreditLine' AND lap.ActivityMasterName IN ('KYC','PersonalInfo','BusinessInfo','Bank Detail')  )
						or
					(temp.ProductCode='BusinessLoan' AND lap.ActivityMasterName IN ('KYC','PersonalInfo','BusinessInfo','MSME','Bank Detail') )
				) 
				AND lap.IsDeleted = 0  
				group by lap.LeadMasterId  
				HAVING SUM(CASE WHEN lap.IsApproved in (0,2) OR lap.IsCompleted = 0 THEN 1 ELSE 0 END) = 0 
			) t   
		) ;   
 
		set @RejectedLeads =   
		(select count(*) from  
			(   
				SELECT lap.LeadMasterId  
				FROM LeadActivityMasterProgresses lap with(nolock)  
				inner join #temp temp on temp.Id = lap.LeadMasterId  
				WHERE  
				((	temp.ProductCode='CreditLine'  AND (lap.ActivityMasterName IN ('KYC','PersonalInfo','BusinessInfo','Bank Detail') and lap.IsApproved=2)
					or (lap.ActivityMasterName in ('Rejected') and lap.IsActive = 1)	)
				or
				(	temp.ProductCode='BusinessLoan'  AND (lap.ActivityMasterName IN ('KYC','PersonalInfo','BusinessInfo','MSME','Bank Detail') and lap.IsApproved=2)
					or (lap.ActivityMasterName in ('Rejected') and lap.IsActive = 1)	))
				AND lap.IsDeleted = 0  
				group by lap.LeadMasterId    
			) t  
		);   

	 select    
		cast(@ApprovedLeads as int) Approved  
		,cast((@TotalLeads -(@ApprovedLeads+@RejectedLeads))as int) as Pending 
		,cast(@RejectedLeads as int) Rejected  
		,cast(0 as int) NotContactable  
		,cast(0 as int) NotIntrested  
		,case when @ApprovedLeads > 0 then cast((@ApprovedLeads*100)/@TotalLeads  as float) else cast(0 as float) end as ApprovalPercentage  
		,cast(@TotalLeads as int) TotalLeads  
		,isnull(cast( @AvgValue / 24 as int),0) AS WholeDays  
		,isnull(cast( @AvgValue  % 24  as int),0) AS RemainingHours  
end  

Go
CREATE OR ALTER procedure [dbo].[AccountDashboard]  
	--declare  
	 @CityId dbo.Intvalues readonly,          
	 @AnchorId dbo.Intvalues readonly,  
	 @FromDate datetime ='2023-01-01',          
	 @ToDate datetime ='2024-05-017',  
	 @ProductId bigint =1,
	 @ProductType varchar(50) ='BusinessLoan'
as  
begin   
	--  insert into @CityId values(1),(9),(10) ,(11) ,(13) ,(14) ,(17) ,(26) ,(29) ,(31) ,(49) 
	--insert into @AnchorId values(2)
	--insert into @AnchorId values(14) 

	  SELECT l.Id LeadId,l.Created ,l.ProductId,l.Status 
	  FROM leads l  with(nolock)  
	  inner join CompanyLead cl with(nolock) on l.Id = cl.LeadId   
	  inner join @cityid c on  c.IntValue = l.CityId  
	  inner join @AnchorId a on  a.IntValue = cl.CompanyId  
	  and l.ProductId = @ProductId  and l.IsDeleted=0 and l.IsActive=1
	  and ( ( cast(l.Created as date) between cast(@FromDate as date) and  cast(@ToDate as date)) ) 
	  and 
	  (
      (@ProductType = 'BusinessLoan' AND Status IN ('Pending', 'LoanApproved', 'LoanInitiated', 'LoanActivated', 'LoanRejected', 'LoanAvailed'))
      OR (@ProductType = 'CreditLine' AND Status IN ('pending','line_initiated','line_activated','line_approved','line_rejected'))
		)
	 
end 

Go

CREATE OR ALTER   procedure [dbo].[getSlaLbaStampDetailsData]
--declare   
@isStampUsed int = 1,
@skip int =0,
@take int = 10
as  
begin 

	if(@take=0)
	begin
		SELECT ASD.Id,
			ASD.UsedFor,
			ASD.PartnerName,
			ASD.StampAmount,
			ASD.DateofUtilisation AS DateofUtilisation,
			ASD.StampPaperNo,
			ASD.LeadmasterId,
			ASD.IsStampUsed,
			ASD.Created,
			ASD.IsActive,
			ASD.IsDeleted,
			ASD.StampUrl,
			ISNULL(l.ApplicantName, '') AS LeadName,
			ISNULL(l.MobileNo, '') AS MobileNo,
			ISNULL(l.LeadCode, '') AS LeadCode,
			COUNT(ASD.Id) OVER() AS TotalRecord
		FROM ArthmateSlaLbaStampDetail AS ASD WITH (NOLOCK)
		LEFT JOIN Leads AS l WITH (NOLOCK) ON ASD.LeadmasterId = l.Id
		WHERE ASD.IsActive = 1
		AND ASD.IsDeleted = 0
		ORDER BY ASD.Id DESC
	end
	else 
	begin
		SELECT ASD.Id,
			ASD.UsedFor,
			ASD.PartnerName,
			ASD.StampAmount,
			ASD.DateofUtilisation AS DateofUtilisation,
			ASD.StampPaperNo,
			ASD.LeadmasterId,
			ASD.IsStampUsed,
			ASD.Created,
			ASD.IsActive,
			ASD.IsDeleted,
			ASD.StampUrl,
			ISNULL(l.ApplicantName, '') AS LeadName,
			ISNULL(l.MobileNo, '') AS MobileNo,
			ISNULL(l.LeadCode, '') AS LeadCode,
			COUNT(ASD.Id) OVER() AS TotalRecord
		FROM ArthmateSlaLbaStampDetail AS ASD WITH (NOLOCK)
		LEFT JOIN Leads AS l WITH (NOLOCK) ON ASD.LeadmasterId = l.Id
		WHERE ASD.IsActive = 1
		AND ASD.IsDeleted = 0
		AND (
		(@isStampUsed = 1 AND ASD.IsStampUsed = 1) 
		OR (@isStampUsed = 0 AND ASD.IsStampUsed = 0 AND ASD.LeadmasterId = 0) 
		OR (@isStampUsed = 2 AND ASD.IsStampUsed = 0 AND ASD.LeadmasterId <> 0) 
		)
		ORDER BY ASD.Id DESC
		OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;
	end
end


";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

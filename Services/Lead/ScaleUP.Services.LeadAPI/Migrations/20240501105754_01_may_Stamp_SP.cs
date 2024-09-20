using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _01_may_Stamp_SP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"


CREATE OR ALTER procedure [dbo].[getSlaLbaStampDetailsData]
--declare   
@isStampUsed bit = 0,
@skip int =0,
@take int = 10
as  
begin 
if(@take=0)
	begin
		select ASD.Id,
		ASD.UsedFor,ASD.PartnerName,
		ASD.StampAmount,
		ASD.DateofUtilisation as DateofUtilisation,
		ASD.StampPaperNo,
		ASD.LeadmasterId,
		ASD.IsStampUsed,ASD.Created,  
		ASD.IsActive,ASD.IsDeleted,StampUrl,isnull(l.ApplicantName,'') as LeadName, isnull(l.MobileNo,'') as MobileNo,isnull(l.LeadCode,'') as LeadCode
		,count(ASD.Id) over() as TotalRecord
		from ArthmateSlaLbaStampDetail as ASD  with(nolock)  
		left join Leads  as  l with(nolock) on ASD.LeadmasterId=l.Id  
		where  ASD.IsActive=1 and ASD.IsDeleted=0 and IsStampUsed=@isStampUsed 
		order by ASD.Id desc 
	end
else
	begin
		select ASD.Id,
		ASD.UsedFor,ASD.PartnerName,
		ASD.StampAmount,
		ASD.DateofUtilisation as DateofUtilisation,
		ASD.StampPaperNo,
		ASD.LeadmasterId,
		ASD.IsStampUsed,ASD.Created,  
		ASD.IsActive,ASD.IsDeleted,StampUrl,isnull(l.ApplicantName,'') as LeadName, isnull(l.MobileNo,'') as MobileNo,isnull(l.LeadCode,'') as LeadCode
		,count(ASD.Id) over() as TotalRecord
		from ArthmateSlaLbaStampDetail as ASD  with(nolock)  
		left join Leads  as  l with(nolock) on ASD.LeadmasterId=l.Id  
		where  ASD.IsActive=1 and ASD.IsDeleted=0 and IsStampUsed=@isStampUsed 
		order by ASD.Id desc 
		offset @skip Rows Fetch next @take Rows only
	end  
end
GO



CREATE OR ALTER procedure [dbo].[ScaleupleadDashboardDetails]  
	--declare  
	 @CityId dbo.Intvalues readonly,          
	 @AnchorId dbo.Intvalues readonly,  
	 @FromDate datetime ='2024-02-17',          
	 @ToDate datetime ='2024-04-22',  
	 @ProductId bigint =2  
as  
begin   
  
	  --insert into @CityId values(1)   
	  --insert into @AnchorId values(2)  
  
	 declare  
	 @TotalLeads bigint,  
	 @ApprovedLeads bigint,  
	 @RejectedLeads bigint,  
	 @PendingLeads bigint,  @AvgValue int   
   
	 if(OBJECT_ID('tempdb..#temp') is not null) drop table #temp  
	 (  
	  SELECT l.Id,l.Created initiateDate,y.LastModified offerDate into #temp  
	  FROM leads l  with(nolock)  
	  inner join CompanyLead cl with(nolock) on l.Id = cl.LeadId  
	  inner join @cityid c on  c.IntValue = l.CityId  
	  inner join @AnchorId a on  a.IntValue = cl.CompanyId  
	  OUTER APPLY(select LastModified from LeadActivityMasterProgresses lap where (lap.ActivityMasterName in('Show Offer') or lap.ActivityMasterName in ('Arthmate Show Offer')) and l.Id = lap.LeadMasterId)y  
	  WHERE NOT EXISTS (SELECT 1 FROM LeadOffers with(nolock) WHERE l.Id = LeadOffers.LeadId)  
	  and l.ProductId = @ProductId  
	  and ( ( cast(l.Created as date) between cast(@FromDate as date) and  cast(@ToDate as date)) )  
	 )  
  
	 --select  * from #temp  
  
	 --select DATEDIFF(minute, t.initiateDate, t.offerDate)/60 hr, DATEDIFF(minute, t.initiateDate, t.offerDate)%60 AS min,  
	 --isnull(cast(cast ((DATEDIFF(minute, t.initiateDate, t.offerDate)/60) as VARCHAR(100)) +'.'+cast( (DATEDIFF(minute, t.initiateDate, t.offerDate)%60) as VARCHAR(max)) as NUMERIC(19,2)),0) AS hr_min   
	 --from #temp t  
  
		 set @AvgValue = (select   
		 avg(isnull(cast(cast ((DATEDIFF(minute, t.initiateDate, t.offerDate)/60) as VARCHAR(100)) +'.'+cast( (DATEDIFF(minute, t.initiateDate, t.offerDate)%60) as VARCHAR(max)) as NUMERIC(19,2)),0)) AS hr_min   
		 from #temp t)  

		 set @TotalLeads =(SELECT count(*) from #temp)  
  
		set @ApprovedLeads =   
		(select count(*) from  
			(  
				SELECT lap.LeadMasterId   
				FROM LeadActivityMasterProgresses lap  with(nolock)  
				inner join #temp temp on temp.Id = lap.LeadMasterId   
				WHERE   
				lap.ActivityMasterName IN ('KYC','PersonalInfo','BusinessInfo','CreditBureau','Bank Detail','Inprogress')  
				AND (lap.SubActivityMasterName In ('Pan','Aadhar','Selfie') or lap.SubActivityMasterName is null or lap.SubActivityMasterName='' )  
				AND lap.IsActive = 1  
				AND lap.IsDeleted = 0  
				group by lap.LeadMasterId   
				HAVING SUM(CASE WHEN lap.IsApproved = 0 OR lap.IsCompleted = 0 THEN 1 ELSE 0 END) = 0   
			) t   
		) ;  
  
		set @PendingLeads =   
		(select count(*) from  
			(  
				SELECT lap.LeadMasterId   
				FROM LeadActivityMasterProgresses lap  with(nolock)  
				inner join #temp temp on temp.Id = lap.LeadMasterId   
				WHERE   
				lap.ActivityMasterName IN ('KYC','PersonalInfo','BusinessInfo','CreditBureau','Bank Detail','Inprogress')  
				AND (lap.SubActivityMasterName In ('Pan','Aadhar','Selfie') or lap.SubActivityMasterName is null or lap.SubActivityMasterName='' )  
				AND lap.IsActive = 1  
				AND lap.IsDeleted = 0  
				group by lap.LeadMasterId   
				HAVING SUM(CASE WHEN lap.IsApproved = 0 AND lap.IsCompleted = 0 THEN 1 ELSE 0 END) = 1   
			) t   
		) ;  
  
		set @RejectedLeads =   
		(select count(*) from  
			(   
				SELECT lap.LeadMasterId  
				FROM LeadActivityMasterProgresses lap with(nolock)  
				inner join #temp temp on temp.Id = lap.LeadMasterId   
				WHERE   
				lap.ActivityMasterName IN ('KYC','PersonalInfo','BusinessInfo','CreditBureau','Bank Detail','Inprogress')  
				AND (lap.SubActivityMasterName In ('Pan','Aadhar','Selfie') or lap.SubActivityMasterName is null or lap.SubActivityMasterName='' )  
				AND lap.IsActive = 1  
				AND lap.IsDeleted = 0  
				group by lap.LeadMasterId   
				HAVING max(lap.IsApproved) = 2   
			) t  
		);  
  
	 select    
		cast(@ApprovedLeads as int) Approved  
		,cast(@PendingLeads as int) Pending  
		,cast(@RejectedLeads as int) Rejected  
		,cast(0 as int) NotContactable  
		,cast(0 as int) NotIntrested  
		,case when @ApprovedLeads > 0 then cast((@ApprovedLeads*100)/@TotalLeads  as float) else cast(0 as float) end as ApprovalPercentage  
		,cast(@TotalLeads as int) TotalLeads  
		,isnull(cast( @AvgValue / 24 as int),0) AS WholeDays  
		,isnull(cast( @AvgValue  % 24  as int),0) AS RemainingHours  
   
end  
  
GO



";
			migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

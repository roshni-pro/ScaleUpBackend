using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _07StoreProc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"
GO
IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'NBFCCompanyOffer')
BEGIN
/****** Object:  UserDefinedTableType [dbo].[NBFCCompanyOffer]    Script Date: 07-03-2024 18:43:51 ******/
CREATE TYPE [dbo].[NBFCCompanyOffer] AS TABLE(
	[companyId] [bigint] NULL,
	[leadId] [bigint] NULL,
	[VintageDays] [int] NULL,
	[AvgMonthlyBuying] [float] NULL,
	[CibilScore] [int] NULL,
	[CustomerType] [nvarchar](200) NULL
)
END
GO

/****** Object:  StoredProcedure [dbo].[DeleteAllLead]    Script Date: 07-03-2024 18:43:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER Procedure [dbo].[DeleteAllLead]
as
begin
Delete from LeadActivityMasterProgresses where LeadMasterId in (
Select id from Leads where id not in (132,109))

Delete from LeadCompanyBuyingHistory where CompanyLeadId in (
Select b.id from Leads a inner join CompanyLead b on a.id=b.LeadId where a.id not in (132,109))

Delete from CompanyLead where LeadId in (
Select id from Leads where id not in (132,109))

Delete from eNachBankDetails where LeadId in (
Select id from Leads where id not in (132,109))

Delete from leadAgreements where LeadId in (
Select id from Leads where id not in (132,109))

Delete from LeadBankDetails where LeadId in (
Select id from Leads where id not in (132,109))

Delete from LeadCommonRequestResponses where LeadId in (
Select id from Leads where id not in (132,109))

Delete from LeadOffers where LeadId in (
Select id from Leads where id not in (132,109))

Delete from BlackSoilWebhookResponses where LeadId in (
Select id from Leads where id not in (132,109))

Delete from Leads where id not in (132,109)

end
GO
/****** Object:  StoredProcedure [dbo].[eNachCheckPreviousReq]    Script Date: 07-03-2024 18:43:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE Proc [dbo].[eNachCheckPreviousReq]  
 --declare  
 @LeadMasterId bigint=2  
 As  
  Begin  
  If(select top 1 Id from eNachBankDetails where LeadId=@LeadMasterId and IsDeleted=0 and (Created >= dateADD(Minute,-1,getdate()) or LastModified >= dateADD(Minute,-6,getdate())))is not null  
--  If(select top 1 Id from Leads where Id=@LeadMasterId and IsDeleted=0 and (Created >= dateADD(Minute,-6,getdate()) or LastModified >= dateADD(Minute,-6,getdate())))is not null  
  Begin  
   Select 'Yes' 
  End  
  Else  
  Begin  
   select 'No'  
  End  
 END  
GO
/****** Object:  StoredProcedure [dbo].[eNachInsertBankDetail]    Script Date: 07-03-2024 18:43:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE  Procedure [dbo].[eNachInsertBankDetail]  
@LeadMasterId bigint,  
@AccountNo varchar(50),  
@BankName varchar(50),  
@IfscCode varchar(50),  
@AccountType varchar(50),  
@Channel nvarchar(40),
@CreatedBy varchar(200),  
@ModifiedBy varchar(200),  
@MsgId  nvarchar(400)
  
As Begin  
   DECLARE @enachId bigint=0;  
   declare @CreatedDate datetime = getDate()
   declare @ModifiedDate datetime = getDate()

     SELECT @enachId =Id FROM eNachBankDetails WHERE LeadId = @LeadMasterId and AccountNo=@AccountNo and IsDeleted=0 and IsActive=1;  
     if(@enachId>0)  
         begin  
           update eNachBankDetails set AccountNo=@AccountNo,BankName=@BankName,IfscCode=@IfscCode,AccountType=@AccountType,LastModified=GETDATE(),Channel=@Channel , LastModifiedBy=@ModifiedBy, IsActive=1, MsgId=@MsgId  where Id=@enachId;  
         end  
    else  
    begin  
     update eNachBankDetails set IsActive=0 , IsDeleted=1 where LeadId=@LeadMasterId  

     INSERT INTO [dbo].[eNachBankDetails]  
        ([LeadId]  
        ,[AccountNo]  
        ,[BankName]  
        ,[IfscCode]  
        ,[AccountType]  
        ,[Created]  
        ,[CreatedBy]  
        ,[LastModifiedBy]  
        ,[LastModified]  
        ,[IsActive]  
        ,[IsDeleted]  
        ,[Channel]  
		,[MsgId], [UMRN], [responseJSON]
      )  
     VALUES  
        (@LeadMasterId  
        ,@AccountNo            
        ,@BankName  
        ,@IfscCode  
        ,@AccountType  
        ,@CreatedDate  
        ,@CreatedBy  
        ,@ModifiedBy  
        ,null  
        ,1  
        ,0  
        ,@Channel
		,@MsgId,'','')  

		select 1
     end  
 
End  
GO
/****** Object:  StoredProcedure [dbo].[eNachUpdateMsgIdinLeadMaster]    Script Date: 07-03-2024 18:43:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 CREATE proc [dbo].[eNachUpdateMsgIdinLeadMaster]    
 @MsgId varchar(100),    
 @LeadMasterId bigint    
 as    
 begin    
  update LeadMaster set MsgId = @MsgId where Id = @LeadMasterId;  
  update eNachBankDetail set MsgId =@MsgId where LeadMasterId = @LeadMasterId  and IsActive=1;  
 end
GO
/****** Object:  StoredProcedure [dbo].[GetGenerateOfferStatus]    Script Date: 07-03-2024 18:43:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER Procedure [dbo].[GetGenerateOfferStatus] 
--Declare
	@leadId bigint =184,
    @ActivityName nvarchar(100)='Generate Offer'
AS

BEGIN


select  sub.IdentificationCode ComapanyName,
		sub.ActivityMasterId,
		sub.SubActivityMasterId,
		sub.SubActivitySequence,
		sub.Code,
		sub.ActivityName,
		sub.Status as SubActivityStatus,
		o.Id as LeadOfferId,
		sub.Id as LeadNBFCSubActivityId,
		o.Status as LeadOfferStatus,
		Isnull(o.CreditLimit,0) CreditLimit,
		o.ErrorMessage,
		o.NBFCCompanyId,
		api.*
from LeadNBFCSubActivitys sub with(nolock)
inner join LeadOffers o with(nolock)  on o.LeadId = sub.LeadId and sub.NBFCCompanyId=o.NBFCCompanyId and o.IsActive=1 and o.IsDeleted=0
outer apply(
	select  api.Id as APIId, 
			api.Code as ApiCode, 
			api.Status as ApiStatus,
			api.APIUrl,
			api.Sequence as APISequence,
			comnres.Request,
			comnres.Response
	from LeadNBFCApis api  with(nolock)
	outer apply(select top 1 res.Response,res.Request from BlackSoilCommonAPIRequestResponses res with(nolock) 
	 where  api.Id=res.LeadNBFCApiId and res.LeadId=@leadId order by 1 desc)comnres
	where sub.Id = api.LeadNBFCSubActivityId
	and api.IsActive=1 and api.IsDeleted =0
)api

where sub.ActivityName =@ActivityName
and sub.IsActive =1 and sub.IsDeleted =0
and o.IsActive =1 and o.IsDeleted =0
and sub.LeadId =@leadId
order by sub.IdentificationCode, sub.SubActivitySequence, api.APISequence


END
GO
/****** Object:  StoredProcedure [dbo].[GetQualifiedOffer]    Script Date: 07-03-2024 18:43:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER Procedure [dbo].[GetQualifiedOffer]  
--Declare
@NBFCCompanyOffers dbo.NBFCCompanyOffer readonly
As  
begin

	select   
	       l.LeadId,
	       l.CompanyId,
		    Isnull(case when 
				    case 
						when 
						l.VintageDays BETWEEN ccc.MinVintageDays and ccc.MaxVintageDays and l.AvgMonthlyBuying>0 then ROUND((l.AvgMonthlyBuying*ccc.MultiPlier),0)
						else 0
					 end 
					 >= ccc.MaxCreditLimit then ROUND(ccc.MaxCreditLimit,0)
			     when 
					case  
						when l.VintageDays BETWEEN ccc.MinVintageDays and ccc.MaxVintageDays and l.AvgMonthlyBuying>0 
						then Round((l.AvgMonthlyBuying*ccc.MultiPlier),0)
						else 0
					 end 
					 < ccc.MinCreditLimit then ROUND(ccc.MinCreditLimit,0)  
				  when 
					case  
						when l.VintageDays BETWEEN ccc.MinVintageDays and ccc.MaxVintageDays and l.AvgMonthlyBuying>0 
						then round((l.AvgMonthlyBuying*ccc.MultiPlier),0)
						else 0
					 end 
				    = 0 then ROUND(ccc.MinCreditLimit,0)
				   else 
				    case
				       when l.VintageDays BETWEEN ccc.MinVintageDays and ccc.MaxVintageDays and l.AvgMonthlyBuying>0 
						then round((l.AvgMonthlyBuying*ccc.MultiPlier),0)
						else 0
			         end
				 end,0)
				  OfferAmount
		         from @NBFCCompanyOffers l 
			     left join DefaultOfferSelfConfigurations  ccc   on l.companyId=ccc.CompanyId 
			     and l.CibilScore BETWEEN ccc.MinCibilScore and ccc.MaxCibilScore 
				 and l.VintageDays BETWEEN ccc.MinVintageDays and ccc.MaxVintageDays 
				-- and l.CustomerType=ccc.CustomerType 
				 and  ccc.IsActive=1 and ccc.IsDeleted=0

 End


GO
/****** Object:  StoredProcedure [dbo].[spGetCurrentNumber]    Script Date: 07-03-2024 18:43:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER Procedure [dbo].[spGetCurrentNumber] 
--Declare
	@EntityName nvarchar(100)--='LeadNo'
	--@StateId int 
AS

BEGIN
set NoCount on
--SET TRANSACTION ISOLATION LEVEL SERIALIZABLE 
--SET TRANSACTION ISOLATION LEVEL SNAPSHOT

--BEGIN TRANSACTION
--SAVE TRANSACTION GetNextSequence;
 --BEGIN TRY
	Declare 
			@number bigint,						
			@prefix nvarchar(10)='',
			@suffix nvarchar(10)='',
			@nextNo bigint=1,
			@numberStr nvarchar(50),
			@NumberConfigId int,
			@query nvarchar(max),
			@defaultNo bigint,
			@startFrom bigint,
			@entityId int,
			@separator nvarchar(20),
		    @StateAlias nvarchar(50)

	select @query = EntityQuery, @defaultNo = DefaultNo, @entityId=Id, @Separator = Separator from EntityMasters  WITH (NOLOCK) where EntityName = @EntityName	

	--print 'Query: ' +  @query

	--select @StateAlias = AliasName from states  WITH (NOLOCK) where Stateid = @StateId
	set @StateAlias ='LN'
	--print 'Alias: ' +  @StateAlias

	select @NumberConfigId = cc.Id,
		   @prefix = ISNULL(cc.Prefix,''), 
		   @suffix = ISNULL(cc.Suffix,''),
		   @nextNo = ISNULL(cc.NextNumber, 1),
		   @startFrom = ISNULL(cc.StartFrom, 1)
		from EntitySerialMasters cc  WITH (UPDLOCK)
		where EntityId = @entityId --and StateId = @StateId

		--print '@prefix: ' +  @prefix

	if(@NumberConfigId is null)
	Begin
		--print 'numberconfig is null '

		--select *from EntitySerialMasters
		insert into EntitySerialMasters (EntityId,StartFrom,NextNumber,StateId, Created
		,CreatedBy	,LastModified	,LastModifiedBy	,Deleted	,DeletedBy,	IsActive,	IsDeleted
		) values
		(@entityId, 1,  @defaultNo, 0,getdate(),
		null,null,null,null,null,1,0)
		set @NumberConfigId= (select SCOPE_IDENTITY())
		set @nextNo = @defaultNo

	End

	set @number = @nextNo
	if(@suffix is not null and @suffix <> '')
		set	@numberStr = @StateAlias+ isnull(@Separator,'') +isnull(@prefix, '') + isnull(@Separator,'') +  cast(@nextNo as nvarchar(50)) + isnull(@Separator,'') + isnull(@suffix,'')
	else
		set	@numberStr = @StateAlias+ isnull(@Separator,'') + isnull(@prefix, '') + isnull(@Separator,'') +  cast(@nextNo as nvarchar(50))
		

	declare @numCount int = 1
	declare @numCnt int

	while(@numCount > 0)
	Begin

		EXECUTE sp_executesql @query, N'@numberStr nvarchar(100), @numCnt int OUTPUT', @numberStr = @numberStr,  @numCnt = @numCnt OUTPUT
		set @numCount = @numCnt

		if(@numCount > 0)
		Begin 
			--print 'numcount > 0 for ' + @numberStr

			set @number = @number + 1;
			if(@suffix is not null and @suffix <> '')
				set	@numberStr =  @StateAlias+ isnull(@Separator,'') +isnull(@prefix, '') + isnull(@Separator,'') +  cast(@number as nvarchar(50)) + isnull(@Separator,'') + isnull(@suffix,'')
			else
				set	@numberStr = @StateAlias+ isnull(@Separator,'') + isnull(@prefix, '') + isnull(@Separator,'') +  cast(@number as nvarchar(50))
			--set	@numberStr = @prefix + isnull(@Separator,'') + cast(@number as nvarchar(50)) + isnull(@Separator,'') + @suffix
	    End
	
	end
	
	set @nextNo = @number -1
	
	update EntitySerialMasters set NextNumber = @number +1 where Id = @NumberConfigId
	
	-- set	@numberStr = @prefix + isnull(@Separator,'') + cast(@number as nvarchar(50)) + isnull(@Separator,'') + @suffix

	if(@suffix is not null and @suffix <> '')
			set	@numberStr =  @StateAlias+ isnull(@Separator,'') + isnull(@prefix, '') + isnull(@Separator,'') +cast(@number as nvarchar(50)) + isnull(@Separator,'') + isnull(@suffix,'')
		else
			set	@numberStr = @StateAlias+ isnull(@Separator,'') +isnull(@prefix, '') + isnull(@Separator,'') +  cast(@number as nvarchar(50))

	select  @numberStr -- as CurrentNumber
--COMMIT TRANSACTION 
--END TRY
--    BEGIN CATCH
--        IF @@TRANCOUNT > 0
--        BEGIN
--            ROLLBACK TRANSACTION GetNextSequence; -- rollback to MySavePoint
--        END
--    END CATCH
END
GO
";
            migrationBuilder.Sql(sp1);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

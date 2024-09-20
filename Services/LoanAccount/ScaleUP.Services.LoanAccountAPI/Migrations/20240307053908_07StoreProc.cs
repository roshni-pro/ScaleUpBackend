using IdentityServer4.Validation;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _07StoreProc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"GO
/****** Object:  UserDefinedTableType [dbo].[IntValues]    Script Date: 07-03-2024 18:35:24 ******/
CREATE TYPE [dbo].[IntValues] AS TABLE(
	[IntValue] [int] NULL
)
GO
/****** Object:  StoredProcedure [dbo].[DelayPenalityGetPerDayTransactions]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	CREATE proc [dbo].[DelayPenalityGetPerDayTransactions]
	--Declare
	  @TransactionNo varchar(100) ='',
	  @LoanAccountId bigint =null,
	  @DateOfCalculation datetime= '2024-03-04' 
	As 
	Begin

		Declare @TransactionStatusesID_Overdue bigint
		Select @TransactionStatusesID_Overdue=Id From TransactionStatuses with(nolock)  Where Code='Overdue'


		IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
			DROP TABLE #tempTransaction 

		select    ats.Id AccountTransactionId
				, ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN ad.Amount ELSE 0 END), 0) PricipleAmount  
				,ats.DelayPenaltyRate ,l.NBFCIdentificationCode,ats.LoanAccountId
		into #tempTransaction 
		from AccountTransactions ats  with(nolock) 
		inner join LoanAccounts l with(nolock) on ats.LoanAccountId=l.Id
		inner join TransactionStatuses ts with(nolock) on ats.TransactionStatusId=ts.Id
		inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
		where  t.Code = 'OrderPlacement'
		And (ats.ReferenceId=@TransactionNo or ISNULL(@TransactionNo,'')='')
		And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)
		-- And ats.TransactionStatusId = @TransactionStatusesID_Overdue  and (cast(DATEADD(day, 1, ats.DueDate) as Date) <= Cast(GETDATE() as Date) or ats.DueDate is null)
		And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
		and CAST(ats.DueDate as date) < CAST(@DateOfCalculation as DAte)
		and ts.Code in ('Pending','Due','Overdue') and ats.DisbursementDate is not null
		group by ats.Id,ats.DelayPenaltyRate,l.NBFCIdentificationCode,ats.LoanAccountId


		select    tmp.*
				, abs(ISNULL(x.PaymentAmount, 0)) PaymentAmount
				, tmp.PricipleAmount - abs(ISNULL(x.PaymentAmount, 0)) PrincipleOutstanding
		from #tempTransaction tmp 
		outer apply 
		(
			select   SUM(CASE WHEN h.Code = 'Payment' THEN ad.Amount ELSE 0 END ) PaymentAmount 
			from  AccountTransactions ats  with(nolock) 
			inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
			inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
			inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
				--and (h.Code = 'Payment' ) 
			where tmp.AccountTransactionId = ats.ParentAccountTransactionsID and t.Code = 'OrderPayment' and cast(ad.TransactionDate as date) <= cast(@DateOfCalculation as date)
			And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
		)x
	
	End
GO
/****** Object:  StoredProcedure [dbo].[GenerateReferenceNoForTrans]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


Create Proc [dbo].[GenerateReferenceNoForTrans]
--Declare
	@EntityName nvarchar(100)='Transaction'
	
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
	set @StateAlias =''
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
	--if(@suffix is not null and @suffix <> '')
	--	set	@numberStr = @StateAlias+ isnull(@Separator,'') +isnull(@prefix, '') + isnull(@Separator,'') +  cast(@nextNo as nvarchar(50)) + isnull(@Separator,'') + isnull(@suffix,'')
	--else
	--	set	@numberStr = @StateAlias+ isnull(@Separator,'') + isnull(@prefix, '') + isnull(@Separator,'') +  cast(@nextNo as nvarchar(50))
		set	@numberStr = concat(isnull(@prefix, ''),cast(@nextNo as nvarchar(50)))

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
			--if(@suffix is not null and @suffix <> '')
			--	set	@numberStr =  @StateAlias+ isnull(@Separator,'') +isnull(@prefix, '') + isnull(@Separator,'') +  cast(@number as nvarchar(50)) + isnull(@Separator,'') + isnull(@suffix,'')
			--else
			--	set	@numberStr = @StateAlias+ isnull(@Separator,'') + isnull(@prefix, '') + isnull(@Separator,'') +  cast(@number as nvarchar(50))
			----set	@numberStr = @prefix + isnull(@Separator,'') + cast(@number as nvarchar(50)) + isnull(@Separator,'') + @suffix

			set	@numberStr = concat(isnull(@prefix, ''),cast(@nextNo as nvarchar(50)))
	    End
	
	end
	
	set @nextNo = @number -1
	
	update EntitySerialMasters set NextNumber = @number +1 where Id = @NumberConfigId
	
	-- set	@numberStr = @prefix + isnull(@Separator,'') + cast(@number as nvarchar(50)) + isnull(@Separator,'') + @suffix

	--if(@suffix is not null and @suffix <> '')
		--	set	@numberStr =  @StateAlias+ isnull(@Separator,'') + isnull(@prefix, '') + isnull(@Separator,'') +cast(@number as nvarchar(50)) + isnull(@Separator,'') + isnull(@suffix,'')
		--else
		--	set	@numberStr = @StateAlias+ isnull(@Separator,'') +isnull(@prefix, '') + isnull(@Separator,'') +  cast(@number as nvarchar(50))
	
		set	@numberStr = concat(isnull(@prefix, ''),cast(@nextNo as nvarchar(50)))

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
/****** Object:  StoredProcedure [dbo].[GetAccountTransactionList]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	
	CREATE Proc [dbo].[GetAccountTransactionList]
	--Declare
	@Status varchar(50)='All',
	@SearchKeyward varchar(50)='',
	@skip int=0,
	@take int=10,
	@CityName varchar(50)='',
	@AnchorId bigint=0,
	@FromDate datetime='2024-02-01',
	@ToDate datetime='2024-02-22',
	@LoanAccountId bigint=0
	
	as Begin
	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 

	select    ats.Id 
			, ats.ReferenceId As transactionReqNo
			, s.code Status
			, ats.DelayPenaltyRate
			, ats.GstRate
			, ats.PayableBy
			, IsNull(ats.InvoiceNo,'') As InvoiceNo
			, s.Code
			, ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN ad.Amount ELSE 0 END), 0) PricipleAmount  
			, ISNULL(SUM(CASE WHEN h.Code = 'Interest' THEN ad.Amount ELSE 0 END), 0) FullInterestAmount  
			, ISNULL(SUM(CASE WHEN h.Code = 'Interest' And (Cast(ad.TransactionDate as Date)<= Cast(GETDATE() as Date)) THEN ad.Amount ELSE 0 END), 0) InterestAmount  
			, ats.InvoiceId
			, Isnull(cast(ats.DueDate as date),'') DueDate
			, Isnull(cast(ats.Created as date),'') TransactionDate
			, dense_rank() over (order by ats.Id) + dense_rank() over (order by ats.Id  desc) -1 TotalCount
			, CASE WHEN pats.Id IS NOT NULL AND  s.code != 'Paid' THEN 'YES' ELSE 'NO' END as PartiaPaidStatus 
			,LeadCode
			,AccountCode
			,MobileNo
			,CustomerName	
			,AnchorName UtilizationAnchor	
			,l.AnchorCompanyId	
			,CityName
			,ats.LoanAccountId
			,'' OrderId,pats.SettlementDate
	into #tempTransaction 
	from AccountTransactions ats  with(nolock) 
	inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
	inner join TransactionStatuses s with(nolock) on s.id=ats.TransactionStatusId
	inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
	inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
	inner join LoanAccounts l with(nolock) on ats.LoanAccountId=l.Id 
	outer apply(
		select pats.Id,cast(pats.Created as date) SettlementDate 
		from AccountTransactions pats  with(nolock) 
		inner join TransactionTypes pt  with(nolock) on pats.TransactionTypeId = pt.Id
		where ats.Id = pats.ParentAccountTransactionsID and pt.Code =  'OrderPayment'
	)pats
	where  t.Code = 'OrderPlacement'
	And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)	
	And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	and 1=(case   when @Status='Due' and DueDate= cast(Getdate() as date) then 1
					when (@Status='Pending' or @Status='Initiate' or @Status='Intransit') and DueDate>=@FromDate and DueDate <=@ToDate then 1
					when @Status='Overdue' and cast(DATEADD(day,1,DueDate) as date)>=@FromDate and cast(DATEADD(day,1,DueDate) as date)<=@ToDate then 1
					when @Status='All' and DueDate>=@FromDate and DueDate<=@ToDate then 1
					when @Status='Paid' and s.Code='Paid' and cast(ats.Created as date)>=@FromDate and cast(ats.Created as date)<=@ToDate then 1
					else 1
		   end)
			and (s.Code=@Status or @Status='All' OR (@Status = 'Partial' and pats.Id IS NOT NULL AND  s.code != 'Paid'))

	group by ats.Id, ats.ReferenceId, ats.DelayPenaltyRate, ats.GstRate ,ats.PayableBy, ats.InvoiceNo,ats.InvoiceId,s.Code, pats.Id
	,cast(ats.DueDate as date),cast(ats.Created as date)
	,LeadCode,AccountCode,MobileNo,CustomerName,AnchorName,l.AnchorCompanyId,CityName,ats.LoanAccountId
	,pats.SettlementDate

	order by ats.Id desc
	OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY

	select Id ParentID,transactionReqNo ReferenceId, 
		Code ,
		PricipleAmount, 
		PaymentAmount,
		InterestAmount, 
		FullInterestAmount,
		(PricipleAmount+FullInterestAmount) ActualOrderAmount,
		(PaymentAmount+ExtraPaymentAmount+InterestPaymentAmount+BouncePaymentAmount+PenalPaymentAmount) PaidAmount,
		(PricipleAmount+InterestAmount+DelayPenalityAmount+DelayPenalityGstAmount) OutstandingAmount,

		(PricipleAmount+ (case when Status!='Paid' then InterestAmount else 0 end)+DelayPenalityAmount+DelayPenalityGstAmount)
		-(PaymentAmount+ExtraPaymentAmount+InterestPaymentAmount+BouncePaymentAmount+PenalPaymentAmount) as PayableAmount,

		case when PaymentAmount>0 and (PricipleAmount-PaymentAmount)>0 then (PricipleAmount-PaymentAmount) else 0 end PartialPayment,

		DueDate,
		TransactionDate,
		PaymentDate,
		--PaymentAmount,
		ExtraPaymentAmount, 
		InterestPaymentAmount,	
		BouncePaymentAmount, 
		PenalPaymentAmount, 
		--Outstanding, 
		--PaneltyTxnId,	
		DelayPenalityAmount	,
		DelayPenalityGstAmount,
		TotalCount,
		AccountCode
		,LeadCode
			,MobileNo
			,CustomerName	
			,UtilizationAnchor	
			,AnchorCompanyId	
			,CityName
			,LoanAccountId
			,OrderId
			,'' PaymentMode
			,cast(0 as float) ReceivedPayment
			,SettlementDate

	from (
		select    tmp.*
			, abs(ISNULL(x.PaymentAmount, 0)) PaymentAmount
			, abs(ISNULL(x.ExtraPaymentAmount, 0)) ExtraPaymentAmount
			, abs(ISNULL(x.InterestPaymentAmount, 0)) InterestPaymentAmount
			, abs(ISNULL(x.BouncePaymentAmount, 0)) BouncePaymentAmount
			, abs(ISNULL(x.PenalPaymentAmount, 0)) PenalPaymentAmount
			, abs(ISNULL(tmp.PricipleAmount, 0) + ISNULL( x.PaymentAmount, 0)) as Outstanding
			, abs(ISNULL(pats.Id, 0)) PaneltyTxnId 
			, (tmp.PricipleAmount + IsNull(x.PaymentAmount,0)) * ((tmp.DelayPenaltyRate/100)/30) as DelayPenalityAmount
			, ((tmp.PricipleAmount + IsNull(x.PaymentAmount,0)) * ((tmp.DelayPenaltyRate/100)/30)) * (tmp.GstRate/100) as DelayPenalityGstAmount
			,X.PaymentDate
		from #tempTransaction tmp 
	outer apply 
	(
		select   SUM(CASE WHEN h.Code = 'Payment' THEN ad.Amount ELSE 0 END ) PaymentAmount 
				,SUM(CASE WHEN h.Code = 'InterestPaymentAmount' THEN ad.Amount ELSE 0 END ) InterestPaymentAmount
				,SUM(CASE WHEN h.Code = 'ExtraPaymentAmount' THEN ad.Amount ELSE 0 END ) ExtraPaymentAmount
				,SUM(CASE WHEN h.Code = 'BouncePaymentAmount' THEN ad.Amount ELSE 0 END ) BouncePaymentAmount
				,SUM(CASE WHEN h.Code = 'PenalPaymentAmount' THEN ad.Amount ELSE 0 END ) PenalPaymentAmount
				,MAX(CASE WHEN h.Code = 'Payment' THEN ad.PaymentDate ELSE null END ) PaymentDate 
		from  AccountTransactions ats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
			--and (h.Code = 'Payment' ) 
		where tmp.Id = ats.ParentAccountTransactionsID and t.Code = 'OrderPayment'
		And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)x
	outer apply(
	
		select pats.Id from   
		AccountTransactions pats  with(nolock) 
		left join TransactionTypes pt  with(nolock)  on pats.TransactionTypeId = pt.Id
		where  pt.Code = 'PenaltyCharges' and tmp.Id = pats.ParentAccountTransactionsID
	)pats
	) x

	end
GO
/****** Object:  StoredProcedure [dbo].[GetCustomerTransactionBreakup]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER Procedure [dbo].[GetCustomerTransactionBreakup]

--declare
	@TransactionId bigint--=659
As 
Begin
	

	select   h.Code TransactionType
			,ISNULL(SUM(ISNULL(ad.Amount, 0)), 0) Amount ,
			max(ats.Id) as Id
	from AccountTransactions ats  with(nolock) 
	inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id  and t.IsActive =1 and t.IsDeleted =0
	inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
	inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id  and h.IsActive =1 and h.IsDeleted =0
	where ( ats.Id = @TransactionId or ats.ParentAccountTransactionsId = @TransactionId)
	and (ad.TransactionDate  IS NULL OR CAST(ad.TransactionDate as DATE) <= GETDATE())
	and ats.IsActive =1 and ats.IsDeleted =0 
	and ad.IsActive =1 and ad.IsDeleted =0
	group by  h.Code,h.SequenceNo
	order by h.SequenceNo
	 --MAX(ats.Created)
			--,ats.OrderId
			--,ats.Status


End

GO
/****** Object:  StoredProcedure [dbo].[GetCustomerTransactionList]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER Procedure [dbo].[GetCustomerTransactionList]

--declare
	@LeadId bigint=205, --= 15
	@AnchorCompanyID bigint=2, 
	@Skip int = 0,
	@Take int = 4,
	@TransactionType nvarchar(100) = 'All' --- 'ALL/Paid/Unpaid'
As 
Begin
	

	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 

	SELECT   t.Id as TransactionId
			,ts.Code as Status	
	        ,sum(Isnull(atd.Amount,0)) as TotalAmount
			,CASE WHEN t.DisbursementDate IS NOT NULL THEN t.DueDate ELSE NULL END DueDate
			,t.InvoiceNo as OrderId 
			,la.AnchorName as AnchorName  
			,ROUND(ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN atd.Amount ELSE 0 END), 0),2) PricipleAmount  
	into #tempTransaction 
	FROM AccountTransactions t
	inner join TransactionStatuses ts on t.TransactionStatusId = ts.Id  and ts.IsActive=1 and ts.IsDeleted=0
	inner join TransactionTypes tt  with(nolock) on t.TransactionTypeId = tt.Id  and tt.IsActive=1 and tt.IsDeleted=0
	inner join AccountTransactionDetails atd with(nolock) on t.Id = atd.AccountTransactionId  and atd.IsActive=1 and atd.IsDeleted=0
	inner join TransactionDetailHeads h with(nolock) on h.Id=atd.TransactionDetailHeadId and h.IsActive=1 and h.IsDeleted=0
	inner join LoanAccounts la with(nolock) on t.LoanAccountId = la.Id and la.IsActive=1 and la.IsDeleted=0
	WHERE (@AnchorCompanyID = 0 OR t.AnchorCompanyId = @AnchorCompanyID)
		and  tt.Code = 'OrderPlacement'
		and (ts.Code != 'Failed'  AND ts.Code != 'Initiate'  AND ts.Code != 'Canceled')
		and (@TransactionType = 'All' OR (@TransactionType = 'Paid' AND  ts.Code= 'Paid') OR (@TransactionType = 'Unpaid' AND  ts.Code != 'Paid'))
		and t.IsActive=1 and t.IsDeleted=0 and (atd.TransactionDate is null or cast(atd.TransactionDate as date) <= cast(getdate() as date))
		--and cast(t.DueDate as date) <=  cast(getdate() as date)
		and t.IsActive =1 and t.IsDeleted =0
		and la.LeadId = @LeadId
		group by  t.Id, t.Created, t.DisbursementDate , t.DueDate , t.InvoiceNo ,la.AnchorName , ts.Code
		order by t.Created desc
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY


	select   tt.AnchorName
		    ,tt.DueDate
			,tt.OrderId
			,tt.Status
			,tt.TransactionId
			,ISNULL(tt.TotalAmount, 0) + IsNULL(x.Amount, 0) TotalRemainingAmount
			,tt.PricipleAmount as Amount
	from #tempTransaction tt
	outer apply(
             select
			       SUM(ISNULL(atd.Amount, 0)) Amount
			 from AccountTransactions at with(nolock) 
			 inner join AccountTransactionDetails atd with(nolock) on at.Id = atd.AccountTransactionId
			 where tt.TransactionId = at.ParentAccountTransactionsID  and (atd.TransactionDate  IS NULL OR CAST(atd.TransactionDate as DATE) <= GETDATE())
	         and at.IsActive =1 and at.IsDeleted =0 and atd.IsActive =1 and atd.IsDeleted =0
			 --group by at.ParentAccountTransactionsID 
	)x	
		
			--,ats.OrderId
			--,ats.Status
End


GO
/****** Object:  StoredProcedure [dbo].[GetCustomerTransactionList_Two]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER Procedure [dbo].[GetCustomerTransactionList_Two]

--declare
	@LeadId bigint=248, --= 15
	@Skip int = 5,
	@Take int = 5
	
As 
Begin
	


	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 

	SELECT   t.Id as TransactionId
			,ts.Code as Status	
	        ,sum(Isnull(atd.Amount,0)) as TotalAmount
			,CASE WHEN t.DisbursementDate IS NOT NULL THEN t.DueDate ELSE NULL END DueDate
			,t.InvoiceNo as OrderId 
			,la.AnchorName as AnchorName  
			,ROUND(ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN atd.Amount ELSE 0 END), 0),2) PricipleAmount  
	into #tempTransaction 
	FROM AccountTransactions t
	inner join TransactionStatuses ts on t.TransactionStatusId = ts.Id  and ts.IsActive=1 and ts.IsDeleted=0
	inner join TransactionTypes tt  with(nolock) on t.TransactionTypeId = tt.Id  and tt.IsActive=1 and tt.IsDeleted=0
	inner join AccountTransactionDetails atd with(nolock) on t.Id = atd.AccountTransactionId  and atd.IsActive=1 and atd.IsDeleted=0
	inner join TransactionDetailHeads h with(nolock) on h.Id=atd.TransactionDetailHeadId and h.IsActive=1 and h.IsDeleted=0
	inner join LoanAccounts la with(nolock) on t.LoanAccountId = la.Id and la.IsActive=1 and la.IsDeleted=0
	WHERE tt.Code = 'OrderPlacement'
		and (ts.Code != 'Failed'  AND ts.Code != 'Initiate'  AND ts.Code != 'Canceled')
		and t.IsActive=1 and t.IsDeleted=0 and (atd.TransactionDate is null or cast(atd.TransactionDate as date) <= cast(getdate() as date))
		--and cast(t.DueDate as date) <=  cast(getdate() as date)
		and t.IsActive =1 and t.IsDeleted =0
		and la.LeadId = @LeadId
		group by  t.Id, t.Created, t.DisbursementDate , t.DueDate , t.InvoiceNo ,la.AnchorName , ts.Code
		order by t.Created desc
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY


	select   tt.AnchorName
		    ,tt.DueDate
			,tt.OrderId
			,tt.Status
			,tt.TransactionId
			,ISNULL(tt.TotalAmount, 0) + IsNULL(x.Amount, 0) TotalRemainingAmount
			,tt.PricipleAmount as Amount
	from #tempTransaction tt
	outer apply(
             select
			       SUM(ISNULL(atd.Amount, 0)) Amount
			 from AccountTransactions at with(nolock) 
			 inner join AccountTransactionDetails atd with(nolock) on at.Id = atd.AccountTransactionId
			 where tt.TransactionId = at.ParentAccountTransactionsID  and (atd.TransactionDate  IS NULL OR CAST(atd.TransactionDate as DATE) <= GETDATE())
	         and at.IsActive =1 and at.IsDeleted =0 and atd.IsActive =1 and atd.IsDeleted =0
			 --group by at.ParentAccountTransactionsID 
	)x	

End

GO
/****** Object:  StoredProcedure [dbo].[GetLeadProductId]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create Proc [dbo].[GetLeadProductId]
--declare
@TransactionReqNo varchar(50)=''
As 
Begin
select l.Id LoanAccountId, l.LeadId,  l.ProductId,l.AnchorCompanyId,l.MobileNo from PaymentRequest p with(nolock)
inner join LoanAccounts l with(nolock)  on l.Id=p.LoanAccountId
where p.TransactionReqNo=@TransactionReqNo
End
GO
/****** Object:  StoredProcedure [dbo].[GetLoanAccountDetail]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	CREATE proc [dbo].[GetLoanAccountDetail]
	--Declare
	   @LoanAccountId bigint =35
	as
	begin
	declare @InvoiceDate datetime = getdate()
	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
			DROP TABLE #tempTransaction 

		select  ats.Id	        
				, ats.ReferenceId As transactionReqNo
				, ats.DelayPenaltyRate
				, ats.GstRate
				, ats.PayableBy
				, IsNull(ats.InvoiceNo,'') As InvoiceNo			
				, ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN ad.Amount ELSE 0 END), 0) PricipleAmount  			
				, ISNULL(SUM(CASE WHEN h.Code = 'Interest'   And (Cast(ad.TransactionDate as Date)<= Cast(@InvoiceDate as Date) or @InvoiceDate is null) THEN ad.Amount ELSE 0 END), 0) InterestAmount  
				,ats.InvoiceId 
				,ats.PaidAmount
		into #tempTransaction 
		from AccountTransactions ats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
		inner join TransactionStatuses s  with(nolock) on ats.TransactionStatusId = s.Id
		inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 		
		where  t.Code = 'OrderPlacement' and ats.LoanAccountId=@LoanAccountId  
		And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
		and s.Code!='Canceled'
		group by ats.Id, ats.ReferenceId, ats.DelayPenaltyRate, ats.GstRate ,ats.PayableBy, ats.InvoiceNo,ats.InvoiceId 
		,ats.PaidAmount

	

		Select l.AccountCode,l.MobileNo,l.CustomerName,l.CityName,l.ProductType
		       , isnull(l.ShopName,'') ShopName,isnull(l.CustomerImage,'') LoanImage,l.IsAccountActive,l.IsBlock,l.IsBlockComment, l.ThirdPartyLoanCode
			  ,l.NBFCIdentificationCode,l.IsDefaultNBFC
			  --,b.CreditLimitAmount
			  ,b.DisbursalAmount CreditLimitAmount
			  ,b.DisbursalAmount TotalSanctionedAmount
			  ,y.UtilizedAmount
			  ,y.LTDUtilizedAmount
			   ,(round(PrincipleOutstanding,2,2)+round(InterestOutstanding,2,2)+round(PenalOutStanding,2,2)+round(OverdueInterestOutStanding,2,2)) TotalOutStanding
			   ,round(PrincipleOutstanding,2,2) PrincipleOutstanding
			   ,round(InterestOutstanding,2,2) InterestOutstanding
			   ,round(PenalOutStanding,2,2) PenalOutStanding
			   ,round(OverdueInterestOutStanding,2,2) OverdueInterestOutStanding
			   ,abs(round(TotalRepayment,2,2))TotalRepayment
			   ,abs(round(PrincipalRepaymentAmount,2,2))PrincipalRepaymentAmount
			   ,abs(round(InterestRepaymentAmount,2,2))InterestRepaymentAmount
			   ,abs(round(OverdueInterestPaymentAmount,2,2))OverdueInterestPaymentAmount
			   ,abs(round(PenalRePaymentAmount,2,2))PenalRePaymentAmount
			   ,abs(round(BounceRePaymentAmount,2,2))BounceRePaymentAmount
			   ,abs(round(ExtraPaymentAmount,2,2))ExtraPaymentAmount,
			   tans.*
		from LoanAccounts l	
		inner join LoanAccountCredits b on l.id=b.LoanAccountId and b.IsActive=1 and b.IsDeleted=0
		outer apply
		(
			select	
				Sum(case when h.Code in ('Refund','Order','Payment') and s.Code not in ('Paid','Canceled','Failed')  then d.Amount else 0 end) as UtilizedAmount,
				Sum(case when t.Code='OrderPlacement' and s.Code in ('Initiate','Intransit') then a.TransactionAmount else 0 end) Intransitlimit,
				Sum(case when h.Code in ('Refund','Order') and s.Code not in ('Failed','Canceled')  then d.Amount else 0 end) as LTDUtilizedAmount
				,Sum(case when h.Code in ('Order','Payment','Refund') and s.Code in ('Pending','Due','Overdue','Delinquent')  then d.Amount else 0 end) as PrincipleOutstanding
				,Sum(case when h.Code in ('Interest','InterestPaymentAmount') and s.Code in ('Pending','Due','Overdue','Delinquent')  and cast(TransactionDate as date)<=cast(GETDATE() as date) then d.Amount else 0 end) as InterestOutstanding
				,Sum(case when h.Code in ('DelayPenalty','PenalPaymentAmount') and s.Code in ('Pending','Due','Overdue','Delinquent')  then d.Amount else 0 end) as PenalOutStanding
				,Sum(case when h.Code in ('OverdueInterestAmount','OverduePaymentAmount') and s.Code in ('Pending','Due','Overdue','Delinquent')  then d.Amount else 0 end) as OverdueInterestOutStanding
				
				,Sum(case when h.Code in ('ExtraPaymentAmount'
										,'OverduePaymentAmount'
										,'PenalPaymentAmount'
										,'InterestPaymentAmount'
										,'BouncePaymentAmount'
										,'Payment'
										,'Refund') and s.Code='Paid'
						then d.Amount else 0 end) as TotalRepayment
				,Sum(case when h.Code in ('Payment') and s.Code='Paid' then d.Amount else 0 end) as PrincipalRepaymentAmount
				,Sum(case when h.Code in ('InterestPaymentAmount') and s.Code='Paid' then d.Amount else 0 end) as InterestRepaymentAmount
				,Sum(case when h.Code in ('OverduePaymentAmount') and s.Code='Paid' then d.Amount else 0 end) as OverdueInterestPaymentAmount
				,Sum(case when h.Code in ('PenalPaymentAmount') and s.Code='Paid' then d.Amount else 0 end) as PenalRePaymentAmount
				,Sum(case when h.Code in ('BouncePaymentAmount') and s.Code='Paid' then d.Amount else 0 end) as BounceRePaymentAmount
				,Sum(case when h.Code in ('ExtraPaymentAmount') and s.Code='Paid' then d.Amount else 0 end) as ExtraPaymentAmount

				from AccountTransactions a
				inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId
				inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId
				inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
				inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId
				where d.IsActive=1 and d.IsDeleted=0 and a.LoanAccountId=l.Id and h.IsActive=1 and h.IsDeleted=0
		) y
		Outer apply
		(
		select   --isnull(sum(isnull(tmp.PricipleAmount,0)),0) LTDUtilizedAmount
				 isnull(Sum(Isnull(x.PenalAmount,0)),0) PenalAmount
				--,isnull(Sum( abs(ISNULL(x.PaymentAmount, 0))),0) PrincipalRepaymentAmount
				--, isnull(Sum(abs(ISNULL(x.ExtraPaymentAmount, 0))),0) ExtraPaymentAmount
				--, isnull(Sum(abs(ISNULL(x.InterestPaymentAmount, 0))),0) InterestRepaymentAmount
				--,isnull(Sum( abs(ISNULL(x.BouncePaymentAmount, 0))),0) BounceRePaymentAmount
				--,isnull(Sum( abs(ISNULL(x.PenalPaymentAmount, 0))),0) PenalRePaymentAmount
				--,isnull(Sum( abs(ISNULL(x.OverdueInterestPaymentAmount, 0))),0) OverdueInterestPaymentAmount
				--,isnull(Sum( abs(ISNULL(x.PaymentAmount, 0)) 
				--+ abs(ISNULL(x.ExtraPaymentAmount, 0)) 
				--+ abs(ISNULL(x.InterestPaymentAmount, 0)) 
				--+ abs(ISNULL(x.BouncePaymentAmount, 0)) 
				--+ abs(ISNULL(x.PenalPaymentAmount, 0))
				--+ abs(ISNULL(x.OverdueInterestPaymentAmount, 0))
				--),0) TotalRepayment
				--,isnull(Sum( abs(ISNULL(tmp.PricipleAmount, 0) + ISNULL( x.PaymentAmount, 0))),0) as PrincipleOutstanding			
				--,isnull(Sum(ISNULL(tmp.InterestAmount,0) + ISNULL(x.InterestPaymentAmount, 0)),0) InterestOutstanding			
				--,isnull(Sum( Isnull(x.PenalAmount,0) + ISNULL(x.PenalPaymentAmount, 0)),0)  PenalOutStanding	
				--,isnull(Sum( Isnull(Overdue.OverDueInterest,0) + ISNULL(x.OverdueInterestPaymentAmount, 0)),0)  OverdueInterestOutStanding			
				--,isnull(Sum( abs(ISNULL(tmp.PricipleAmount, 0) + ISNULL( x.PaymentAmount, 0)))
				-- + Sum(ISNULL(tmp.InterestAmount,0) + ISNULL(x.InterestPaymentAmount, 0))
				-- +Sum(Isnull(x.PenalAmount,0) + ISNULL(x.PenalPaymentAmount, 0))
				-- +Sum(Isnull(Overdue.OverDueInterest,0) + ISNULL(x.OverdueInterestPaymentAmount, 0))
				-- ,0)  TotalOutStanding 
				 
		from #tempTransaction tmp 
		
		outer apply 
		(select sum(ib.Amount) OverDueInterest from 	AccountTransactions Ia  with(nolock) 
				inner join TransactionTypes it  with(nolock) on ia.TransactionTypeId = it.Id
				inner join AccountTransactionDetails ib  with(nolock)  on ia.Id = ib.AccountTransactionId 
				where ia.ParentAccountTransactionsID=tmp.id and it.Code='OverdueInterest' 
				and cast(ib.TransactionDate as date)<=cast(@InvoiceDate as date)
				And ia.IsActive=1 And ia.IsDeleted=0 and ib.IsActive=1 and ib.IsDeleted=0
		) Overdue
		outer apply 
		(
			select   SUM(CASE WHEN h.Code = 'Payment' THEN ad.Amount ELSE 0 END ) PaymentAmount 
					,SUM(CASE WHEN h.Code = 'InterestPaymentAmount' THEN ad.Amount ELSE 0 END ) InterestPaymentAmount
					,SUM(CASE WHEN h.Code = 'ExtraPaymentAmount' THEN ad.Amount ELSE 0 END ) ExtraPaymentAmount
					,SUM(CASE WHEN h.Code = 'BouncePaymentAmount' THEN ad.Amount ELSE 0 END ) BouncePaymentAmount
					,SUM(CASE WHEN h.Code = 'PenalPaymentAmount' THEN ad.Amount ELSE 0 END ) PenalPaymentAmount
					,SUM(CASE WHEN h.Code = 'DelayPenalty' THEN ad.Amount ELSE 0 END ) PenalAmount
					,SUM(CASE WHEN h.Code = 'OverduePaymentAmount' THEN ad.Amount ELSE 0 END ) OverdueInterestPaymentAmount
					
					
			from  AccountTransactions ats  with(nolock) 
			inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
			inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
			inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
				--and (h.Code = 'Payment' ) 
			where tmp.Id = ats.ParentAccountTransactionsID and t.Code in ( 'OrderPayment','PenaltyCharges')
			And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
		)x 	
	  ) tans
	  where  l.id=@LoanAccountId
	end

GO
/****** Object:  StoredProcedure [dbo].[GetLoanAccountDetailByTxnId]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	CREATE Proc [dbo].[GetLoanAccountDetailByTxnId]
	--declare
	@TransactionReqNo varchar(50)='2024549'
	As 
	Begin
		
		declare @LoanId bigint=0

	select top 1 @LoanId=LoanAccountId from PaymentRequest with(nolock) where TransactionReqNo=@TransactionReqNo

	select l.Id LoanAccountId, l.LeadId,  l.ProductId,p.AnchorCompanyId,l.MobileNo,ac.CreditLimitAmount,l.NBFCCompanyId
	,isnull((x.UtilizeAmount),0) UtilizateLimit,l.AnchorName,p.TransactionAmount InvoiceAmount,l.NBFCIdentificationCode
	,p.OrderNo,l.CustomerName, '' ImageUrl, cast(0 as bigint) creditday
	
	,case when Overdue.Status is not null then Overdue.Status else '' end TransactionStatus
	
	,l.IsAccountActive,l.IsBlock,l.IsBlockComment

	from PaymentRequest p with(nolock)
	inner join LoanAccounts l with(nolock)  on l.Id=p.LoanAccountId
	inner join LoanAccountCredits ac with(nolock) on ac.LoanAccountId=l.Id
	
	outer apply (
			select 
				Sum(case when h.Code in ('Refund','Order','Payment') and s.Code not in ('Paid','Canceled','Failed')  then d.Amount else 0 end) as UtilizeAmount,
				Sum(case when t.Code='OrderPlacement' and s.Code in ('Initiate','Intransit') then a.TransactionAmount else 0 end) Intransitlimit
				from AccountTransactions a
				inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId
				inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId
				inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
				inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId
				where d.IsActive=1 and d.IsDeleted=0 and a.LoanAccountId=l.Id and h.IsActive=1 and h.IsDeleted=0
		) x
	outer apply(
			select top 1 s.code Status 
			from AccountTransactions a with(nolock) 
			inner join TransactionStatuses s with(nolock) on a.TransactionStatusId=s.Id
			where LoanAccountId=@LoanId and s.Code='Overdue'
		) Overdue
		where p.TransactionReqNo=@TransactionReqNo
	End
	
GO
/****** Object:  StoredProcedure [dbo].[GetLoanAccountList]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- exec GetLoanAccountList 0,0,0,'2024-01-01','2024-02-02','','',0,0,0,10

	CREATE Proc [dbo].[GetLoanAccountList]
	--declare 
	@ProductType bigint=0,
	@Status int=-1,
	@FromDate datetime='2024-01-31',
	@ToDate datetime='2024-03-29',
	@CityName  varchar(200)='',
	@Keyward varchar(200)='',
	@Min int=0,
	@Max int=0,
	@Skip int=0,
	@Take int=100,
	@AnchorId bigint=0

	 As
	 Begin

	 

		DECLARE @StartDate date,
				@EndDate date

		IF((@FromDate is null or @FromDate='') and (@ToDate is null or @ToDate=''))
			Begin
				SELECT @StartDate = MIN(c.Created),  @EndDate = max(c.Created) FROM LoanAccounts c with(nolock) where c.IsActive=1 and c.IsDeleted=0
			End
			

		IF(@Max=0)
			Begin
				select @Max =max(c.DisbursalAmount)
				from LoanAccounts l with(nolock)
				inner join LoanAccountCredits c with(nolock) on l.Id=c.LoanAccountId  and l.IsActive=1 and l.IsDeleted=0
			End
		
		select 
			l.Id LoanAccountId,
			case when l.IsAccountActive=1 and l.IsBlock=0 then 'Active' 
				  when l.IsAccountActive=0 and l.IsBlock=0 then 'InActive'
				  when l.IsBlock=1 then 'Block'
			end AccountStatus,

			LeadId, 
			ProductId,
			l.ProductType,
			UserId, 
			CustomerName, 
			l.AccountCode LeadCode, 
			MobileNo,	
			NBFCCompanyId,	
			AnchorCompanyId,
			AgreementRenewalDate,	
			CreditScore,	
			ApplicationDate,	
			DisbursalDate,	
			IsDefaultNBFC,	
			isnull(CityName,'') CityName,
			isnull(AnchorName,'')  AnchorName,
			--c.CreditLimitAmount AvailableCreditLimit,
			case when c.CreditLimitAmount>Intransitlimit then c.CreditLimitAmount-Intransitlimit else 0 end AvailableCreditLimit,
			c.DisbursalAmount,
			--isNull(x.UtilizeAmount,0) UtilizeAmount,
			isNull((x.UtilizeAmount),0) UtilizeAmount,
		    isNull(case when c.DisbursalAmount>0 then	((x.UtilizeAmount)/c.DisbursalAmount)*100 else 0 end,0) as UtilizePercent,
			l.IsAccountActive status,
			dense_rank() over (order by l.id) + dense_rank() over (order by l.id desc) -1 TotalCount

			,sum(case when l.IsAccountActive=1 then ISNULL(c.CreditLimitAmount,0)  else 0 end) over() as TotalActive,
			sum(case when l.IsAccountActive=0 then ISNULL(c.CreditLimitAmount,0) else 0 end) over() as TotalInActive,
			sum(ISNULL(c.CreditLimitAmount,0)) over() as TotalDisbursal,
			l.IsBlock,l.NBFCIdentificationCode
			, cast(0 as bigint) ParentID
			
		from LoanAccounts l with(nolock)
		inner join LoanAccountCredits c with(nolock) on l.Id=c.LoanAccountId
		outer apply (
				select 
				Sum(case when h.Code in ('Refund','Order','Payment') and s.Code not in ('Paid','Canceled','Failed')  then d.Amount else 0 end) as UtilizeAmount,
				Sum(case when t.Code='OrderPlacement' and s.Code in ('Initiate','Intransit') then a.TransactionAmount else 0 end) Intransitlimit
				from AccountTransactions a
				inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId
				inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId
				inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
				inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId
				where d.IsActive=1 and d.IsDeleted=0 and a.LoanAccountId=l.Id
		) x
		
		--select * from TransactionStatuses
			where ((l.CustomerName like '%'+@Keyward+'%' or ISNULL(@Keyward,'')='')
			  or (l.MobileNo like '%'+@Keyward+'%' or ISNULL(@Keyward,'')='')
			  or (l.AccountCode like '%'+@Keyward+'%' or ISNULL(@Keyward,'')='')
			  or (l.LeadCode like '%'+@Keyward+'%' or ISNULL(@Keyward,'')=''))
		     
		      and (l.CityName=@CityName or Isnull(@CityName,'')='')
		      and (l.AnchorCompanyId=@AnchorId or Isnull(@AnchorId,0)=0)
			  and (l.ProductId=@ProductType or Isnull(@ProductType,0)=0)
			  and CAST(l.DisbursalDate as date)>=@FromDate and CAST(l.DisbursalDate as date)<=@ToDate
			  and l.IsActive=1 and l.IsDeleted=0
			  and (@Status=-1 or (@Status<>2 and (l.IsAccountActive =@Status and l.IsBlock=0)) or (@Status=2 and (l.IsAccountActive=0 and l.IsBlock=1)))

		order by l.LastModified 
		OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY
		
	End

GO
/****** Object:  StoredProcedure [dbo].[GetLoanAccountListExport]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


	Create Proc [dbo].[GetLoanAccountListExport]
	--declare 
	@ProductType bigint=0,
	@Status int=-1,
	@FromDate datetime='2024-01-31',
	@ToDate datetime='2024-03-29',
	@CityName  varchar(200)='',
	@Keyward varchar(200)='',
	@Min int=0,
	@Max int=0,
	@Skip int=0,
	@Take int=100,
	@AnchorId bigint=0

	 As
	 Begin

	 

		DECLARE @StartDate date,
				@EndDate date

		IF((@FromDate is null or @FromDate='') and (@ToDate is null or @ToDate=''))
			Begin
				SELECT @StartDate = MIN(c.Created),  @EndDate = max(c.Created) FROM LoanAccounts c with(nolock) where c.IsActive=1 and c.IsDeleted=0
			End
			

		IF(@Max=0)
			Begin
				select @Max =max(c.DisbursalAmount)
				from LoanAccounts l with(nolock)
				inner join LoanAccountCredits c with(nolock) on l.Id=c.LoanAccountId  and l.IsActive=1 and l.IsDeleted=0
			End
		
		select 
			l.Id LoanAccountId,
			case when l.IsAccountActive=1 and l.IsBlock=0 then 'Active' 
				  when l.IsAccountActive=0 and l.IsBlock=0 then 'InActive'
				  when l.IsBlock=1 then 'Block'
			end AccountStatus,

			LeadId, 
			ProductId,
			l.ProductType,
			UserId, 
			CustomerName, 
			l.AccountCode LeadCode, 
			MobileNo,	
			NBFCCompanyId,	
			AnchorCompanyId,
			AgreementRenewalDate,	
			CreditScore,	
			ApplicationDate,	
			DisbursalDate,	
			IsDefaultNBFC,	
			isnull(CityName,'') CityName,
			isnull(AnchorName,'')  AnchorName,
			--c.CreditLimitAmount AvailableCreditLimit,
			case when c.CreditLimitAmount>Intransitlimit then c.CreditLimitAmount-Intransitlimit else 0 end AvailableCreditLimit,
			c.DisbursalAmount,
			--isNull(x.UtilizeAmount,0) UtilizeAmount,
			isNull((x.UtilizeAmount),0) UtilizeAmount,
		    isNull(case when c.DisbursalAmount>0 then	((x.UtilizeAmount)/c.DisbursalAmount)*100 else 0 end,0) as UtilizePercent,
			l.IsAccountActive status,
			dense_rank() over (order by l.id) + dense_rank() over (order by l.id desc) -1 TotalCount

			,sum(case when l.IsAccountActive=1 then ISNULL(c.CreditLimitAmount,0)  else 0 end) over() as TotalActive,
			sum(case when l.IsAccountActive=0 then ISNULL(c.CreditLimitAmount,0) else 0 end) over() as TotalInActive,
			sum(ISNULL(c.CreditLimitAmount,0)) over() as TotalDisbursal,
			l.IsBlock,l.NBFCIdentificationCode
			, cast(0 as bigint) ParentID
			
		from LoanAccounts l with(nolock)
		inner join LoanAccountCredits c with(nolock) on l.Id=c.LoanAccountId
		outer apply (
				select 
				Sum(case when h.Code in ('Refund','Order','Payment') and s.Code not in ('Paid','Canceled','Failed')  then d.Amount else 0 end) as UtilizeAmount,
				Sum(case when t.Code='OrderPlacement' and s.Code in ('Initiate','Intransit') then a.TransactionAmount else 0 end) Intransitlimit
				from AccountTransactions a
				inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId
				inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId
				inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
				inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId
				where d.IsActive=1 and d.IsDeleted=0 and a.LoanAccountId=l.Id
		) x
		
		--select * from TransactionStatuses
			where ((l.CustomerName like '%'+@Keyward+'%' or ISNULL(@Keyward,'')='')
			  or (l.MobileNo like '%'+@Keyward+'%' or ISNULL(@Keyward,'')='')
			  or (l.AccountCode like '%'+@Keyward+'%' or ISNULL(@Keyward,'')='')
			  or (l.LeadCode like '%'+@Keyward+'%' or ISNULL(@Keyward,'')=''))
		     
		      and (l.CityName=@CityName or Isnull(@CityName,'')='')
		      and (l.AnchorCompanyId=@AnchorId or Isnull(@AnchorId,0)=0)
			  and (l.ProductId=@ProductType or Isnull(@ProductType,0)=0)
			  and CAST(l.DisbursalDate as date)>=@FromDate and CAST(l.DisbursalDate as date)<=@ToDate
			  and l.IsActive=1 and l.IsDeleted=0
			  and (@Status=-1 or (@Status<>2 and (l.IsAccountActive =@Status and l.IsBlock=0)) or (@Status=2 and (l.IsAccountActive=0 and l.IsBlock=1)))

		order by l.LastModified 
		
		
	End

GO
/****** Object:  StoredProcedure [dbo].[GetLoanAccountSummary]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE proc [dbo].[GetLoanAccountSummary]

--declare

@AnchorCompanyId bigint=0,
@LeadId bigint =235

as
begin

     select 
	      
		  Sum(case when h.Code in ('Refund','Order','Payment') and s.Code not in ('Paid','Canceled','Failed')  then d.Amount else 0 end) as UtilizedAmount,
		  max(lc.DisbursalAmount) - Sum(case when h.Code in ('Refund','Order','Payment') and s.Code not in ('Paid','Canceled','Failed')  then d.Amount else 0 end) as AvailableLimit
	 from LoanAccountCredits lc with(nolock)
     inner join LoanAccounts l with(nolock) on lc.LoanAccountId = l.Id and l.IsActive=1 and l.IsDeleted=0 
	 inner join AccountTransactions a with(nolock) on l.Id = a.LoanAccountId and a.IsActive=1 and a.IsDeleted=0 
	 inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId and d.IsActive=1 and d.IsDeleted=0 
	 inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId and t.IsActive=1 and t.IsDeleted=0
	 inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId and s.IsActive=1 and s.IsDeleted=0
	 inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId and h.IsActive=1 and h.IsDeleted=0
	 where lc.IsActive =1 and lc.IsDeleted =0  and a.LoanAccountId=l.Id and l.LeadId = @LeadId

end
		
GO
/****** Object:  StoredProcedure [dbo].[GetOutstandingTransactionsList]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER Procedure [dbo].[GetOutstandingTransactionsList]
--Declare
@TransactionNo varchar(20) = NULL --= 'HQDUD33EYW0Q3RP4'
, @LoanAccountId bigint =41 --= 15
, @InvoiceDate datetime ='2024-03-06'
As 
Begin
	Declare @TransactionStatusesID_Overdue bigint
		Select @TransactionStatusesID_Overdue=Id From TransactionStatuses with(nolock)  Where Code='Overdue'


	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 

	select    ats.Id
			, ats.ReferenceId As transactionReqNo
			, ats.DelayPenaltyRate
			, ats.GstRate
			, ats.PayableBy
			, IsNull(ats.InvoiceNo,'') As InvoiceNo
			, Isnull(b.WithdrawalId,0) as WithdrawlId
			, ROUND(ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN ad.Amount ELSE 0 END), 0),2) PricipleAmount  
			--, ISNULL(SUM(CASE WHEN h.Code = 'Interest' THEN ad.Amount ELSE 0 END), 0) InterestAmount  
			, ROUND(ISNULL(SUM(CASE WHEN h.Code = 'Interest' And (Cast(ad.TransactionDate as Date)<= Cast(@InvoiceDate as Date) or @InvoiceDate is null) THEN ad.Amount ELSE 0 END), 0),2) InterestAmount  
			--, 0 as PenaltyChargesAmount
			,ats.InvoiceId 
	into #tempTransaction 
	from AccountTransactions ats  with(nolock) 
	inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
	inner join TransactionStatuses ts  with(nolock) on ats.TransactionStatusId = ts.Id
	inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
	inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
	Left Outer Join  BlackSoilAccountTransactions b with(nolock) ON b.LoanInvoiceId= ats.InvoiceId
		--and (h.Code = 'Order' OR h.Code = 'Refund') 
	where  t.Code = 'OrderPlacement'
	and ts.IsActive =1 and ts.IsDeleted =0
	and (ts.Code = 'Pending' OR ts.Code = 'Due' OR ts.Code = 'Overdue'  OR ts.Code = 'Delinquent')
	And (ats.ReferenceId=@TransactionNo or ISNULL(@TransactionNo,'')='')
	And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)
	-- And ats.TransactionStatusId = @TransactionStatusesID_Overdue  and (cast(DATEADD(day, 1, ats.DueDate) as Date) <= Cast(GETDATE() as Date) or ats.DueDate is null)
	And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	group by ats.Id, ats.ReferenceId, ats.DelayPenaltyRate, ats.GstRate ,ats.PayableBy, ats.InvoiceNo,b.WithdrawalId,ats.InvoiceId 

	select    tmp.*
			, abs(ISNULL(x.PaymentAmount, 0)) PaymentAmount
			, abs(ISNULL(x.ExtraPaymentAmount, 0)) ExtraPaymentAmount
			, abs(ISNULL(x.InterestPaymentAmount, 0)) InterestPaymentAmount
			, abs(ISNULL(x.BouncePaymentAmount, 0)) BouncePaymentAmount
			, abs(ISNULL(x.PenalPaymentAmount, 0)) PenalPaymentAmount
			, abs(ISNULL(x.OverduePaymentAmount, 0)) OverduePaymentAmount
			, abs(ISNULL(tmp.PricipleAmount, 0) + ISNULL( x.PaymentAmount, 0)) as Outstanding
			, abs(ISNULL(pats.Id, 0)) PaneltyTxnId 
			, ROUND(IsNull(patsAmt.PenaltyChargesAmount,0),2)  as PenaltyChargesAmount
			, ROUND(IsNull(ovrAmt.OverdueCharges,0) , 2) as OverdueInterestAmount			
			, (tmp.PricipleAmount + IsNull(x.PaymentAmount,0)) * ((tmp.DelayPenaltyRate/100)/30) as DelayPenalityAmount
			, ((tmp.PricipleAmount + IsNull(x.PaymentAmount,0)) * ((tmp.DelayPenaltyRate/100)/30)) * (tmp.GstRate/100) as DelayPenalityGstAmount			
	from #tempTransaction tmp 
	outer apply 
	(
		select   SUM(CASE WHEN h.Code = 'Payment' THEN ad.Amount ELSE 0 END ) PaymentAmount 
				,SUM(CASE WHEN h.Code = 'InterestPaymentAmount' THEN ad.Amount ELSE 0 END ) InterestPaymentAmount
				,SUM(CASE WHEN h.Code = 'ExtraPaymentAmount' THEN ad.Amount ELSE 0 END ) ExtraPaymentAmount
				,SUM(CASE WHEN h.Code = 'BouncePaymentAmount' THEN ad.Amount ELSE 0 END ) BouncePaymentAmount
				,SUM(CASE WHEN h.Code = 'PenalPaymentAmount' THEN ad.Amount ELSE 0 END ) PenalPaymentAmount
				,SUM(CASE WHEN h.Code = 'OverduePaymentAmount' THEN ad.Amount ELSE 0 END ) OverduePaymentAmount
		from  AccountTransactions ats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
			--and (h.Code = 'Payment' ) 
		where tmp.Id = ats.ParentAccountTransactionsID and t.Code = 'OrderPayment'
		And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)x
	outer apply(	
		select pats.Id from   
		AccountTransactions pats  with(nolock) 
		left join TransactionTypes pt  with(nolock)  on pats.TransactionTypeId = pt.Id
		where  pt.Code = 'PenaltyCharges' and tmp.Id = pats.ParentAccountTransactionsID
	)pats
	outer apply(
		select IsNull(sum(ad.Amount),0) PenaltyChargesAmount from   
		AccountTransactions pats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on pats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on pats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads ph  with(nolock) on ad.TransactionDetailHeadId  = ph.Id 
		where tmp.Id = pats.ParentAccountTransactionsID  and t.Code ='PenaltyCharges' and ph.Code = 'DelayPenalty' 
		And pats.IsActive=1 And pats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)patsAmt
	outer apply(
		select IsNull(sum(ad.Amount),0) OverdueCharges from   
		AccountTransactions pats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on pats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on pats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads ph  with(nolock) on ad.TransactionDetailHeadId  = ph.Id 
		where tmp.Id = pats.ParentAccountTransactionsID and  t.Code = 'OverdueInterest' and ph.Code = 'OverdueInterestAmount'
		And pats.IsActive=1 And pats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)ovrAmt
	order by tmp.transactionReqNo 
End
GO
/****** Object:  StoredProcedure [dbo].[GetOverDueTransactionsList]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER Procedure [dbo].[GetOverDueTransactionsList]
--Declare
@TransactionNo varchar(20) ='' --= 'HQDUD33EYW0Q3RP4'
, @LoanAccountId bigint =NULL --= 15
, @DateOfCalculation datetime= '2024-02-28' --='2024-02-18'
As 
Begin
	Declare @TransactionStatusesID_Overdue bigint
		Select @TransactionStatusesID_Overdue=Id From TransactionStatuses with(nolock)  Where Code='Overdue'
		

	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 

	select    ats.Id AccountTransactionId
			, ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN ad.Amount ELSE 0 END), 0) PricipleAmount  
			,ats.InterestRate ,l.NBFCIdentificationCode,ats.LoanAccountId
	into #tempTransaction 
	from AccountTransactions ats  with(nolock) 
	inner join LoanAccounts l with(nolock) on ats.LoanAccountId=l.Id
	inner join TransactionStatuses ts with(nolock) on ats.TransactionStatusId=ts.Id
	inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
	inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
	inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
	where  t.Code = 'OrderPlacement'
	And (ats.ReferenceId=@TransactionNo or ISNULL(@TransactionNo,'')='')
	And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)
	-- And ats.TransactionStatusId = @TransactionStatusesID_Overdue  and (cast(DATEADD(day, 1, ats.DueDate) as Date) <= Cast(GETDATE() as Date) or ats.DueDate is null)
	And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	and CAST(ats.DueDate as date) < CAST(@DateOfCalculation as DAte)
	and ts.Code in ('Pending','Due','Overdue') and ats.DisbursementDate is not null
	group by ats.Id,ats.InterestRate,l.NBFCIdentificationCode,ats.LoanAccountId


	select    tmp.*
			, abs(ISNULL(x.PaymentAmount, 0)) PaymentAmount
			, tmp.PricipleAmount - abs(ISNULL(x.PaymentAmount, 0)) PrincipleOutstanding
	from #tempTransaction tmp 
	outer apply 
	(
		select   SUM(CASE WHEN h.Code = 'Payment' THEN ad.Amount ELSE 0 END ) PaymentAmount 
		from  AccountTransactions ats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
			--and (h.Code = 'Payment' ) 
		where tmp.AccountTransactionId = ats.ParentAccountTransactionsID and t.Code = 'OrderPayment' and cast(ad.TransactionDate as date) <= cast(@DateOfCalculation as date)
		And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)x
	
End
GO
/****** Object:  StoredProcedure [dbo].[GetPaymentOutstanding]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER Procedure [dbo].[GetPaymentOutstanding]

--declare
	@LoanAccountId bigint=35, --= 15
	@AnchorCompanyID bigint =0

as
Begin
	

	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 

	SELECT t.Id ,
	       sum(Isnull(atd.Amount,0)) as TotalAmount
	into #tempTransaction 
	FROM AccountTransactions t
	inner join TransactionStatuses ts on t.TransactionStatusId = ts.Id  and ts.IsActive=1 and ts.IsDeleted=0
	inner join TransactionTypes tt  with(nolock) on t.TransactionTypeId = tt.Id  and tt.IsActive=1 and tt.IsDeleted=0
	inner join AccountTransactionDetails atd with(nolock) on t.Id = atd.AccountTransactionId  and atd.IsActive=1 and atd.IsDeleted=0
	inner join TransactionDetailHeads h with(nolock) on h.Id=atd.TransactionDetailHeadId and h.IsActive=1 and h.IsDeleted=0
	WHERE (@AnchorCompanyID = 0 OR t.AnchorCompanyId = @AnchorCompanyID)
		and t.LoanAccountId = @LoanAccountId 
		and  tt.Code = 'OrderPlacement'
		and (ts.Code != 'Paid' AND ts.Code != 'Failed'  AND ts.Code != 'Initiate'  AND ts.Code != 'Canceled')
		and t.DisbursementDate is not null 
		and t.IsActive=1 and t.IsDeleted=0 and (atd.TransactionDate is null or cast(atd.TransactionDate as date) <= cast(getdate() as date))
		and cast(t.DueDate as date) <=  cast(getdate() as date)
		and t.IsActive =1 and t.IsDeleted =0
		group by  t.Id
		--order by t.InvoiceDate desc

	select  
			IsNull(SUM(ISNULL(tt.TotalAmount, 0)),0) + isnull(SUM(ISNULL(x.Amount, 0)),0)  as TotalPayableAmount ,
			Count(Distinct tt.Id) TotalPendingInvoiceCount
	from  #tempTransaction tt 

outer apply(
             select
			       SUM(ISNULL(atd.Amount, 0)) Amount
			 from AccountTransactions at with(nolock) 
			 inner join AccountTransactionDetails atd with(nolock) on at.Id = atd.AccountTransactionId
			 where tt.Id = at.ParentAccountTransactionsID  and (atd.TransactionDate  IS NULL OR CAST(atd.TransactionDate as DATE) <= GETDATE())
	         and at.IsActive =1 and at.IsDeleted =0 and atd.IsActive =1 and atd.IsDeleted =0
			 --group by at.ParentAccountTransactionsID 
)x	

End

GO
/****** Object:  StoredProcedure [dbo].[GetPenaltyBounceChargesByTxnId]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

 Create Proc [dbo].[GetPenaltyBounceChargesByTxnId]
--declare
 @TransactionId varchar(100)='35YO33ACG9T48CR5',
 @Type varchar(100)='PenaltyCharges'
 As 
 Begin

	select 
	 s.Code
	,p.ReferenceId
	,p.PaidAmount PaidAmount
	,p.GSTAmount
	,(p.PaidAmount+p.GSTAmount) TotalAmount
	,t.Code Type
	,p.TransactionTypeId
	
	from  AccountTransactions P with(nolock)
	inner join TransactionTypes t with(nolock) on p.TransactionTypeId=t.Id
	inner join TransactionStatuses s with(nolock) on s.Id=p.TransactionStatusId 
	where p.ReferenceId=@TransactionId and t.Code= @Type

	End
GO
/****** Object:  StoredProcedure [dbo].[GetPenaltyChargesByTxnId]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


 create Proc [dbo].[GetPenaltyChargesByTxnId]
--declare
 @TransactionId varchar(100)='',
 @Type varchar(100)=''
 As 
 Begin

	select 
	 s.Code
	,p.ReferenceId
	,p.PaidAmount PaidAmount
	,p.GSTAmount
	,(p.PaidAmount+p.GSTAmount) TotalAmount
	,t.Code Type
	,p.TransactionTypeId
	
	from  AccountTransactions P with(nolock)
	inner join TransactionTypes t with(nolock) on p.TransactionTypeId=t.Id
	inner join TransactionStatuses s with(nolock) on s.Id=p.TransactionStatusId 
	where p.ReferenceId=@TransactionId and t.Code= @Type

	End

GO
/****** Object:  StoredProcedure [dbo].[GetTransactionHistoryDetail]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER Procedure [dbo].[GetTransactionHistoryDetail]
@TransactionId varchar(100)
As
Begin
select A.Amount as Amount,A.PaymentReqNo as PaymentReferenceNo,A.Created as PaymentDate,A.PaymentMode,0 as RemainingAmount from AccountTransactionDetails  A Inner Join TransactionDetailHeads M ON A.TransactionDetailHeadId=M.Id 
Where A.AccountTransactionId IN (select A.Id from AccountTransactions A 
Inner Join TransactionStatuses S ON A.TransactionStatusId=S.Id
Inner Join TransactionTypes T ON A.TransactionTypeId=T.ID where ReferenceId=@TransactionId)
and A.TransactionDetailHeadId=7
Order by A.Id asc
End
GO
/****** Object:  StoredProcedure [dbo].[GetTransactionList]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	CREATE Proc [dbo].[GetTransactionList]
	--Declare
	@Status varchar(50)='Paid',
	@SearchKeyward varchar(50)='',
	@skip int=0,
	@take int=1000,
	@CityName varchar(50)='',
	@AnchorId bigint=0,
	@FromDate datetime='2024-02-01',
	@ToDate datetime='2024-03-25',
	@LoanAccountId bigint=0
	
	As 
	Begin
	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 

	select    ats.Id 
			, ats.ReferenceId As transactionReqNo
			, s.code Status
			, ats.DelayPenaltyRate
			, ats.GstRate
			, ats.PayableBy
			, IsNull(ats.InvoiceNo,'') As InvoiceNo
			, s.Code
			, ISNULL(SUM(CASE WHEN (h.Code = 'Order' OR h.Code = 'Refund') THEN ad.Amount ELSE 0 END), 0) PricipleAmount  
			, ISNULL(SUM(CASE WHEN h.Code = 'Interest' THEN ad.Amount ELSE 0 END), 0) FullInterestAmount  
			, ISNULL(SUM(CASE WHEN h.Code = 'Interest' And (Cast(ad.TransactionDate as Date)<= Cast(GETDATE() as Date) ) THEN ad.Amount ELSE 0 END), 0) InterestAmount  
			, ats.InvoiceId
			, Isnull(cast(ats.DueDate as date),'') DueDate
			, Isnull(cast(ats.Created as date),'') TransactionDate
			, dense_rank() over (order by ats.Id) + dense_rank() over (order by ats.Id  desc) -1 TotalCount
			, CASE WHEN pats.Id IS NOT NULL AND  s.code != 'Paid' THEN 'YES' ELSE 'NO' END as PartiaPaidStatus 
			,LeadCode
			,AccountCode
			,MobileNo
			,CustomerName	
			,AnchorName UtilizationAnchor	
			,l.AnchorCompanyId	
			,CityName
			,ats.LoanAccountId
			,i.OrderNo OrderId
			,pats.SettlementDate
			,cast(ats.DisbursementDate as date) DisbursementDate
			,l.ThirdPartyLoanCode
	into #tempTransaction 
	from AccountTransactions ats  with(nolock) 
	inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
	inner join TransactionStatuses s with(nolock) on s.id=ats.TransactionStatusId
	inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
	inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
	inner join LoanAccounts l with(nolock) on ats.LoanAccountId=l.Id 
	left join Invoices i with(nolock) on i.Id=ats.InvoiceId
	outer apply(
		select MAX(pats.Id) as Id, cast(MAX(pats.Created) as date) SettlementDate 
		from AccountTransactions pats  with(nolock) 
		inner join TransactionTypes pt  with(nolock) on pats.TransactionTypeId = pt.Id
		where ats.Id = pats.ParentAccountTransactionsID and pt.Code =  'OrderPayment'
	)pats

	where  t.Code = 'OrderPlacement'
	And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)	
	And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	and 1=(case when @Status='Due' and DueDate= cast(Getdate() as date) then 1
				when (@Status='Pending' or @Status='Initiate' or @Status='Intransit') and DueDate>=@FromDate and DueDate <=@ToDate then 1
				when @Status='Overdue' and cast(DATEADD(day,1,DueDate) as date)>=@FromDate and cast(DATEADD(day,1,DueDate) as date)<=@ToDate then 1
				when @Status='All' and DueDate>=@FromDate and DueDate<=@ToDate then 1
				when @Status='Paid' and s.Code='Paid' and cast(ats.Created as date)>=@FromDate and cast(ats.Created as date)<=@ToDate then 1
				else 1
		   end)
			and (s.Code=@Status or @Status='All' OR (@Status = 'Partial' and pats.Id IS NOT NULL AND  s.code != 'Paid'))
			And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)
	    And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
		And l.IsActive=1 and l.IsDeleted=0 --and l.IsBlock=0
		and ((l.CustomerName like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (ats.ReferenceId like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (l.AccountCode like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (l.MobileNo like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')=''))
		And (l.CityName=@CityName or Isnull(@CityName,'')='')
		And (l.AnchorCompanyId=@AnchorId or Isnull(@AnchorId,0)=0)

	group by ats.Id, ats.ReferenceId, ats.DelayPenaltyRate, ats.GstRate ,ats.PayableBy, ats.InvoiceNo,ats.InvoiceId,s.Code
	, pats.Id
	,cast(ats.DueDate as date),cast(ats.Created as date)
	,LeadCode,AccountCode,MobileNo,CustomerName,AnchorName,l.AnchorCompanyId,CityName,ats.LoanAccountId
	,pats.SettlementDate
	,i.OrderNo,cast(ats.DisbursementDate as date) ,l.ThirdPartyLoanCode

	order by ats.Id desc
	OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY

	select Id ParentID,transactionReqNo ReferenceId, 
		Code ,
		PricipleAmount, 
		PaymentAmount,
		 InterestAmount, 
		FullInterestAmount,
		PenaltyAmount,
		OverdueInterestAmount,
		((PricipleAmount+FullInterestAmount)) ActualOrderAmount,
		isnull(round((PaymentAmount+ExtraPaymentAmount+InterestPaymentAmount+OverduePaymentAmount+BouncePaymentAmount+PenalPaymentAmount),2,2),0) PaidAmount,
		isnull(round((PricipleAmount+InterestAmount+PenaltyAmount+OverdueInterestAmount),2,2),0) OutstandingAmount,

		case when Code!='Canceled' then
		isnull(round((PricipleAmount+InterestAmount+PenaltyAmount+OverdueInterestAmount)
		-(PaymentAmount+ExtraPaymentAmount+InterestPaymentAmount+OverduePaymentAmount+BouncePaymentAmount+PenalPaymentAmount),2,2),0)
		else 0 end
		as PayableAmount,

		case when PaymentAmount>0 and (PricipleAmount-PaymentAmount)>0 then (PricipleAmount-PaymentAmount) else 0 end PartialPayment,
		case when PaymentAmount>0 and (PricipleAmount-PaymentAmount)>0 then (((PricipleAmount+round(InterestAmount,2)+PenaltyAmount))-((PricipleAmount+ (case when Status!='Paid' then (InterestAmount) else 0 end)+PenaltyAmount)
		-(PaymentAmount+ExtraPaymentAmount+InterestPaymentAmount+OverduePaymentAmount+BouncePaymentAmount+PenalPaymentAmount) )) else 0 end ReceivedPayment,
		DueDate,
		TransactionDate,
		PaymentDate,
		--PaymentAmount,
		ExtraPaymentAmount, 
		InterestPaymentAmount,	
		OverduePaymentAmount,
		BouncePaymentAmount, 
		PenalPaymentAmount, 
		
		TotalCount,
		AccountCode
		,LeadCode
			,MobileNo
			,CustomerName	
			,UtilizationAnchor	
			,AnchorCompanyId	
			,CityName
			,LoanAccountId
			,IsNull(OrderId,'') OrderId
			,'' PaymentMode
			--,cast(0 as float) ReceivedPayment
			,IsNull(SettlementDate,'') SettlementDate
			,PartiaPaidStatus
			, abs(case when DueDate<>'1900-01-01' then DATEDIFF(DAY,GETDATE(),DueDate) else 0 end) Aging
			,isnull(InvoiceNo,'') InvoiceNo
			,IsNull(DisbursementDate,'') DisbursementDate
			,Isnull(ThirdPartyLoanCode,'') ThirdPartyLoanCode
			

	from (
		select    tmp.*
			, abs(ISNULL(x.PaymentAmount, 0)) PaymentAmount
			, abs(ISNULL(x.ExtraPaymentAmount, 0)) ExtraPaymentAmount
			, abs(ISNULL(x.InterestPaymentAmount, 0)) InterestPaymentAmount
			, abs(ISNULL(x.OverduePaymentAmount, 0)) OverduePaymentAmount
			, abs(ISNULL(x.BouncePaymentAmount, 0)) BouncePaymentAmount
			, abs(ISNULL(x.PenalPaymentAmount, 0)) PenalPaymentAmount
			, abs(ISNULL(tmp.PricipleAmount, 0) + ISNULL( x.PaymentAmount, 0)) as Outstanding
			--, abs(ISNULL(pats.Id, 0)) PaneltyTxnId 
			, (tmp.PricipleAmount + IsNull(x.PaymentAmount,0)) * ((tmp.DelayPenaltyRate/100)/30) as DelayPenalityAmount
			, ((tmp.PricipleAmount + IsNull(x.PaymentAmount,0)) * ((tmp.DelayPenaltyRate/100)/30)) * (tmp.GstRate/100) as DelayPenalityGstAmount
			,X.PaymentDate,isnull(pats.PenaltyAmount,0) PenaltyAmount
			,ISNULL(Overdue.OverdueInterestAmount,0) OverdueInterestAmount
		from #tempTransaction tmp 
	outer apply 
	(
		select   SUM(CASE WHEN h.Code = 'Payment' THEN ad.Amount ELSE 0 END ) PaymentAmount 
				,SUM(CASE WHEN h.Code = 'InterestPaymentAmount' THEN ad.Amount ELSE 0 END ) InterestPaymentAmount
				,SUM(CASE WHEN h.Code = 'OverduePaymentAmount' THEN ad.Amount ELSE 0 END ) OverduePaymentAmount
				,SUM(CASE WHEN h.Code = 'ExtraPaymentAmount' THEN ad.Amount ELSE 0 END ) ExtraPaymentAmount
				,SUM(CASE WHEN h.Code = 'BouncePaymentAmount' THEN ad.Amount ELSE 0 END ) BouncePaymentAmount
				,SUM(CASE WHEN h.Code = 'PenalPaymentAmount' THEN ad.Amount ELSE 0 END ) PenalPaymentAmount
				,MAX(CASE WHEN h.Code = 'Payment' THEN ad.PaymentDate ELSE null END ) PaymentDate 
		from  AccountTransactions ats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
			--and (h.Code = 'Payment' ) 
		where tmp.Id = ats.ParentAccountTransactionsID and t.Code = 'OrderPayment'
		And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)x
	outer apply(
		select sum(IsNull(ad.Amount,0)) PenaltyAmount from   
		AccountTransactions pats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on pats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on pats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads ph  with(nolock) on ad.TransactionDetailHeadId  = ph.Id 
			--and (h.Code = 'Payment' ) 
		where tmp.Id = pats.ParentAccountTransactionsID and ph.Code in ('DelayPenalty')
		And pats.IsActive=1 And pats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)pats
	outer apply(
		select sum(IsNull(ad.Amount,0)) OverdueInterestAmount from   
		AccountTransactions pats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on pats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on pats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads ph  with(nolock) on ad.TransactionDetailHeadId  = ph.Id 
			--and (h.Code = 'Payment' ) 
		where tmp.Id = pats.ParentAccountTransactionsID and ph.Code in ('OverdueInterestAmount')
		And pats.IsActive=1 And pats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)Overdue
	) x
	end
GO
/****** Object:  StoredProcedure [dbo].[GetTransactionListExport]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	CREATE Proc [dbo].[GetTransactionListExport]
	--Declare
	@Status varchar(50)='All',
	@SearchKeyward varchar(50)='',
	@skip int=0,
	@take int=0,
	@CityName varchar(50)='',
	@AnchorId bigint=0,
	@FromDate datetime='2024-02-01',
	@ToDate datetime='2024-03-25',
	@LoanAccountId bigint=0
	
	As 
	Begin
	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 

	select    ats.Id 
			, ats.ReferenceId As transactionReqNo
			, s.code Status
			, ats.DelayPenaltyRate
			, ats.GstRate
			, ats.PayableBy
			, IsNull(ats.InvoiceNo,'') As InvoiceNo
			, s.Code
			, ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN ad.Amount ELSE 0 END), 0) PricipleAmount  
			, ISNULL(SUM(CASE WHEN h.Code = 'Interest' THEN ad.Amount ELSE 0 END), 0) FullInterestAmount  
			, ISNULL(SUM(CASE WHEN h.Code = 'Interest' And (Cast(ad.TransactionDate as Date)<= Cast(GETDATE() as Date)) THEN ad.Amount ELSE 0 END), 0) InterestAmount  
			, ats.InvoiceId
			, Isnull(cast(ats.DueDate as date),'') DueDate
			, Isnull(cast(ats.Created as date),'') TransactionDate
			, dense_rank() over (order by ats.Id) + dense_rank() over (order by ats.Id  desc) -1 TotalCount
			, CASE WHEN pats.Id IS NOT NULL AND  s.code != 'Paid' THEN 'YES' ELSE 'NO' END as PartiaPaidStatus 
			,LeadCode
			,AccountCode
			,MobileNo
			,CustomerName	
			,AnchorName UtilizationAnchor	
			,l.AnchorCompanyId	
			,CityName
			,ats.LoanAccountId
			,i.OrderNo OrderId
			,pats.SettlementDate
			,cast(ats.DisbursementDate as date) DisbursementDate
			,l.ThirdPartyLoanCode
	into #tempTransaction 
	from AccountTransactions ats  with(nolock) 
	inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
	inner join TransactionStatuses s with(nolock) on s.id=ats.TransactionStatusId
	inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
	inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
	inner join LoanAccounts l with(nolock) on ats.LoanAccountId=l.Id 
	left join Invoices i with(nolock) on i.Id=ats.InvoiceId
	outer apply(
		select MAX(pats.Id) as Id, cast(MAX(pats.Created) as date) SettlementDate 
		from AccountTransactions pats  with(nolock) 
		inner join TransactionTypes pt  with(nolock) on pats.TransactionTypeId = pt.Id
		where ats.Id = pats.ParentAccountTransactionsID and pt.Code =  'OrderPayment'
	)pats

	where  t.Code = 'OrderPlacement'
	And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)	
	And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	and 1=(case when @Status='Due' and DueDate= cast(Getdate() as date) then 1
				when (@Status='Pending' or @Status='Initiate' or @Status='Intransit') and DueDate>=@FromDate and DueDate <=@ToDate then 1
				when @Status='Overdue' and cast(DATEADD(day,1,DueDate) as date)>=@FromDate and cast(DATEADD(day,1,DueDate) as date)<=@ToDate then 1
				when @Status='All' and DueDate>=@FromDate and DueDate<=@ToDate then 1
				when @Status='Paid' and s.Code='Paid' and cast(ats.Created as date)>=@FromDate and cast(ats.Created as date)<=@ToDate then 1
				else 1
		   end)
			and (s.Code=@Status or @Status='All' OR (@Status = 'Partial' and pats.Id IS NOT NULL AND  s.code != 'Paid'))
			And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)
	    And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
		And l.IsActive=1 and l.IsDeleted=0 --and l.IsBlock=0
		and ((l.CustomerName like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (ats.ReferenceId like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (l.AccountCode like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (l.MobileNo like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')=''))
		And (l.CityName=@CityName or Isnull(@CityName,'')='')
		And (l.AnchorCompanyId=@AnchorId or Isnull(@AnchorId,0)=0)

	group by ats.Id, ats.ReferenceId, ats.DelayPenaltyRate, ats.GstRate ,ats.PayableBy, ats.InvoiceNo,ats.InvoiceId,s.Code
	, pats.Id
	,cast(ats.DueDate as date),cast(ats.Created as date)
	,LeadCode,AccountCode,MobileNo,CustomerName,AnchorName,l.AnchorCompanyId,CityName,ats.LoanAccountId
	,pats.SettlementDate
	,i.OrderNo,cast(ats.DisbursementDate as date) ,l.ThirdPartyLoanCode

	order by ats.Id desc
	

	select Id ParentID,transactionReqNo ReferenceId, 
		Code Status,
		PricipleAmount, 
		PaymentAmount,
		 InterestAmount, 
		FullInterestAmount,
		PenaltyAmount,
		OverdueInterestAmount,
		((PricipleAmount+FullInterestAmount)) ActualOrderAmount,
		isnull((PaymentAmount+ExtraPaymentAmount+InterestPaymentAmount+OverduePaymentAmount+BouncePaymentAmount+PenalPaymentAmount),0) PaidAmount,
		isnull(((PricipleAmount+InterestAmount+PenaltyAmount+OverdueInterestAmount)),0) OutstandingAmount,

		--(PricipleAmount+ (case when Status!='Paid' then InterestAmount else 0 end)+PenaltyAmount)
		---(PaymentAmount+ExtraPaymentAmount+InterestPaymentAmount+BouncePaymentAmount+PenalPaymentAmount) as PayableAmount2,

		isnull((PricipleAmount+InterestAmount+PenaltyAmount+OverdueInterestAmount)
		-(PaymentAmount+ExtraPaymentAmount+InterestPaymentAmount+OverduePaymentAmount+BouncePaymentAmount+PenalPaymentAmount),0) as PayableAmount,

		case when PaymentAmount>0 and (PricipleAmount-PaymentAmount)>0 then (PricipleAmount-PaymentAmount) else 0 end PartialPayment,
		case when PaymentAmount>0 and (PricipleAmount-PaymentAmount)>0 then (((PricipleAmount+round(InterestAmount,2)+PenaltyAmount))-((PricipleAmount+ (case when Status!='Paid' then (InterestAmount) else 0 end)+PenaltyAmount)
		-(PaymentAmount+ExtraPaymentAmount+InterestPaymentAmount+OverduePaymentAmount+BouncePaymentAmount+PenalPaymentAmount) )) else 0 end ReceivedPayment,
		DueDate,
		TransactionDate,
		PaymentDate,
		--PaymentAmount,
		ExtraPaymentAmount, 
		InterestPaymentAmount,	
		OverduePaymentAmount,
		BouncePaymentAmount, 
		PenalPaymentAmount, 
		
		TotalCount,
		AccountCode
		,LeadCode
			,MobileNo
			,CustomerName	
			,UtilizationAnchor	
			,AnchorCompanyId	
			,CityName
			,LoanAccountId
			,IsNull(OrderId,'') OrderNo
			--,cast(0 as float) ReceivedPayment
			,IsNull(SettlementDate,'') SettlementDate
			,PartiaPaidStatus
			, abs(case when DueDate<>'1900-01-01' then DATEDIFF(DAY,GETDATE(),DueDate) else 0 end) Aging
			,isnull(InvoiceNo,'') InvoiceNo
			,IsNull(DisbursementDate,'') DisbursementDate
			,Isnull(ThirdPartyLoanCode,'') ThirdPartyLoanCode
			

	from (
		select    tmp.*
			, abs(ISNULL(x.PaymentAmount, 0)) PaymentAmount
			, abs(ISNULL(x.ExtraPaymentAmount, 0)) ExtraPaymentAmount
			, abs(ISNULL(x.InterestPaymentAmount, 0)) InterestPaymentAmount
			, abs(ISNULL(x.OverduePaymentAmount, 0)) OverduePaymentAmount
			, abs(ISNULL(x.BouncePaymentAmount, 0)) BouncePaymentAmount
			, abs(ISNULL(x.PenalPaymentAmount, 0)) PenalPaymentAmount
			, abs(ISNULL(tmp.PricipleAmount, 0) + ISNULL( x.PaymentAmount, 0)) as Outstanding
			--, abs(ISNULL(pats.Id, 0)) PaneltyTxnId 
			, (tmp.PricipleAmount + IsNull(x.PaymentAmount,0)) * ((tmp.DelayPenaltyRate/100)/30) as DelayPenalityAmount
			, ((tmp.PricipleAmount + IsNull(x.PaymentAmount,0)) * ((tmp.DelayPenaltyRate/100)/30)) * (tmp.GstRate/100) as DelayPenalityGstAmount
			,X.PaymentDate,isnull(pats.PenaltyAmount,0) PenaltyAmount
			,ISNULL(Overdue.OverdueInterestAmount,0) OverdueInterestAmount
		from #tempTransaction tmp 
	outer apply 
	(
		select   SUM(CASE WHEN h.Code = 'Payment' THEN ad.Amount ELSE 0 END ) PaymentAmount 
				,SUM(CASE WHEN h.Code = 'InterestPaymentAmount' THEN ad.Amount ELSE 0 END ) InterestPaymentAmount
				,SUM(CASE WHEN h.Code = 'OverduePaymentAmount' THEN ad.Amount ELSE 0 END ) OverduePaymentAmount
				,SUM(CASE WHEN h.Code = 'ExtraPaymentAmount' THEN ad.Amount ELSE 0 END ) ExtraPaymentAmount
				,SUM(CASE WHEN h.Code = 'BouncePaymentAmount' THEN ad.Amount ELSE 0 END ) BouncePaymentAmount
				,SUM(CASE WHEN h.Code = 'PenalPaymentAmount' THEN ad.Amount ELSE 0 END ) PenalPaymentAmount
				,MAX(CASE WHEN h.Code = 'Payment' THEN ad.PaymentDate ELSE null END ) PaymentDate 
		from  AccountTransactions ats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
			--and (h.Code = 'Payment' ) 
		where tmp.Id = ats.ParentAccountTransactionsID and t.Code = 'OrderPayment'
		And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)x
	outer apply(
		select sum(IsNull(ad.Amount,0)) PenaltyAmount from   
		AccountTransactions pats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on pats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on pats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads ph  with(nolock) on ad.TransactionDetailHeadId  = ph.Id 
			--and (h.Code = 'Payment' ) 
		where tmp.Id = pats.ParentAccountTransactionsID and ph.Code in ('DelayPenalty')
		And pats.IsActive=1 And pats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)pats
	outer apply(
		select sum(IsNull(ad.Amount,0)) OverdueInterestAmount from   
		AccountTransactions pats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on pats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on pats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads ph  with(nolock) on ad.TransactionDetailHeadId  = ph.Id 
			--and (h.Code = 'Payment' ) 
		where tmp.Id = pats.ParentAccountTransactionsID and ph.Code in ('OverdueInterestAmount')
		And pats.IsActive=1 And pats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)Overdue
	) x
	end
GO
/****** Object:  StoredProcedure [dbo].[spGetCurrentNumber]    Script Date: 07-03-2024 18:35:24 ******/
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
	set @StateAlias ='AC'
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
/****** Object:  StoredProcedure [dbo].[SpManualTransactionSettle]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER Procedure [dbo].[SpManualTransactionSettle]
  @TransactionId varchar(20) ,--='ZRER2GFWC4C3PC44',	
  @SettleAmt float			 ,--=100.7,  
  @username varchar(50)		 --='system',  
As 
Begin

	declare   
	--@TrnAmount Decimal(18,2)=0, @PaidAmount Decimal(18,2), @result  varchar(50)
	@TrnAmount float, @PaidAmount float, @result  varchar(50)
	,   @PaymentRefNo varchar(300)
	Declare @amount float=0

	SET @TrnAmount =(SELECT isnull(abs(sum(PaidAmount)),0.00) as TrnAmount 
					from AccountTransactions  With(Nolock) 
					Where ReferenceId= @TransactionId and  TransactionTypeId in (Select Id from TransactionTypes Where Code not in ('OrderPayment') )
					and IsActive =1 and IsDeleted =0  )
	
	SET @PaidAmount	=(select  isnull(abs(sum(PaidAmount)),0.00) as PaidAmount 
					  from AccountTransactions  With(Nolock) Where ReferenceId= @TransactionId 
					  and TransactionTypeId in (Select Id from TransactionTypes Where Code in ('OrderPayment') )  and IsActive =1 and IsDeleted =0  )

	set @amount= @TrnAmount - (@PaidAmount+@SettleAmt)

	If (@TrnAmount <> @PaidAmount )
		BEGIN
			--if (select  /*COUNT(PaymentRefNo)*/ top 1 1 TotalPaymentRefNo from AccountTransactions  With(Nolock)
			--		where TransactionTypeId in (Select Id from TransactionTypes Where Code in ('OrderPayment') ) 
			--		And ReferenceId=@TransactionId 
			--		--And PaymentRefNo=@PaymentRefNo group by PaymentRefNo Having COUNT(PaymentRefNo)>0
			--		)>0
			--	Begin
			--		set @result=  'error: Same PaymentRefNo exist' 
			--	End
			--else
				Begin
					set @result= 'Process' 

				--set @amount=(select abs(sum(cast(a.Amount as decimal(18,2)))) from AccountTransaction a with (nolock)
				--				where a.TrasanctionId=@TransactionId and a.TrasanctionType not in(1,11) and a.IsActive=1 and a.IsDelete=0) 
				End
		END
	Else
		 BEGIN
			 set @result= 'Failed'
		 END
	  
	
	select @result as TransactionStatus, round(@TrnAmount, 2) as TrnAmount, @PaidAmount as PaidAmount, @SettleAmt As SettlementAmount,	@amount as ReaminAmounts

End
GO
/****** Object:  StoredProcedure [dbo].[TransactionDetail]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


	CREATE OR ALTER Procedure [dbo].[TransactionDetail]
	--declare
	--@ReferenceId varchar(100)='2024354'
	@AccountTransactionsId bigint=576
	AS
	Begin
		if(@AccountTransactionsId != 0)
		begin
			
			select * ,Cast(round(Sum(TxnAmount) over(),2) as float) TotalAmount 
			from (
	select   ats.Id AccountTransactionId
			,h.Code TransactionType
			,cast(Max(ad.TransactionDate) as date) TransactionDate,
			-- ROUND(ROUND(Sum(ISNULL(ad.Amount,0)),2,2),2) TxnAmount
			(ROUND(Sum(ISNULL(ad.Amount,0)),2)) TxnAmount
			,h.SequenceNo	
			--,Sum(ad.Amount) over() totalAmount
			from AccountTransactions ats  with(nolock) 
			inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
			inner join TransactionStatuses s with(nolock) on s.id=ats.TransactionStatusId
			inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
			inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
			where (ats.Id=@AccountTransactionsID or ats.ParentAccountTransactionsID=@AccountTransactionsID)
			and (ad.TransactionDate is null or Cast(ad.TransactionDate as Date)<= Cast(GETDATE() as Date))
			and ad.IsActive=1 
			group by ats.Id,s.Code,h.Code,SequenceNo
			) x
			order by SequenceNo
		end
		else
		begin
		select 'ReferenceId is null'
		end
	End



GO
/****** Object:  StoredProcedure [dbo].[UpdateTransactionToDueStatus]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER Procedure [dbo].[UpdateTransactionToDueStatus]
as
begin
	Declare @CurrentDate datetime = GetDate()
	--select @CurrentDate, DATEADD(day, -3,@CurrentDate) 
	----set  @CurrentDate= DATEADD(day, -2,@CurrentDate) 

	Declare @TransactionStatusesID_Overdue bigint
	Declare @TransactionStatusesID_Pending bigint
	Declare @TransactionStatusesID_Due bigint, @TransactionStatusesID_Delinquent bigint

	Select @TransactionStatusesID_Pending=Id From TransactionStatuses with(nolock)  Where Code='Pending'	
	Select @TransactionStatusesID_Due=Id From TransactionStatuses with(nolock)  Where Code='Due'
	Select @TransactionStatusesID_Overdue=Id From TransactionStatuses with(nolock)  Where Code='Overdue'
	Select @TransactionStatusesID_Delinquent=Id From TransactionStatuses with(nolock)  Where Code='Delinquent'

	 IF Object_id('tempdb..#TxnDTransctionStatus') IS NOT NULL
	  DROP TABLE #TxnDTransctionStatus

	   CREATE TABLE #TxnDTransctionStatus([TBLId] bigint IDENTITY(1, 1) PRIMARY KEY, TrasanctionId varchar(200))

	    Insert into #TxnDTransctionStatus
	    Select Distinct a.ReferenceId
	    from AccountTransactions a with(nolock) WHERE a.IsDeleted=0 AND a.IsActive=1 
		and a.TransactionStatusId=@TransactionStatusesID_Pending And ((cast(a.DueDate as date) =cast(@CurrentDate as date)))

		--select *from  #TxnDTransctionStatus
		update ac
		set ac.TransactionStatusId=@TransactionStatusesID_Due
		from AccountTransactions ac with (nolock)
		where exists(select 1 from #TxnDTransctionStatus ss where ss.TrasanctionId=ac.ReferenceId )



		IF Object_id('tempdb..#TxnDTransctionStatusForOverDue') IS NOT NULL
	        DROP TABLE #TxnDTransctionStatusForOverDue
		CREATE TABLE #TxnDTransctionStatusForOverDue([TBLId] bigint IDENTITY(1, 1) PRIMARY KEY, TrasanctionId varchar(200))

	    Insert into #TxnDTransctionStatusForOverDue
		Select distinct a.ReferenceId
	    from AccountTransactions a with(nolock) WHERE a.IsDeleted=0 AND a.IsActive=1 
		and a.TransactionStatusId=@TransactionStatusesID_Due And ((cast(DATEADD(day, 1, a.DueDate) as date) =cast(@CurrentDate as date)))


		--select *from #TxnDTransctionStatusForOverDue
		update ac
		set ac.TransactionStatusId=@TransactionStatusesID_Overdue
		from AccountTransactions ac with (nolock)
		where exists(select 1 from #TxnDTransctionStatusForOverDue ss where ss.TrasanctionId=ac.ReferenceId )


		---- For Delinquent ----
		update a
		set a.TransactionStatusId=@TransactionStatusesID_Delinquent
	    from AccountTransactions a with(nolock) WHERE a.IsDeleted=0 AND a.IsActive=1 
		and a.TransactionStatusId=@TransactionStatusesID_Overdue And ((cast(DATEADD(day, (1+90), a.DueDate) as date) =cast(@CurrentDate as date)))
End
GO
/****** Object:  StoredProcedure [dbo].[WaiveOffBouncePenaltyInsert]    Script Date: 07-03-2024 18:35:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER Procedure [dbo].[WaiveOffBouncePenaltyInsert]
--Declare
	@TransactionNo varchar(20) --='O4A83QCGPWQEHCWE'
	, @DiscountSourceType nvarchar(100) --='PenaltyCharges' --BounceCharges/PenaltyCharges
	, @DiscountAmount float --=4
	, @RemainingGSTAmount float --=0
As 
Begin
	Declare @Result varchar(50)='Failed'
	Declare @curentDate datetime = Getdate()
	Declare @TransactionStatusesID_Paid bigint
	Declare @TransactionTypeId_PenaltyBounceCharges bigint
	Declare @TransactionDetailHeadId_PenaltyBounce bigint
	Declare @TransactionDetailHeadId_GST bigint
	Declare @TransactionDetailHeadId_Discount bigint

	Select @TransactionStatusesID_Paid=Id From TransactionStatuses with(nolock)  Where Code='Paid'
	Select @TransactionTypeId_PenaltyBounceCharges=Id From TransactionTypes with(nolock)  Where Code=@DiscountSourceType

	Select @TransactionDetailHeadId_Discount=Id From TransactionDetailHeads with(nolock)  Where Code='Discount'
	if @DiscountSourceType='PenaltyCharges'
		Select @TransactionDetailHeadId_PenaltyBounce=Id From TransactionDetailHeads with(nolock)  Where Code='DelayPenalty'
	else
		Select @TransactionDetailHeadId_PenaltyBounce=Id From TransactionDetailHeads with(nolock)  Where Code='BounceCharge'


	Declare @PenaltyBounceExist bigint=0, @GstHead varchar(10), @GstRate float=0,  @PreviousGSTAmount float=0
		,@TotalPenaltyBounceAmount float=0, @GSTAmount float=0, @TotalAmount float=0
		,@AddTotalCreditLimit float=0, @RemaingPenaltyBounceAmount float=0, @LoanAccountId float=0

	Select @PenaltyBounceExist=Id, @GstHead='GST'+Rtrim(Cast(GstRate as char)), @GstRate=GstRate, @LoanAccountId=LoanAccountId 
	from AccountTransactions with(nolock) Where ReferenceId=@TransactionNo And IsActive=1	And IsDeleted=0
	And TransactionTypeId= @TransactionTypeId_PenaltyBounceCharges ANd TransactionStatusId!=@TransactionStatusesID_Paid
			
	if @DiscountAmount=0
	Begin
		SET @Result ='Rquired DiscountAmount!'
	End
	Else

	If @PenaltyBounceExist>0
	Begin
		Select @TransactionDetailHeadId_GST=Id From TransactionDetailHeads with(nolock)  Where Code=@GstHead

		select @TotalPenaltyBounceAmount=Sum(Amount) from AccountTransactionDetails with(nolock) Where AccountTransactionId=@PenaltyBounceExist 
		And TransactionDetailHeadId= @TransactionDetailHeadId_PenaltyBounce And IsActive=1	And IsDeleted=0

		Declare @TotalOldDiscountAmount float=0
		select @TotalOldDiscountAmount=Abs(Sum(Amount)) from AccountTransactionDetails with(nolock) Where AccountTransactionId=@PenaltyBounceExist 
		And TransactionDetailHeadId= @TransactionDetailHeadId_Discount And IsActive=1	And IsDeleted=0

		select top 1 @GSTAmount=Amount from AccountTransactionDetails with(nolock) Where AccountTransactionId=@PenaltyBounceExist And Amount>0 
		And TransactionDetailHeadId= @TransactionDetailHeadId_GST And IsActive=1 And IsDeleted=0
		Order by id desc
			
		select top 1 @PreviousGSTAmount=Amount from AccountTransactionDetails with(nolock) Where AccountTransactionId=@PenaltyBounceExist And Amount>0 
		And TransactionDetailHeadId= @TransactionDetailHeadId_GST And IsActive=1 And IsDeleted=0
		Order by id desc

		--Calculation of GST exculing GST
		SET @RemaingPenaltyBounceAmount=(@TotalPenaltyBounceAmount-(IsNull(@DiscountAmount,0)+IsNull(@TotalOldDiscountAmount,0)))
		SET @RemainingGSTAmount = @RemaingPenaltyBounceAmount * (@GstRate/100)

		--For Discount
		Insert Into AccountTransactionDetails (AccountTransactionId,Amount,TransactionDetailHeadId,Created,CreatedBy,IsActive,IsDeleted,IsPayableBy)
		Values(@PenaltyBounceExist, @DiscountAmount*(-1), @TransactionDetailHeadId_Discount, @curentDate, 'System', 1,0,1)

		--For Previous GST
		Begin
			Insert Into AccountTransactionDetails (AccountTransactionId,Amount,TransactionDetailHeadId,Created,CreatedBy,IsActive,IsDeleted,IsPayableBy)
			Values(@PenaltyBounceExist, @PreviousGSTAmount * (-1), @TransactionDetailHeadId_GST, @curentDate, 'System', 1,0,1)
		End

		--For GST
		Insert Into AccountTransactionDetails (AccountTransactionId,Amount,TransactionDetailHeadId,Created,CreatedBy,IsActive,IsDeleted,IsPayableBy)
		Values(@PenaltyBounceExist, @RemainingGSTAmount, @TransactionDetailHeadId_GST, @curentDate, 'System', 1,0,1)

		Update AccountTransactions SET OrderAmount=@RemaingPenaltyBounceAmount, GSTAmount= @RemainingGSTAmount, PaidAmount=@RemaingPenaltyBounceAmount+@RemainingGSTAmount, DiscountAmount=IsNull(DiscountAmount,0)+@DiscountAmount Where ID=@PenaltyBounceExist			

		SET @AddTotalCreditLimit = @DiscountAmount + (@PreviousGSTAmount-@RemainingGSTAmount)
		Update LoanAccountCredits SET CreditLimitAmount = (CreditLimitAmount + @AddTotalCreditLimit) Where LoanAccountId=@LoanAccountId				

		SET @Result='Success'
	End --If @PenaltyBounceExist>0	

	Select @Result as Result
End
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

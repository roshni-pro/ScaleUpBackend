using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _08feb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"Create Proc TransactionDetailByInvoiceId
	--declare
	@InvoiceId bigint=274,
	@Head varchar(50)='Interest'

	As 
	Begin

	select a.ReferenceId, h.Code Head, d.Amount,d.TransactionDate
	from AccountTransactions a with(nolock) 
	inner join AccountTransactionDetails d with(nolock) on a.Id=d.AccountTransactionId
	inner join TransactionDetailHeads h with(nolock) on d.TransactionDetailHeadId=h.Id
	where a.InvoiceId=@InvoiceId and h.Code=@Head
	and d.IsActive=1 and d.IsDeleted=0 

	End

go

CREATE OR ALTER Procedure [dbo].[InvoiceDetail]
--declare
	--@ReferenceId varchar(100)='2024354'
	@InvoiceId bigint=274
AS
Begin
	if(@InvoiceId != 0)
	begin

		select    x.AccountTransactionId
				, x.TransactionType 
				, x.SequenceNo
				, MAX(TransactionDate) TransactionDate
				, (ROUND(Sum(ISNULL(x.TxnAmount,0)),2))  TxnAmount
				, x.ReferenceId
		from
		(
			select		 ats.Id AccountTransactionId
						,case when h.Code = 'Refund' then 'Order' ELSE h.Code END TransactionType
						,case when cast(Max(ad.TransactionDate) as date) IS NULL then MAX(ats.Created) ELSE cast(Max(ad.TransactionDate) as date) END TransactionDate,
						-- ROUND(ROUND(Sum(ISNULL(ad.Amount,0)),2,2),2) TxnAmount
						Sum(ISNULL(ad.Amount,0)) TxnAmount
						,ats.ReferenceId
						,case when h.Code = 'Refund' then  2 else h.SequenceNo	END SequenceNo	
						--,Sum(ad.Amount) over() totalAmount
						from AccountTransactions ats  with(nolock) 
						inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
						inner join TransactionStatuses s with(nolock) on s.id=ats.TransactionStatusId
						inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
						inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
						where (ats.InvoiceId=@InvoiceId or ats.ParentAccountTransactionsID=@InvoiceId)
						and (ad.TransactionDate is null or Cast(ad.TransactionDate as Date)<= Cast(GETDATE() as Date))
						and ad.IsActive=1 
						group by ats.Id,s.Code,h.Code,SequenceNo, ats.Id, ats.ReferenceId
		)X
		group by x.AccountTransactionId, x.TransactionType, SequenceNo	, x.ReferenceId
		order by ReferenceId, SequenceNo

	end
	else
	begin
	select 'ReferenceId is null'
	end
End

Go
	Create Proc GetInvoiceDetailList
	--Declare
	@Status varchar(50)='All',
	@SearchKeyward varchar(50)='',
	@skip int=0,
	@take int=10,
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
			,i.InvoiceDate
			
	into #tempTransaction 
	from AccountTransactions ats  with(nolock) 
	inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
	inner join TransactionStatuses s with(nolock) on s.id=ats.TransactionStatusId
	inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
	inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
	inner join LoanAccounts l with(nolock) on ats.LoanAccountId=l.Id 
	inner join Invoices i with(nolock) on i.Id=ats.InvoiceId
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
	,i.OrderNo,cast(ats.DisbursementDate as date) ,l.ThirdPartyLoanCode,i.InvoiceDate

	order by ats.Id desc
	OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY

	select 
	Code Status
	,sum(PricipleAmount) PricipleAmount
	,sum(PaymentAmount) PaymentAmount
	,sum(ActualOrderAmount) ActualOrderAmount
	,sum(PaidAmount) PaidAmount
	,sum(OutstandingAmount) OutstandingAmount
	,sum(PayableAmount) PayableAmount
	--,sum(InterestAmount) InterestAmount
	--,sum(FullInterestAmount) FullInterestAmount
	--,sum(PenaltyAmount)PenaltyAmount
	--,sum(OverdueInterestAmount) OverdueInterestAmount
	--,sum(PartialPayment) PartialPayment
	,sum(ReceivedPayment) ReceivedPayment
	--,sum(ExtraPaymentAmount) ExtraPaymentAmount
	--,sum(InterestPaymentAmount) InterestPaymentAmount
	--,sum(OverduePaymentAmount) OverduePaymentAmount
	--,sum(BouncePaymentAmount)BouncePaymentAmount,
	--sum(PenalPaymentAmount) PenalPaymentAmount
	,DueDate,AccountCode,LeadCode,MobileNo,CustomerName,UtilizationAnchor,AnchorCompanyId,CityName,LoanAccountId,OrderId, ThirdPartyLoanCode,InvoiceNo
	,DisbursementDate,SettlementDate
	,InvoiceDate
	,TotalCount
	,InvoiceId
	from (

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
			,InvoiceDate
			,InvoiceId
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
	) invoice

	group by AccountCode,Code,LeadCode,MobileNo,CustomerName,UtilizationAnchor,AnchorCompanyId,CityName,LoanAccountId,OrderId, ThirdPartyLoanCode,InvoiceNo,
			DisbursementDate,DueDate,TotalCount,SettlementDate,InvoiceDate,InvoiceId

	end
";
            migrationBuilder.Sql(sp1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

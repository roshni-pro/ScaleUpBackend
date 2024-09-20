using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _14Mar_StoreProc2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"

GO

ALTER Proc [dbo].[TransactionDetailByInvoiceId]
		--declare
@InvoiceId bigint=47,
@Head varchar(50)='Order'
As 
Begin
	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 
	SELECT ID INTO #tempTransaction FROM AccountTransactions with(nolock)
	where InvoiceId = @InvoiceId and IsActive =1 and IsDeleted =0


	select s.Code, a.ReferenceId, h.Code Head, Round(d.Amount,3) Amount
	--,case when d.TransactionDate IS NOT null then d.TransactionDate else d.Created end TransactionDate
	,case when @Head='Order' OR @Head='Refund'  then a.DisbursementDate
	when d.TransactionDate IS NOT null then d.TransactionDate else d.Created end TransactionDate
	from AccountTransactions a with(nolock) 
	inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
	inner join AccountTransactionDetails d with(nolock) on a.Id=d.AccountTransactionId
	inner join TransactionDetailHeads h with(nolock) on d.TransactionDetailHeadId=h.Id
	where h.Code=@Head and s.Code not in ('Failed', 'Canceled', 'Initiate')
	and (a.Id in  (select Id from #tempTransaction) OR a.ParentAccountTransactionsID in  (select Id from #tempTransaction) )
	and d.IsActive=1 and d.IsDeleted=0 and Amount!=0
	and (d.TransactionDate IS NULL OR cast(d.TransactionDate as date) <= cast(getdate() as date))
	order by d.TransactionDate desc
End


GO

ALTER Procedure [dbo].[GetCustomerTransactionList]

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

	SELECT   max(t.Id )as TransactionId
			,max(ts.Code) as Status	
	        ,SUM(ISNULL(x.TotalAmount,0)) + SUM(y.Amount) as PaidAmount
			,max(CASE WHEN t.DisbursementDate IS NOT NULL THEN t.DueDate ELSE NULL END) DueDate
			,max(t.InvoiceNo) as OrderId 
			,max(la.AnchorName) as AnchorName  
			,SUM(ISNULL(x.PricipleAmount,0)) PricipleAmount 
			,t.InvoiceId
	into #tempTransaction 
	FROM AccountTransactions t
	inner join TransactionStatuses ts on t.TransactionStatusId = ts.Id  and ts.IsActive=1 and ts.IsDeleted=0
	inner join TransactionTypes tt  with(nolock) on t.TransactionTypeId = tt.Id  and tt.IsActive=1 and tt.IsDeleted=0
	inner join LoanAccounts la with(nolock) on t.LoanAccountId = la.Id and la.IsActive=1 and la.IsDeleted=0
	CROSS APPLY (
		SELECT   ROUND(ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN atd.Amount ELSE 0 END), 0),2) PricipleAmount
				,sum(Isnull(atd.Amount,0)) as TotalAmount
		FROM AccountTransactionDetails atd with(nolock)  
		inner join TransactionDetailHeads h with(nolock) on h.Id=atd.TransactionDetailHeadId and h.IsActive=1 and h.IsDeleted=0
		where t.Id = atd.AccountTransactionId  and atd.IsActive=1 and atd.IsDeleted=0
		and t.IsActive=1 and t.IsDeleted=0 and (atd.TransactionDate is null or cast(atd.TransactionDate as date) <= cast(getdate() as date))
		
	)x
	OUTER APPLY (
		SELECT SUM(xatd.Amount) Amount
		FROM AccountTransactions xt
		inner join TransactionStatuses xts on xt.TransactionStatusId = xts.Id  and xts.IsActive=1 and xts.IsDeleted=0
		inner join AccountTransactionDetails xatd with(nolock) on xt.Id = xatd.AccountTransactionId  and xatd.IsActive=1 and xatd.IsDeleted=0  
		
		WHERE (xts.Code != 'Failed'  AND xts.Code != 'Initiate'  AND ts.Code != 'Canceled')
		and xt.ParentAccountTransactionsID = t.Id
		
	)y
	WHERE (@AnchorCompanyID = 0 OR t.AnchorCompanyId = @AnchorCompanyID)
		and  tt.Code = 'OrderPlacement'
		and (ts.Code != 'Failed'  AND ts.Code != 'Initiate'  AND ts.Code != 'Canceled')
		and (@TransactionType = 'All' OR (@TransactionType = 'Paid' AND  ts.Code= 'Paid') OR (@TransactionType = 'Unpaid' AND  ts.Code != 'Paid'))
		--and cast(t.DueDate as date) <=  cast(getdate() as date)
		and t.IsActive =1 and t.IsDeleted =0
		and la.LeadId = @LeadId
		group by  t.InvoiceId
		order by max(t.Created) desc
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY


	select   tt.AnchorName
		    ,tt.DueDate
			,tt.OrderId
			,tt.Status
			,tt.TransactionId
			,ISNULL(tt.PaidAmount, 0) TotalRemainingAmount
			,tt.PricipleAmount as Amount
			,tt.InvoiceId
	from #tempTransaction tt

		
			--,ats.OrderId
			--,ats.Status
End


GO
ALTER Procedure [dbo].[GetCustomerTransactionList_Two]

--declare
	@LeadId bigint=248, --= 15
	@Skip int = 5,
	@Take int = 5
	
As 
Begin
	


	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 

	SELECT   max(t.Id) as TransactionId
			,max(ts.Code) as Status	
	        ,sum(Isnull(atd.Amount,0)) as TotalAmount
			,max(CASE WHEN t.DisbursementDate IS NOT NULL THEN t.DueDate ELSE NULL END) DueDate
			,max(t.InvoiceNo) as OrderId 
			,max(la.AnchorName) as AnchorName  
			,ROUND(ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN atd.Amount ELSE 0 END), 0),2) PricipleAmount  
			,t.InvoiceId
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
		group by  t.InvoiceId
		order by max(t.Created) desc
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY


	select   tt.AnchorName
		    ,tt.DueDate
			,tt.OrderId
			,tt.Status
			,tt.TransactionId
			,ISNULL(tt.TotalAmount, 0) + IsNULL(x.Amount, 0) TotalRemainingAmount
			,tt.PricipleAmount as Amount
			,tt.InvoiceId
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

ALTER Procedure [dbo].[GetCustomerTransactionBreakup]

--declare
	@InvoiceId bigint =51--=179
As 
Begin

	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 


   select 
         t.Id
    Into #tempTransaction 
	from AccountTransactions t
	inner join TransactionStatuses ts on t.TransactionStatusId = ts.Id  and ts.IsActive=1 and ts.IsDeleted=0
	where t.InvoiceId = @InvoiceId  and (ts.Code != 'Failed'  AND ts.Code != 'Initiate'  AND ts.Code != 'Canceled')



	select   h.Code TransactionType
			,ROUND(ISNULL(SUM(ISNULL(ad.Amount, 0)), 0),2) Amount ,
			max(ats.Id) as Id
	from AccountTransactions ats  with(nolock) 
	inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id  and t.IsActive =1 and t.IsDeleted =0
	inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
	inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id  and h.IsActive =1 and h.IsDeleted =0
	where --( ats.Id = @TransactionId or ats.ParentAccountTransactionsId = @TransactionId)
	( ats.Id in( select * from #tempTransaction  ) or ats.ParentAccountTransactionsId  in( select * from #tempTransaction)) and
	 (ad.TransactionDate  IS NULL OR CAST(ad.TransactionDate as DATE) <= GETDATE())
	and ats.IsActive =1 and ats.IsDeleted =0 
	and ad.IsActive =1 and ad.IsDeleted =0
	group by  h.Code,h.SequenceNo
	order by h.SequenceNo
	 --MAX(ats.Created)
			--,ats.OrderId
			--,ats.Status


End


GO

	ALTER Procedure [dbo].[InvoiceDetail]
	--declare
		@InvoiceId bigint=37
	AS
	Begin
		if(@InvoiceId != 0)
		begin
					IF OBJECT_ID('tempdb..#tempTest') IS NOT NULL 
					DROP TABLE #tempTest
			IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
				DROP TABLE #tempTransaction 
			SELECT ID INTO #tempTransaction FROM AccountTransactions
			where InvoiceId = @InvoiceId and IsActive =1 and IsDeleted =0
			select    x.AccountTransactionId
					, x.TransactionType 
					, x.SequenceNo
					, MAX(TransactionDate) TransactionDate
					, (ROUND(Sum(ISNULL(x.TxnAmount,0)),2))  TxnAmount
					, x.ReferenceId
					, x.InvoiceId
	   into #tempTest
			from
			(
				select		 ats.Id AccountTransactionId
							,case when h.Code = 'Refund' then 'Order' ELSE h.Code END TransactionType
							,case when cast(Max(ad.TransactionDate) as date) IS NULL then MAX(ats.Created) ELSE cast(Max(ad.TransactionDate) as date) END TransactionDate,
							-- ROUND(ROUND(Sum(ISNULL(ad.Amount,0)),2,2),2) TxnAmount
							Sum(ISNULL(ad.Amount,0)) TxnAmount
							,ats.ReferenceId
							,case when h.Code = 'Refund' then  2 else h.SequenceNo	END SequenceNo	
							,@InvoiceId InvoiceId
							from AccountTransactions ats  with(nolock) 
							inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
							inner join TransactionStatuses s with(nolock) on s.id=ats.TransactionStatusId
							inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
							inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
							where (ats.Id in  (select Id from #tempTransaction) OR ats.ParentAccountTransactionsID in  (select Id from #tempTransaction) )
							and (ad.TransactionDate is null or Cast(ad.TransactionDate as Date)<= Cast(GETDATE() as Date))
							and ad.IsActive=1 and s.Code not in ('Failed','Canceled','Initiate')
							group by ats.Id,s.Code,h.Code,SequenceNo, ats.Id, ats.ReferenceId,ats.InvoiceId
			)X
			group by x.AccountTransactionId, x.TransactionType, SequenceNo, x.ReferenceId
			,x.InvoiceId
			--group by x.TransactionType, TxnAmount, ReferenceId
			order by ReferenceId, SequenceNo
			SELECT cast(0 as bigint) AccountTransactionId,cast('' as date) TransactionDate, InvoiceId, TransactionType, (ROUND(Sum(ISNULL(TxnAmount,0)),2))  TxnAmount
				,ReferenceId = STUFF(
							 (SELECT distinct  ',' + ReferenceId FROM #tempTest FOR XML PATH ('')), 1, 1, ''
						   ) 
			FROM #tempTest GROUP BY InvoiceId,TransactionType,SequenceNo
			Order BY SequenceNo

			
		end
		else
		begin
		select 'ReferenceId is null'
		end
	End


GO

ALTER Proc [dbo].[GetInvoiceDetailList]
--declare
@Status varchar(50)='All',
@SearchKeyward varchar(50)='',
@skip int=0,
@take int=1000,
@CityName varchar(50)='',
@AnchorId bigint=2,
@FromDate datetime='2024-02-01',
@ToDate datetime='2024-03-12',
@LoanAccountId bigint=3
As 
Begin
	SELECT	z.InvoiceId
		   ,z.OrderNo
		   ,Isnull(z.InvoiceDate,'') InvoiceDate
		   ,z.AccountCode
		   ,Isnull(z.ThirdPartyLoanCode,'') ThirdPartyLoanCode
		   ,z.CustomerName
		   ,z.MobileNo	
		   --,MAX(z.Status) Status
		   ,z.Status
		   ,Isnull(z.InvoiceNo,'') InvoiceNo
		   ,z.LoanAccountId
		   ,z.UtilizationAnchor 
		   ,Isnull(MAX(z.DisbursementDate),'') DisbursementDate
		   ,Isnull(MAX(CASE WHEN z.DisbursementDate IS NOT NULL then z.DueDate ELSE NULL END),'') DueDate
		   ,round(SUM(z.PrincipleAmount),2) ActualOrderAmount
		   --,round(SUM( z.TotalOrderAmount) + IsNULL(SUM(z.Amount), 0),2,2)  PayableAmount
		   ,round(Sum(case when z.Status='Canceled' then 0 else z.TotalOrderAmount+z.Amount end),2,2) PayableAmount
		   ,Isnull(Sum(z.PaymentAmount),0) PaymentAmount
		   ,dense_rank() over (order by z.InvoiceId) + dense_rank() over (order by z.InvoiceId  desc) -1 TotalCount
		   ,Isnull(SettlementDate,'') SettlementDate
		   ,Sum(case when abs(z.PaymentAmount)>0 and (z.PrincipleAmount-abs(z.PaymentAmount))>0 and z.Status!='Paid' then (z.PrincipleAmount-abs(z.PaymentAmount)) else 0 end) PartialAmount
	FROM(
	select   i.Id as InvoiceId
			,i.OrderNo
			,i.InvoiceDate
			,la.AccountCode
			,la.ThirdPartyLoanCode
			,la.CustomerName
			,la.MobileNo
			--,MAX(ts.Code) Status
			,i.Status
			,i.InvoiceNo
			,la.id LoanAccountId
			,ats.DisbursementDate
			,CASE WHEN @Status = 'Paid' THEN  MAX( x.TransactionDate) ELSE NULL END SettlementDate
			,ats.DueDate
			,y.PrincipleAmount
			,y.TotalOrderAmount 
			,AnchorName UtilizationAnchor
			,ats.Created
			,IsNULL(SUM(x.Amount), 0) Amount
			,sum(x.PaymentAmount) PaymentAmount
		FROM Invoices i with(nolock)
		INNER JOIN AccountTransactions ats with(nolock) on i.Id = ats.InvoiceId and ats.IsActive =1 and ats.IsDeleted =0
		INNER JOIN TransactionTypes tt with(nolock) on ats.TransactionTypeId = tt.Id 
		INNER JOIN TransactionStatuses ts with(nolock) on ats.TransactionStatusId = ts.Id  
		INNER JOIN LoanAccounts la on i.LoanAccountId = la.Id
		CROSS APPLY (
			SELECT   SUM(ISNULL(yatd.Amount,0)) as TotalOrderAmount
					,SUM (CASE WHEN yh.Code = 'Order' OR yh.Code = 'Refund' THEN ISNULL(yatd.Amount,0) ELSE 0 END) as PrincipleAmount
			FROM AccountTransactionDetails yatd with(nolock)
			INNER JOIN TransactionDetailHeads yh with(nolock) on yatd.TransactionDetailHeadId = yh.Id  
			WHERE  ats.Id = yatd.AccountTransactionId and yatd.IsActive =1 and yatd.IsDeleted =0 
				   and (yatd.TransactionDate IS NULL OR CAST(yatd.TransactionDate as date) <= cast(getdate() as date))
		)y
		OUTER APPLY (
			select xatd.Amount,
				   xatd.TransactionDate,
				   xh.Code Head,
				   xtt.Code TransactionType
				   ,CASE WHEN @Status = 'Paid'  AND xts.Code = 'Paid' and xtt.Code in ('OrderPayment') AND xatd.TransactionDate >= @FromDate AND CAST(xatd.TransactionDate as date) <= @ToDate THEN 1
				    WHEN @Status != 'Paid'  THEN 1
				    WHEN xtt.Code !='OrderPayment' THEN 1
				  ELSE 0 END IsTakePaymentTransaction
				  ,case when xh.Code='Payment' then xatd.Amount else null end PaymentAmount
				  --, CASE WHEN xats.Id IS NOT NULL AND  xts.Code != 'Paid' THEN 'YES' ELSE 'NO' END as PartiaPaidStatus 
			from AccountTransactions xats with(nolock)
			INNER JOIN TransactionTypes xtt with(nolock) on xats.TransactionTypeId = xtt.Id 
			INNER JOIN TransactionStatuses xts with(nolock) on xats.TransactionStatusId = xts.Id  
			INNER JOIN AccountTransactionDetails xatd with(nolock) on xats.Id = xatd.AccountTransactionId 
					and xatd.IsActive =1 and xatd.IsDeleted =0
					and (xatd.TransactionDate IS NULL OR CAST(xatd.TransactionDate as date) <= cast(getdate() as date))
			INNER JOIN TransactionDetailHeads xh with(nolock) on xatd.TransactionDetailHeadId = xh.Id  
			WHERE ats.Id = xats.ParentAccountTransactionsID  
					and xats.IsActive =1 
					and xats.IsDeleted =0
					and xatd.IsActive=1
					and xatd.IsDeleted=0
					and tt.Code not in ('Failed', 'Canceled', 'Initiate')
		)x
		WHERE (ISNULL(@LoanAccountId, 0) = 0 OR i.LoanAccountId = @LoanAccountId)
			  AND (ISNULL(@AnchorId, 0) = 0 OR la.AnchorCompanyId = @AnchorId)
			  AND (
						(@Status = 'All'AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
					 OR (@Status = 'Pending' AND i.Status = 'Pending' AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
					 OR (@Status = 'Due' AND i.Status  = 'Due' AND ats.DueDate >= @FromDate AND CAST(ats.DueDate as date) <= @ToDate)
					 OR (@Status = 'Overdue' AND i.Status = 'Overdue' AND ats.DueDate >= @FromDate AND CAST(ats.DueDate as date) <= @ToDate)
					 --OR (@Status = 'Paid'  AND ts.Code = 'Paid' and x.TransactionType = 'OrderPayment' AND x.TransactionDate >= @FromDate AND CAST(x.TransactionDate as date) <= @ToDate)
					 OR (@Status = 'Paid' AND i.Status = 'Paid' AND x.IsTakePaymentTransaction =1)
					 or (@Status =  'Partial' )
					 or (@Status = 'Initiate' and i.Status = 'Initiate' AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
					 or (@Status = 'Created' and i.Status = 'Created' AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
					 or (@Status = 'Disbursed' and i.Status = 'Disbursed' AND ats.DisbursementDate >= @FromDate AND CAST(ats.DisbursementDate as date) <= @ToDate)
			  )
			  AND tt.Code ='OrderPlacement' and ts.Code not in ('Failed', 'Canceled', 'Initiate')
			  and ((la.CustomerName like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (i.InvoiceNo like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (la.AccountCode like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (la.MobileNo like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')=''))
		  And (la.CityName=@CityName or Isnull(@CityName,'')='')
		group by	i.Id
				   ,i.OrderNo
				   ,i.InvoiceDate
				   ,i.Status
				   ,la.AccountCode
				   ,la.ThirdPartyLoanCode
				   ,la.CustomerName
				   ,la.MobileNo	
				   ,i.Status
				   ,i.InvoiceNo
				   ,la.id
				   ,AnchorName
				   ,ats.DisbursementDate
				   ,ats.DueDate
				   ,y.PrincipleAmount
			       ,y.TotalOrderAmount
				   ,ats.Created
				   ,ts.Code
		 HAVING ( @Status !=  'Partial'   or (@Status =  'Partial' AND ts.Code != 'Paid'   AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate AND sum(abs(x.PaymentAmount))>0 AND (sum(x.Amount) + y.TotalOrderAmount) > 0.01))
		)Z
		group by	z.InvoiceId
				   ,z.InvoiceDate
				   ,z.Status
				   ,z.AccountCode
				   ,z.ThirdPartyLoanCode
				   ,z.CustomerName
				   ,z.MobileNo	
				   --,z.Status
				   ,z.InvoiceNo
				   ,z.LoanAccountId
				   ,z.UtilizationAnchor
				   ,SettlementDate
				   ,z.OrderNo
		Order By MAX(z.Created) DESC
		OFFSET @skip ROWS 
		FETCH NEXT @take ROWS ONLY
END


GO

ALTER Procedure [dbo].[GetOutstandingTransactionsList]
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
	and ats.DisbursementDate IS NOT NULL
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

CREATE OR ALTER Procedure [dbo].[GetOutstandingTransactionsListForDisbursement]
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
--	and (ts.Code = 'Pending' OR ts.Code = 'Due' OR ts.Code = 'Overdue'  OR ts.Code = 'Delinquent')
	and (ts.Code = 'Pending')
	And (ats.ReferenceId=@TransactionNo or ISNULL(@TransactionNo,'')='')
	And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)
	and ats.DisbursementDate IS NULL
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
-- exec GetLoanAccountList 0,0,0,'2024-01-01','2024-02-02','','',0,0,0,10
	ALTER Proc [dbo].[GetLoanAccountList]
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
			sum(ISNULL(c.DisbursalAmount,0)) over() as TotalDisbursal,
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

";

            migrationBuilder.Sql(sp1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

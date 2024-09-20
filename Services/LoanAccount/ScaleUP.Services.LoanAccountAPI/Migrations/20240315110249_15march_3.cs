using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _15march_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"

CREATE OR ALTER Proc [dbo].[GetInvoiceDetailList]
--declare
@Status varchar(50)='All',
@SearchKeyward varchar(50)='',
@skip int=0,
@take int=1000,
@CityName varchar(50)='',
@AnchorId bigint=2,
@FromDate datetime='2024-02-01',
@ToDate datetime='2024-03-15',
@LoanAccountId bigint=5,
@IsExport bit =0
As 
Begin
	If(@IsExport=0)
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
		   ,round(Sum(case when z.Status='Canceled' then 0 else z.TotalOrderAmount end),2) 
		   +round(Sum(case when z.Status='Canceled' then 0 else z.Amount end),2) PayableAmount

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
		INNER JOIN TransactionTypes tt with(nolock) on ats.TransactionTypeId = tt.Id --and i.Id in (53,54)
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
	End
	Else
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
			   ,round(Sum(case when z.Status='Canceled' then 0 else z.TotalOrderAmount end),2) 
		       +round(Sum(case when z.Status='Canceled' then 0 else z.Amount end),2) PayableAmount
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
	End
END

GO

	CREATE OR ALTER Procedure [dbo].[InvoiceDetail]
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
					,Sum(ISNULL(x.TxnAmount,0)) TxnAmount
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
			SELECT cast(0 as bigint) AccountTransactionId,cast('' as date) TransactionDate, InvoiceId, TransactionType, Cast(Round(Cast(Sum(ISNULL(TxnAmount,0)) AS decimal(18,5)),2) as float)  TxnAmount
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

	CREATE OR ALTER proc [dbo].[GetLoanAccountDetail] 
	--Declare
	   @LoanAccountId bigint =5
	as
	begin
	
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
			   ,abs(round(TotalRepayment,2))TotalRepayment
			   ,abs(round(PrincipalRepaymentAmount,2,2))PrincipalRepaymentAmount
			   ,abs(round(InterestRepaymentAmount,2))InterestRepaymentAmount
			   ,abs(round(OverdueInterestPaymentAmount,2,2))OverdueInterestPaymentAmount
			   ,abs(round(PenalRePaymentAmount,2,2))PenalRePaymentAmount
			   ,abs(round(BounceRePaymentAmount,2,2))BounceRePaymentAmount
			   ,abs(round(ExtraPaymentAmount,2,2))ExtraPaymentAmount
			   ,abs(round(PenalAmount,2,2)) PenalAmount
			   --,tans.*
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
										,'Refund') 
						then d.Amount else 0 end) as TotalRepayment
				,Sum(case when h.Code in ('Payment') then d.Amount else 0 end) as PrincipalRepaymentAmount
				,Sum(case when h.Code in ('InterestPaymentAmount')  then d.Amount else 0 end) as InterestRepaymentAmount
				,Sum(case when h.Code in ('OverduePaymentAmount')  then d.Amount else 0 end) as OverdueInterestPaymentAmount
				,Sum(case when h.Code in ('PenalPaymentAmount')  then d.Amount else 0 end) as PenalRePaymentAmount
				,Sum(case when h.Code in ('BouncePaymentAmount')  then d.Amount else 0 end) as BounceRePaymentAmount
				,Sum(case when h.Code in ('ExtraPaymentAmount')  then d.Amount else 0 end) as ExtraPaymentAmount
				,Sum(case when h.Code in ('DelayPenalty')   then d.Amount else 0 end) as PenalAmount
				
				from AccountTransactions a
				inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId
				inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId
				inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
				inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId
				where d.IsActive=1 and d.IsDeleted=0 and a.LoanAccountId=l.Id and h.IsActive=1 and h.IsDeleted=0
		) y	
	  where  l.id=@LoanAccountId
	end

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

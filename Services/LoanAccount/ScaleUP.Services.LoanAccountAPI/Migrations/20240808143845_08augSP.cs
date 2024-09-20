using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _08augSP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER   Proc [dbo].[GetInvoiceDetailList]
--declare
@Status varchar(50)='All',
@SearchKeyward varchar(50)='',
@skip int=0,
@take int=1000,
@CityName varchar(50)='',
@AnchorId bigint,
@FromDate datetime='2024-01-31',
@ToDate datetime='2024-07-03',
@LoanAccountId bigint,
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
		   ,Z.PaymentDate
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
			,Max(p.PaymentDate) as PaymentDate
		FROM Invoices i with(nolock)
		INNER JOIN AccountTransactions ats with(nolock) on i.Id = ats.InvoiceId and ats.IsActive =1 and ats.IsDeleted =0
		INNER JOIN TransactionTypes tt with(nolock) on ats.TransactionTypeId = tt.Id --and i.Id in (53,54)
		INNER JOIN TransactionStatuses ts with(nolock) on ats.TransactionStatusId = ts.Id  
		INNER JOIN LoanAccounts la on i.LoanAccountId = la.Id
		CROSS APPLY (
			SELECT   SUM(ISNULL(yatd.Amount,0)) as TotalOrderAmount
					,SUM (CASE WHEN yh.Code = 'Order' OR yh.Code = 'Refund' THEN ISNULL(yatd.Amount,0) ELSE 0 END) as PrincipleAmount
					--,MIN(cast(yatd.TransactionDate as date)) as PaymentDate
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
				  --,CASE WHEN xtt.Code in ('OrderPayment') THEN xatd.TransactionDate else null end as PaymentDate
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
	outer apply 
		(
			select Max(ad.TransactionDate) PaymentDate
			from  AccountTransactions at1  with(nolock) 
			inner join TransactionTypes t  with(nolock) on at1.TransactionTypeId = t.Id
			inner join AccountTransactionDetails ad  with(nolock) on at1.Id = ad.AccountTransactionId 
			inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
				--and (h.Code = 'Payment' ) 
			where ats.Id = at1.ParentAccountTransactionsID and t.Code = 'OrderPayment'
			And at1.IsActive=1 And at1.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
		)P
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
			  --tt.Code ='OrderPlacement' and ts.Code not in ('Failed', 'Canceled', 'Initiate')
			  AND tt.Code ='OrderPlacement' and ts.Code not in ('Failed', 'Canceled') 
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
				   --,y.PaymentDate
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
				   ,Z.PaymentDate
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
			   ,isnull(round(SUM(z.PrincipleAmount),2),0) ActualOrderAmount
			   --,round(SUM( z.TotalOrderAmount) + IsNULL(SUM(z.Amount), 0),2,2)  PayableAmount
			   ,round(Sum(case when z.Status='Canceled' then 0 else isnull(z.TotalOrderAmount,0) end),2) 
		       +round(Sum(case when z.Status='Canceled' then 0 else isnull(z.Amount,0) end),2) PayableAmount
			   --,Isnull(Sum(z.PaymentAmount),0) PaymentAmount
			   ,dense_rank() over (order by z.InvoiceId) + dense_rank() over (order by z.InvoiceId  desc) -1 TotalCount
			   ,Isnull(SettlementDate,'') SettlementDate
			   ,Sum(case when abs(z.PaymentAmount)>0 and (z.PrincipleAmount-abs(z.PaymentAmount))>0 and z.Status!='Paid' then (z.PrincipleAmount-abs(z.PaymentAmount)) else 0 end) PartialAmount
			   ,Z.PaymentDate
			   ,isnull(Z.PAmount,0) as PaymentAmount
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
				,Max(p.PaymentDate) as PaymentDate
				,(P.PaymentAmount) as PAmount
			FROM Invoices i with(nolock)
			INNER JOIN AccountTransactions ats with(nolock) on i.Id = ats.InvoiceId and ats.IsActive =1 and ats.IsDeleted =0
			INNER JOIN TransactionTypes tt with(nolock) on ats.TransactionTypeId = tt.Id 
			INNER JOIN TransactionStatuses ts with(nolock) on ats.TransactionStatusId = ts.Id  
			INNER JOIN LoanAccounts la on i.LoanAccountId = la.Id
			CROSS APPLY (
				SELECT   SUM(ISNULL(yatd.Amount,0)) as TotalOrderAmount
						,SUM (CASE WHEN yh.Code = 'Order' OR yh.Code = 'Refund' THEN ISNULL(yatd.Amount,0) ELSE 0 END) as PrincipleAmount
						--,MIN(cast(yatd.TransactionDate as date)) as PaymentDate
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
					  --,CASE WHEN xtt.Code in ('OrderPayment') THEN xatd.TransactionDate else null end as PaymentDate
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
	outer apply 
		(
			select   --Max(CASE WHEN h.Code = 'Payment' OR h.Code = 'InterestPaymentAmount' OR h.Code = 'ExtraPaymentAmount' OR h.Code = 'BouncePaymentAmount' OR h.Code = 'PenalPaymentAmount' OR h.Code = 'OverduePaymentAmount' THEN ad.TransactionDate  ELSE null END ) PaymentDate
			Max(ad.TransactionDate) PaymentDate
			, Sum(ad.amount) PaymentAmount
			from  AccountTransactions at1  with(nolock) 
			inner join TransactionTypes t  with(nolock) on at1.TransactionTypeId = t.Id
			inner join AccountTransactionDetails ad  with(nolock) on at1.Id = ad.AccountTransactionId 
			inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
				--and (h.Code = 'Payment' ) 
			where ats.Id = at1.ParentAccountTransactionsID and t.Code = 'OrderPayment'
			And at1.IsActive=1 And at1.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
		)P
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
					   --,y.PaymentDate
					   ,P.PaymentAmount
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
					   ,Z.PaymentDate
					   ,Z.PAmount
			Order By MAX(z.Created) DESC
	End
END

GO


CREATE OR ALTER     Procedure [dbo].[InvoiceDetail]  
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
    select   ats.Id AccountTransactionId  
       ,case when h.Code = 'Refund' then 'Order' ELSE h.Code END TransactionType  
       ,case when cast(Max(ad.TransactionDate) as date) IS NULL then MAX(ats.Created) ELSE cast(Max(ad.TransactionDate) as date) END TransactionDate,  
       -- ROUND(ROUND(Sum(ISNULL(ad.Amount,0)),2,2),2) TxnAmount  
       Sum(ISNULL(ad.Amount,0)) TxnAmount  
       ,ats.ReferenceId  
       ,case when h.Code = 'Refund' then  2 else h.SequenceNo END SequenceNo   
       ,@InvoiceId InvoiceId  
       from AccountTransactions ats  with(nolock)   
       inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id  
       inner join TransactionStatuses s with(nolock) on s.id=ats.TransactionStatusId  
       inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId   
       inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id   
       where (ats.Id in  (select Id from #tempTransaction) OR ats.ParentAccountTransactionsID in  (select Id from #tempTransaction) )  
       and (ad.TransactionDate is null or Cast(ad.TransactionDate as Date)<= Cast(GETDATE() as Date))  
       and ad.IsActive=1 and s.Code not in ('Failed','Canceled') and t.Code not like '%scaleup%'  
	   --s.Code not in ('Failed','Canceled','Initiate')
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


	CREATE OR ALTER   Proc [dbo].[TransactionDetailByInvoiceId]
			--declare
	@InvoiceId bigint=47,
	@Head varchar(50)='Payment'
	As 
	Begin
		IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
			DROP TABLE #tempTransaction 
		SELECT ID INTO #tempTransaction FROM AccountTransactions with(nolock)
		where InvoiceId = @InvoiceId and IsActive =1 and IsDeleted =0
		select s.Code, 
		--d.PaymentReqNo as ReferenceId, 
		case when @Head='Payment' or @Head='InterestPaymentAmount' or @Head='PenalPaymentAmount' then d.PaymentReqNo else a.ReferenceId end ReferenceId
		,h.Code Head, Round(d.Amount,3) Amount
		--,case when d.TransactionDate IS NOT null then d.TransactionDate else d.Created end TransactionDate
		,case when @Head='Order' OR @Head='Refund'  then a.DisbursementDate
		when d.TransactionDate IS NOT null then d.TransactionDate else d.Created end TransactionDate
		from AccountTransactions a with(nolock) 
		inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
		inner join AccountTransactionDetails d with(nolock) on a.Id=d.AccountTransactionId
		inner join TransactionDetailHeads h with(nolock) on d.TransactionDetailHeadId=h.Id
		where h.Code=@Head and s.Code not in ('Failed', 'Canceled')
		--s.Code not in ('Failed', 'Canceled', 'Initiate')
		and (a.Id in  (select Id from #tempTransaction) OR a.ParentAccountTransactionsID in  (select Id from #tempTransaction) )
		and d.IsActive=1 and d.IsDeleted=0 and Amount!=0
		and (d.TransactionDate IS NULL OR cast(d.TransactionDate as date) <= cast(getdate() as date))
		order by d.TransactionDate desc
	End

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

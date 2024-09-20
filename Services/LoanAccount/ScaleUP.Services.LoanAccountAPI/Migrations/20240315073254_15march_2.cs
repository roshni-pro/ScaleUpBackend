using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _15march_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"


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
@LoanAccountId bigint=3,
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
	End
END

Go
    
	ALTER Proc [dbo].[GetInvoiceDetailListExport]
	--declare
	@Status varchar(50)='All',
	@SearchKeyward varchar(50)='',
	@skip int=0,
	@take int=1000,
	@CityName varchar(50)='',
	@AnchorId bigint=2,
	@FromDate datetime='2024-02-01',
	@ToDate datetime='2024-03-11',
	@LoanAccountId bigint=0
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
			
END

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

	SELECT   max(t.Id )as TransactionId
			,max(ts.Code) as Status	
	        ,SUM(ISNULL(x.TotalAmount,0)) + ISNULL(SUM(y.Amount),0) as PaidAmount
			,max(CASE WHEN t.DisbursementDate IS NOT NULL THEN t.DueDate ELSE NULL END) DueDate
			,i.OrderNo as OrderId 
			,max(la.AnchorName) as AnchorName  
			,SUM(ISNULL(x.PricipleAmount,0)) PricipleAmount 
			,t.InvoiceId
			,i.InvoiceNo 
	into #tempTransaction 
	FROM AccountTransactions t
	inner join Invoices i on t.InvoiceId = i.Id
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
		group by  t.InvoiceId, i.InvoiceNo, i.OrderNo
		order by max(t.Created) desc
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY


	select   tt.AnchorName
		    ,tt.DueDate
			,tt.OrderId
			,tt.Status
			,tt.TransactionId
			,ISNULL(tt.PaidAmount, 0) PaidAmount
			,tt.PricipleAmount as Amount
			,tt.InvoiceId
	from #tempTransaction tt

		
			--,ats.OrderId
			--,ats.Status
End


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
	SELECT   max(t.Id) as TransactionId
			,max(ts.Code) as Status	
	        ,sum(Isnull(atd.Amount,0)) as TotalAmount
			,max(CASE WHEN t.DisbursementDate IS NOT NULL THEN t.DueDate ELSE NULL END) DueDate
			,i.OrderNo as OrderId 
			,max(la.AnchorName) as AnchorName  
			,ROUND(ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN atd.Amount ELSE 0 END), 0),2) PricipleAmount  
			,t.InvoiceId
			,i.InvoiceNo 
	into #tempTransaction 
	FROM AccountTransactions t
	inner join Invoices i on t.InvoiceId = i.Id
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
		group by  t.InvoiceId, i.InvoiceNo, i.OrderNo
		order by max(t.Created) desc
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY
	select   tt.AnchorName
		    ,tt.DueDate
			,tt.OrderId
			,tt.Status
			,tt.TransactionId
			,ISNULL(tt.TotalAmount, 0) + IsNULL(x.Amount, 0) PaidAmount
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

CREATE OR ALTER   Procedure [dbo].[GetCustomerTransactionList]

--declare
	@LeadId bigint=30, --= 15
	@AnchorCompanyID bigint=2, 
	@Skip int = 0,
	@Take int = 5,
	@TransactionType nvarchar(100) = 'All' --- 'ALL/Paid/Unpaid'
As 
Begin
	

	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 

	SELECT   max(t.Id )as TransactionId
			,i.Status as Status	
	        ,CASE WHEN i.Status != 'Paid' THEN SUM(ISNULL(x.TotalAmount,0)) + ISNULL(SUM(y.Amount),0) ELSE ABS(ISNULL(SUM(y.Amount),0)) END as PaidAmount
			,max(CASE WHEN t.DisbursementDate IS NOT NULL THEN t.DueDate ELSE NULL END) DueDate
			,i.OrderNo as OrderId 
			,max(la.AnchorName) as AnchorName  
			,SUM(ISNULL(x.PricipleAmount,0)) PricipleAmount 
			,t.InvoiceId
			,i.InvoiceNo 
	into #tempTransaction 
	FROM AccountTransactions t
	inner join Invoices i on t.InvoiceId = i.Id
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
		inner join TransactionTypes xtt  with(nolock) on xt.TransactionTypeId = xtt.Id  and xtt.IsActive=1 and xtt.IsDeleted=0
		inner join TransactionStatuses xts on xt.TransactionStatusId = xts.Id  and xts.IsActive=1 and xts.IsDeleted=0
		inner join AccountTransactionDetails xatd with(nolock) on xt.Id = xatd.AccountTransactionId  and xatd.IsActive=1 and xatd.IsDeleted=0  
		
		WHERE (xts.Code != 'Failed'  AND xts.Code != 'Initiate'  AND ts.Code != 'Canceled')
		and(ts.Code != 'Paid' OR xtt.Code = 'OrderPayment')
		and xt.ParentAccountTransactionsID = t.Id
		
	)y
	WHERE (@AnchorCompanyID = 0 OR t.AnchorCompanyId = @AnchorCompanyID)
		and  tt.Code = 'OrderPlacement'
		and (ts.Code != 'Failed'  AND ts.Code != 'Initiate'  AND ts.Code != 'Canceled')
		and (@TransactionType = 'All' OR (@TransactionType = 'Paid' AND  ts.Code= 'Paid') OR (@TransactionType = 'Unpaid' AND  ts.Code != 'Paid'))
		--and cast(t.DueDate as date) <=  cast(getdate() as date)
		and t.IsActive =1 and t.IsDeleted =0
		and la.LeadId = @LeadId
		group by  t.InvoiceId, i.InvoiceNo, i.OrderNo,i.Status
		order by max(t.Created) desc
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY


	select   tt.AnchorName
		    ,tt.DueDate
			,tt.OrderId
			,tt.Status
			,tt.TransactionId
			,ROUND(ISNULL(tt.PaidAmount, 0),2) PaidAmount
			,ROUND(tt.PricipleAmount,2) as Amount
			,tt.InvoiceId
			,tt.InvoiceNo
	from #tempTransaction tt

		
			--,ats.OrderId
			--,ats.Status
End


GO

CREATE OR ALTER Procedure [dbo].[GetCustomerTransactionList_Two]
--declare
	@LeadId bigint=19, --= 15
	@Skip int = 0,
	@Take int = 50
As 
Begin
	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 
	SELECT   max(t.Id) as TransactionId
			,max(ts.Code) as Status	
	        ,sum(Isnull(atd.Amount,0)) as TotalAmount
			,max(CASE WHEN t.DisbursementDate IS NOT NULL THEN t.DueDate ELSE NULL END) DueDate
			,i.OrderNo as OrderId 
			,max(la.AnchorName) as AnchorName  
			,ROUND(ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN atd.Amount ELSE 0 END), 0),2) PricipleAmount  
			,t.InvoiceId
			,i.InvoiceNo 
	into #tempTransaction 
	FROM AccountTransactions t
	inner join Invoices i on t.InvoiceId = i.Id
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
		group by  t.InvoiceId, i.InvoiceNo, i.OrderNo
		order by max(t.Created) desc
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY
	select   tt.AnchorName
		    ,tt.DueDate
			,tt.OrderId
			,tt.Status
			,tt.TransactionId
	        ,ROUND(CASE WHEN tt.Status != 'Paid' THEN ISNULL(tt.TotalAmount, 0) + IsNULL(x.Amount, 0) ELSE ABS(IsNULL(x.Amount, 0)) END,2) as PaidAmount
			,ROUND(tt.PricipleAmount,2) as Amount
			,tt.InvoiceId
			,tt.InvoiceNo
	from #tempTransaction tt
	outer apply(
             select
			       SUM(ISNULL(atd.Amount, 0)) Amount
			 from AccountTransactions at with(nolock) 
			 inner join TransactionTypes xtt  with(nolock) on at.TransactionTypeId = xtt.Id  and xtt.IsActive=1 and xtt.IsDeleted=0
			 inner join AccountTransactionDetails atd with(nolock) on at.Id = atd.AccountTransactionId
			 where tt.TransactionId = at.ParentAccountTransactionsID  and (atd.TransactionDate  IS NULL OR CAST(atd.TransactionDate as DATE) <= GETDATE())
	         and at.IsActive =1 and at.IsDeleted =0 and atd.IsActive =1 and atd.IsDeleted =0
			 and(tt.Status != 'Paid' OR xtt.Code = 'OrderPayment')
			 --group by at.ParentAccountTransactionsID 
	)x	
End

GO


CREATE OR ALTER Procedure [dbo].[GetPaymentOutstanding]
--declare
	@LoanAccountId bigint=35, --= 15
	@AnchorCompanyID bigint =0
as
Begin
	--IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
	--	DROP TABLE #tempTransaction 
	--SELECT t.Id ,
	--       sum(Isnull(atd.Amount,0)) as TotalAmount
	--into #tempTransaction 
	--FROM AccountTransactions t
	--inner join TransactionStatuses ts on t.TransactionStatusId = ts.Id  and ts.IsActive=1 and ts.IsDeleted=0
	--inner join TransactionTypes tt  with(nolock) on t.TransactionTypeId = tt.Id  and tt.IsActive=1 and tt.IsDeleted=0
	--inner join AccountTransactionDetails atd with(nolock) on t.Id = atd.AccountTransactionId  and atd.IsActive=1 and atd.IsDeleted=0
	--inner join TransactionDetailHeads h with(nolock) on h.Id=atd.TransactionDetailHeadId and h.IsActive=1 and h.IsDeleted=0
	--WHERE (@AnchorCompanyID = 0 OR t.AnchorCompanyId = @AnchorCompanyID)
	--	and t.LoanAccountId = @LoanAccountId 
	--	and  tt.Code = 'OrderPlacement'
	--	and (ts.Code != 'Paid' AND ts.Code != 'Failed'  AND ts.Code != 'Initiate'  AND ts.Code != 'Canceled')
	--	and t.DisbursementDate is not null 
	--	and t.IsActive=1 and t.IsDeleted=0 and (atd.TransactionDate is null or cast(atd.TransactionDate as date) <= cast(getdate() as date))
	--	and cast(t.DueDate as date) <=  cast(getdate() as date)
	--	and t.IsActive =1 and t.IsDeleted =0
	--	group by  t.Id
		--order by t.InvoiceDate desc
	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 

	SELECT   ROUND(SUM(ISNULL(x.TotalAmount,0)) + ISNULL(SUM(y.Amount),0),2) as PaidAmount
			,t.InvoiceId
	into #tempTransaction 
	FROM AccountTransactions t
	inner join Invoices i on t.InvoiceId = i.Id
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
		inner join TransactionTypes xtt  with(nolock) on xt.TransactionTypeId = xtt.Id  and xtt.IsActive=1 and xtt.IsDeleted=0
		inner join TransactionStatuses xts on xt.TransactionStatusId = xts.Id  and xts.IsActive=1 and xts.IsDeleted=0
		inner join AccountTransactionDetails xatd with(nolock) on xt.Id = xatd.AccountTransactionId  and xatd.IsActive=1 and xatd.IsDeleted=0  
		
		WHERE (xts.Code != 'Failed'  AND xts.Code != 'Initiate'  AND ts.Code != 'Canceled')
		and xt.ParentAccountTransactionsID = t.Id
		
	)y
	WHERE (@AnchorCompanyID = 0 OR t.AnchorCompanyId = @AnchorCompanyID)
		and  tt.Code = 'OrderPlacement'
		and (ts.Code != 'Paid'  AND ts.Code != 'Failed'  AND ts.Code != 'Initiate'  AND ts.Code != 'Canceled')
		and t.IsActive =1 and t.IsDeleted =0
		and la.Id = @LoanAccountId
		and cast(t.DueDate as date) <=  cast(getdate() as date)
		and t.DisbursementDate is not null
		group by  t.InvoiceId, i.InvoiceNo, i.OrderNo,i.Status
		
	
	
	select  
			ROUND(SUM(tt.PaidAmount) ,2)  as TotalPayableAmount ,
			Count(Distinct tt.InvoiceId) TotalPendingInvoiceCount
	from  #tempTransaction tt 

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

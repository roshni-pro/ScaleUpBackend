using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _12march : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"

	Create Proc [dbo].[GetInvoiceDetailListExport]
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
		   ,Isnull(z.InvoiceDate,'') InvoiceDate
		   ,z.AccountCode
		   ,Isnull(z.ThirdPartyLoanCode,'') ThirdPartyLoanCode
		   ,z.CustomerName
		   ,z.MobileNo	
		   ,z.Status
		   ,Isnull(z.InvoiceNo,'') InvoiceNo
		   ,z.LoanAccountId
		   ,z.UtilizationAnchor 
		   ,Isnull(MAX(z.DisbursementDate),'') DisbursementDate
		   ,Isnull(MAX(CASE WHEN z.DisbursementDate IS NOT NULL then z.DueDate ELSE NULL END),'') DueDate
		   ,round(SUM(z.PrincipleAmount),2) ActualOrderAmount
		   ,round(SUM( z.TotalOrderAmount) + IsNULL(SUM(z.Amount), 0),2,2)  PayableAmount
		   ,Isnull(Sum(z.PaymentAmount),0) PaymentAmount
		   ,dense_rank() over (order by z.InvoiceId) + dense_rank() over (order by z.InvoiceId  desc) -1 TotalCount
		   ,Isnull(SettlementDate,'') SettlementDate
		   ,Sum(case when abs(z.PaymentAmount)>0 and (z.PrincipleAmount-abs(z.PaymentAmount))>0 and z.Status!='Paid' then (z.PrincipleAmount-abs(z.PaymentAmount)) else 0 end) PartialAmount
	FROM(
	select   i.Id as InvoiceId
			,i.InvoiceDate
			,la.AccountCode
			,la.ThirdPartyLoanCode
			,la.CustomerName
			,la.MobileNo
			,MAX(ts.Code) Status
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
		)x
		WHERE (ISNULL(@LoanAccountId, 0) = 0 OR i.LoanAccountId = @LoanAccountId)
			  AND (ISNULL(@AnchorId, 0) = 0 OR la.AnchorCompanyId = @AnchorId)
			  AND (
						(@Status = 'All'AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
					 OR (@Status = 'Pending' AND ts.Code = 'Pending' AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
					 OR (@Status = 'Due' AND ts.Code = 'Due' AND ats.DueDate >= @FromDate AND CAST(ats.DueDate as date) <= @ToDate)
					 OR (@Status = 'Overdue' AND ts.Code = 'Overdue' AND ats.DueDate >= @FromDate AND CAST(ats.DueDate as date) <= @ToDate)
					 --OR (@Status = 'Paid'  AND ts.Code = 'Paid' and x.TransactionType = 'OrderPayment' AND x.TransactionDate >= @FromDate AND CAST(x.TransactionDate as date) <= @ToDate)
					 OR (@Status = 'Paid' AND ts.Code = 'Paid' AND x.IsTakePaymentTransaction =1)
					 or (@Status =  'Partial' )
					 or (@Status = 'Initiate' and ts.Code='Initiate' AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
					 or (@Status = 'Intransit' and ts.Code='Intransit' AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
			  )
			  AND tt.Code ='OrderPlacement'
			  and ((la.CustomerName like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (i.InvoiceNo like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (la.AccountCode like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (la.MobileNo like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')=''))
		  And (la.CityName=@CityName or Isnull(@CityName,'')='')

		group by	i.Id
				   ,i.InvoiceDate
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
				   ,z.AccountCode
				   ,z.ThirdPartyLoanCode
				   ,z.CustomerName
				   ,z.MobileNo	
				   ,z.Status
				   ,z.InvoiceNo
				   ,z.LoanAccountId
				   ,z.UtilizationAnchor
				   ,SettlementDate


		Order By MAX(z.Created) DESC
		OFFSET @skip ROWS 
		FETCH NEXT @take ROWS ONLY
END

Go
	
	Alter Proc [dbo].[GetInvoiceDetailList]
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
		   ,Isnull(z.InvoiceDate,'') InvoiceDate
		   ,z.AccountCode
		   ,Isnull(z.ThirdPartyLoanCode,'') ThirdPartyLoanCode
		   ,z.CustomerName
		   ,z.MobileNo	
		   ,z.Status
		   ,Isnull(z.InvoiceNo,'') InvoiceNo
		   ,z.LoanAccountId
		   ,z.UtilizationAnchor 
		   ,Isnull(MAX(z.DisbursementDate),'') DisbursementDate
		   ,Isnull(MAX(CASE WHEN z.DisbursementDate IS NOT NULL then z.DueDate ELSE NULL END),'') DueDate
		   ,round(SUM(z.PrincipleAmount),2) ActualOrderAmount
		   ,round(SUM( z.TotalOrderAmount) + IsNULL(SUM(z.Amount), 0),2,2)  PayableAmount
		   ,Isnull(Sum(z.PaymentAmount),0) PaymentAmount
		   ,dense_rank() over (order by z.InvoiceId) + dense_rank() over (order by z.InvoiceId  desc) -1 TotalCount
		   ,Isnull(SettlementDate,'') SettlementDate
		   ,Sum(case when abs(z.PaymentAmount)>0 and (z.PrincipleAmount-abs(z.PaymentAmount))>0 and z.Status!='Paid' then (z.PrincipleAmount-abs(z.PaymentAmount)) else 0 end) PartialAmount
	FROM(
	select   i.Id as InvoiceId
			,i.InvoiceDate
			,la.AccountCode
			,la.ThirdPartyLoanCode
			,la.CustomerName
			,la.MobileNo
			,MAX(ts.Code) Status
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
		)x
		WHERE (ISNULL(@LoanAccountId, 0) = 0 OR i.LoanAccountId = @LoanAccountId)
			  AND (ISNULL(@AnchorId, 0) = 0 OR la.AnchorCompanyId = @AnchorId)
			  AND (
						(@Status = 'All'AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
					 OR (@Status = 'Pending' AND ts.Code = 'Pending' AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
					 OR (@Status = 'Due' AND ts.Code = 'Due' AND ats.DueDate >= @FromDate AND CAST(ats.DueDate as date) <= @ToDate)
					 OR (@Status = 'Overdue' AND ts.Code = 'Overdue' AND ats.DueDate >= @FromDate AND CAST(ats.DueDate as date) <= @ToDate)
					 --OR (@Status = 'Paid'  AND ts.Code = 'Paid' and x.TransactionType = 'OrderPayment' AND x.TransactionDate >= @FromDate AND CAST(x.TransactionDate as date) <= @ToDate)
					 OR (@Status = 'Paid' AND ts.Code = 'Paid' AND x.IsTakePaymentTransaction =1)
					 or (@Status =  'Partial' )
					 or (@Status = 'Initiate' and ts.Code='Initiate' AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
					 or (@Status = 'Intransit' and ts.Code='Intransit' AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
			  )
			  AND tt.Code ='OrderPlacement'
			  and ((la.CustomerName like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (i.InvoiceNo like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (la.AccountCode like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')='')
		   or (la.MobileNo like '%'+@SearchKeyward+'%' or ISNULL(@SearchKeyward,'')=''))
		  And (la.CityName=@CityName or Isnull(@CityName,'')='')

		group by	i.Id
				   ,i.InvoiceDate
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
				   ,z.AccountCode
				   ,z.ThirdPartyLoanCode
				   ,z.CustomerName
				   ,z.MobileNo	
				   ,z.Status
				   ,z.InvoiceNo
				   ,z.LoanAccountId
				   ,z.UtilizationAnchor
				   ,SettlementDate


		Order By MAX(z.Created) DESC
		OFFSET @skip ROWS 
		FETCH NEXT @take ROWS ONLY
END

Go

alter Proc [dbo].[TransactionDetailByInvoiceId]
		--declare
		@InvoiceId bigint=0,
		@Head varchar(50)=''
		As 
		Begin

		IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
			DROP TABLE #tempTransaction 
		
		SELECT ID INTO #tempTransaction FROM AccountTransactions with(nolock)
		where InvoiceId = @InvoiceId and IsActive =1 and IsDeleted =0

		select s.Code, a.ReferenceId, h.Code Head, round(d.Amount,2) Amount
		,case when d.TransactionDate!=null then d.TransactionDate else d.Created end TransactionDate
		from AccountTransactions a with(nolock) 
		inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
		inner join AccountTransactionDetails d with(nolock) on a.Id=d.AccountTransactionId
		inner join TransactionDetailHeads h with(nolock) on d.TransactionDetailHeadId=h.Id
		where h.Code=@Head and s.Code!='Failed'
		and (a.Id in  (select Id from #tempTransaction) OR a.ParentAccountTransactionsID in  (select Id from #tempTransaction) )
		and d.IsActive=1 and d.IsDeleted=0 and Amount!=0
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

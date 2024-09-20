using IdentityServer4.Validation;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _09feb : Migration
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
	@FromDate datetime='2024-03-01',
	@ToDate datetime='2024-03-09',
	@LoanAccountId bigint=0
	As 
	Begin

	select   i.Id as InvoiceId
			,i.InvoiceDate
			,la.AccountCode
			,la.ThirdPartyLoanCode
			,la.CustomerName
			,la.MobileNo
			,MAX(ts.Code) Status
			,i.InvoiceNo
			,la.id LoanAccountId
			,MAX(ats.DisbursementDate) DisbursementDate
			,CASE WHEN @Status = 'Paid' THEN  MAX( x.TransactionDate) ELSE NULL END SettlementDate
			,MAX(CASE WHEN ats.DisbursementDate IS NOT NULL then ats.DueDate ELSE NULL END) DueDate
			,round(SUM(distinct y.PrincipleAmount),2) ActualOrderAmount
			,round(SUM(distinct y.TotalOrderAmount) + IsNULL(SUM(x.Amount), 0),2)  PayableAmount
			,dense_rank() over (order by i.Id) + dense_rank() over (order by i.Id  desc) -1 TotalCount
			,AnchorName UtilizationAnchor	
		FROM Invoices i with(nolock)
		INNER JOIN AccountTransactions ats with(nolock) on i.Id = ats.InvoiceId and ats.IsActive =1 and ats.IsDeleted =0
		INNER JOIN TransactionTypes tt with(nolock) on ats.TransactionTypeId = tt.Id 
		INNER JOIN TransactionStatuses ts with(nolock) on ats.TransactionStatusId = ts.Id  
		--INNER JOIN AccountTransactionDetails atd on ats.Id = atd.AccountTransactionId and atd.IsActive =1 and atd.IsDeleted =0
		--INNER JOIN TransactionDetailHeads h on atd.TransactionDetailHeadId = h.Id  
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
						@Status = 'All'
					 OR (@Status = 'Pending' AND ts.Code = 'Pending' AND ats.Created >= @FromDate AND CAST(ats.Created as date) <= @ToDate)
					 OR (@Status = 'Due' AND ts.Code = 'Due' AND ats.DueDate >= @FromDate AND CAST(ats.DueDate as date) <= @ToDate)
					 OR (@Status = 'Overdue' AND ts.Code = 'Overdue' AND ats.DueDate >= @FromDate AND CAST(ats.DueDate as date) <= @ToDate)
					 OR (@Status = 'Paid'  AND ts.Code = 'Paid' and x.TransactionType = 'OrderPayment' AND x.TransactionDate >= @FromDate AND CAST(x.TransactionDate as date) <= @ToDate)
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
		Order By MAX(ats.Created) DESC
		OFFSET @skip ROWS 
		FETCH NEXT @take ROWS ONLY
END


Go


	ALTER Proc [dbo].[GetLoanAccountDetailByTxnId]
	--declare
	@TransactionReqNo varchar(50)=''
	As 
	Begin
		
		declare @LoanId bigint=0

	select top 1 @LoanId=LoanAccountId from PaymentRequest with(nolock) where TransactionReqNo=@TransactionReqNo

	select l.Id LoanAccountId, l.LeadId,  l.ProductId,p.AnchorCompanyId,l.MobileNo,ac.CreditLimitAmount,l.NBFCCompanyId
	,isnull((x.UtilizeAmount),0) UtilizateLimit,l.AnchorName,p.TransactionAmount InvoiceAmount,l.NBFCIdentificationCode
	,p.OrderNo,l.CustomerName, l.CustomerImage ImageUrl, cast(0 as bigint) creditday
	
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
	
Go


ALTER Procedure [dbo].[InvoiceDetail]
	--declare
		@InvoiceId bigint=121
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
							and ad.IsActive=1 and s.Code!='Failed'
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
Go

ALTER Proc [dbo].[TransactionDetailByInvoiceId]
		--declare
		@InvoiceId bigint=117,
		@Head varchar(50)='DelayPenalty'
		As 
		Begin

		IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
			DROP TABLE #tempTransaction 
		
		SELECT ID INTO #tempTransaction FROM AccountTransactions with(nolock)
		where InvoiceId = @InvoiceId and IsActive =1 and IsDeleted =0

		select s.Code, a.ReferenceId, h.Code Head, round(d.Amount,2) Amount,d.TransactionDate
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

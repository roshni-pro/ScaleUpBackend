using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class SP_Invoice_6Aug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER   Procedure [dbo].[InvoiceDetail]
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
							and ad.IsActive=1 and s.Code not in ('Failed','Canceled','Initiate') and t.Code not like '%scaleup%'
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



CREATE OR ALTER       Procedure [dbo].[GetCompanyInvoiceDetails]
--declare
@InvoiceNo varchar(100)='AC/2024/9',
@RoleName varchar(100)='MakerUser'
AS
Begin	
		select
		cd.AccountTransactionId
		 ,ac.ReferenceId ReferenceNo
		, ci.Id CompanyInvoiceId
		,ac.CustomerUniqueCode as AnchorCode
		,ac.AnchorCompanyId
		,'' AnchorName
		,'' as NBFCName
		,ac.InvoiceNo
		,ac.InvoiceDate
		,ci.Status
		,Round(SUM(case when cd.InvoiceTransactionType = 1 then PayableAmount else 0 end),2) as ProcessingFeeTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 2 then PayableAmount else 0 end),2) as InterestTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 3 then PayableAmount else 0 end),2) as OverDueInterestTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 4 then PayableAmount else 0 end),2) as PenalTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 5 then PayableAmount else 0 end),2) as BounceTotal
		,Round(inv.InvoiceAmount,2) InvoiceAmount
		,Round(sum(cd.ScaleupShare),2) as ScaleupShare
		,la.NBFCCompanyId
		,Round(sum(cd.PayableAmount),2) as TotalAmount
		,cd.IsActive IsActive
		,'' StatusName
		,cast(0 as bit) IsCheckboxVisible
		,Round((sum(cd.ScaleupShare)/1.09)*0.18,2) GST
		--,ac.GSTAmount GST
		,Isnull(ci.InvoiceUrl,'') InvoiceUrl
		from CompanyInvoices ci  with(nolock)
		inner join CompanyInvoiceDetails cd with(nolock) on ci.Id = cd.CompanyInvoiceId and ci.IsActive=1 and ci.IsDeleted=0 
		inner join AccountTransactions ac with(nolock) on ac.Id = cd.AccountTransactionId
		inner join Invoices inv with(nolock) on inv.Id=ac.InvoiceId
		inner join LoanAccounts la with(nolock) on la.Id = ac.LoanAccountId and la.IsActive=1 and la.IsDeleted=0
		where ci.InvoiceNo = @InvoiceNo

		 --AND (
   --           (@RoleName = 'MakerUser' AND 1 = 1)
   --           OR
   --           (@RoleName = 'checkerUser' and ci.status=1 and ci.status!=3 AND cd.IsActive = 1 AND cd.IsDeleted = 0))
		and cd.IsActive = 1 AND cd.IsDeleted = 0
		
		group by  cd.AccountTransactionId,ac.CustomerUniqueCode
		,la.AnchorName
		,ac.InvoiceNo
		,ac.InvoiceDate
		,inv.InvoiceAmount
		,la.NBFCCompanyId
		,ci.Id
		,ci.status
		,ac.AnchorCompanyId
		,cd.AccountTransactionId
		 ,ac.ReferenceId
		 ,cd.IsActive
		 ,ci.InvoiceUrl
		 ,ac.GSTAmount

End;

GO



CREATE OR ALTER       Procedure [dbo].[GetCompanyInvoicesCharges]
	--declare
	@NBFCId bigint=3,
	@AnchorId dbo.Intvalues readonly,
	@FromDate datetime='2024-05-01',
	@ToDate datetime='2024-05-31',
	@InvoiceNo varchar(100)='AC/2024/9'
	As
	Begin
	--insert into @AnchorId values(2)
	select 
		Round(SUM(case when cd.InvoiceTransactionType = 1 then ScaleupShare else 0 end),2) as ProcessingFee,
		Round(SUM(case when cd.InvoiceTransactionType = 2 then ScaleupShare else 0 end),2) as InterestCharges,
		Round(SUM(case when cd.InvoiceTransactionType = 3 then ScaleupShare else 0 end),2) as OverDueInterest,
		Round(SUM(case when cd.InvoiceTransactionType = 4 then ScaleupShare else 0 end),2) as PenalCharges,
		Round(SUM(case when cd.InvoiceTransactionType = 5 then ScaleupShare else 0 end),2) as BounceCharges,
		Round(SUM(cd.ScaleupShare),2) as TotalTaxableAmount,
		Round(SUM(cd.ScaleupShare)*0.18,2) as TotalGstAmount,
		Round(SUM(cd.ScaleupShare)*1.18,2) as TotalInvoiceAmount
		from CompanyInvoices ci with(nolock)		
		join CompanyInvoiceDetails cd with(nolock) on ci.Id = cd.CompanyInvoiceId and ci.IsActive=1 and ci.IsDeleted=0 and cd.IsActive=1 and cd.IsDeleted=0
		where ci.CompanyId = @NBFCId 
		and ci.InvoiceNo=@InvoiceNo
		and CAST(ci.InvoiceDate as date)>=@FromDate and CAST(ci.InvoiceDate as date)<=@ToDate
		and exists(select 1 from AccountTransactions act with(nolock) 
					inner join @AnchorId a on act.AnchorCompanyId=a.IntValue
					where cd.AccountTransactionId=act.Id)
		group by cd.CompanyInvoiceId
	End
GO



CREATE OR ALTER proc [dbo].[InsertMonthCompanyInvoice_New]
as
begin
IF OBJECT_ID('tempdb..#tempPaidTrans') IS NOT NULL 
  DROP TABLE #tempPaidTrans
IF OBJECT_ID('tempdb..#tempCompanyInvoice') IS NOT NULL 
  DROP TABLE #tempCompanyInvoice
IF OBJECT_ID('tempdb..#tempAllTrans') IS NOT NULL 
  DROP TABLE #tempAllTrans

Declare @startDate DateTime= '2024-07-01'
,@endDate DateTime='2024-08-01'

if(not Exists(select 1 from CompanyInvoices a where month(a.InvoiceDate)=month(@startDate)))
begin
 Select a.AnchorCompanyId,l.NBFCCompanyId,l.ProductId ,a.InvoiceNo,b.TransactionDate,b.Amount,c.Code  TransType,a.ParentAccountTransactionsID 
 Into #tempPaidTrans
 from  AccountTransactions a  with(nolock)
 inner join TransactionTypes tt with(nolock) on a.TransactionTypeId=tt.Id
 inner join LoanAccounts l with(nolock) on a.LoanAccountId=l.Id
 Inner join AccountTransactionDetails b with(nolock) on a.id=b.AccountTransactionId and b.IsActive=1 and b.IsDeleted=0 
 inner join TransactionDetailHeads c with(nolock) on b.TransactionDetailHeadId=c.Id 
 and round(b.Amount,4)!=0 and l.IsActive=1 and l.IsDeleted =0 and a.IsActive=1 and a.IsDeleted =0 and b.IsActive=1 and b.IsDeleted =0
 where   b.TransactionDate<@endDate and c.Code like '%payment%' and  c.code!='Payment' and b.TransactionDate>=@startDate
 and tt.Code not like '%scaleup%' and a.InvoiceNo is not null
 --and not exists(select 1 from CompanyInvoiceDetails ci with(nolock) where ci.AccountTransactionId=a.ParentAccountTransactionsID and ci.IsActive=1 and ci.IsDeleted=0
 --and round(abs(ci.PayableAmount),2) = round(abs(b.Amount),2))
  
   select * from  #tempPaidTrans

 Select at.Id AccountTransactionId,at.InvoiceNo,c.Code,sum(atd.amount) OrderAmount
 Into #tempAllTrans
  From AccountTransactions at  with(nolock)
 Inner join AccountTransactionDetails atd with(nolock) on at.id=atd.AccountTransactionId and atd.IsActive=1 and atd.IsDeleted=0 
 inner join TransactionDetailHeads c with(nolock) on atd.TransactionDetailHeadId=c.Id 
 inner join #tempPaidTrans pd on at.InvoiceNo=pd.InvoiceNo
 where c.Code not like '%payment%' and c.Code not like '%scaleup%' and at.IsActive=1 and at.IsDeleted =0 and atd.IsActive=1 and atd.IsDeleted =0
  group by at.Id ,c.Code,at.InvoiceNo
  
   select * from  #tempAllTrans

  Select a.InvoiceNo,
       a.NBFCCompanyId,
       a.AccountTransactionId,a.TransType InvoiceTransactionType ,a.InvoiceAmt
	   ,a.PaymentDate
       ,a.principalAmt TotalAmount,round(a.PaymentAmt,4) PayableAmount
	   ,SharePercent
	   ,ScaleupShare
	   --,a.AnchorRate
	   --,case when a.IsSharable=1 then a.ScaleupRate else 0 end SharePercent	   
	   --,case when a.IsSharable=1 then 
	   -- case when a.TransType='BouncePaymentAmount' then a.ScaleupRate 
		  --  else  (round(a.PaymentAmt,4)/a.AnchorRate) * a.ScaleupRate  end
	   --else 0 end ScaleupShare
	   Into #tempCompanyInvoice
  from
  (
			 Select    
				 a.NBFCCompanyId,a.InvoiceNo,od.AccountTransactionId,a.TransType,a.TransactionDate PaymentDate
			  ,OtherCharges.amount    principalAmt
			  , od.OrderAmount  InvoiceAmt
			  ,abs(a.Amount) PaymentAmt 
			  ,ScaleUp.AccountTransactionId sc
			  ,ScaleUp.Code
			  ,ScaleUp.SharePercent
			  ,ScaleUp.ScaleupShare
			  from 
			  #tempPaidTrans a
			  outer apply
			  (
			      Select at.AccountTransactionId,at.OrderAmount From #tempAllTrans at
			       where at.InvoiceNo=a.InvoiceNo and at.Code= 'Order' 
			  ) od
			  outer apply
			  (
			      Select at.OrderAmount amount From #tempAllTrans at
			       where at.InvoiceNo=a.InvoiceNo and at.Code= (Case when  a.TransType='InterestPaymentAmount' then  'Interest' 
			        when a.TransType='PenalPaymentAmount' then  'DelayPenalty'
			        when a.TransType='BouncePaymentAmount' then  'BounceCharge' 
					when a.TransType='OverduePaymentAmount' then 'OverdueInterestAmount' 
			       else '' end)
			  ) OtherCharges
			  outer apply
			  (
			      Select at.Id AccountTransactionId,atd.Id,tdh.Code,sum(atd.Amount) ScaleupShare 
				  ,(case when tdh.Code = 'ScaleUpShareInterestAmount' then at.InterestRate 
				  when tdh.Code = 'ScaleUpSharePenalAmount' then at.DelayPenaltyRate 
				  when tdh.Code = 'ScaleUpShareBounceAmount' then at.BounceCharge
				  when tdh.Code = 'ScaleUpShareOverdueAmount' then at.InterestRate end) as SharePercent
				  From AccountTransactions at
				  join TransactionTypes tt on at.TransactionTypeId =tt.Id
				  join AccountTransactionDetails atd on atd.AccountTransactionId =at.Id
				  join TransactionDetailHeads tdh on tdh.Id =atd.TransactionDetailHeadId 
				  and at.IsActive=1 and at.IsDeleted=0 and atd.IsActive=1 and atd.IsDeleted=0
			       where at.ParentAccountTransactionsID=a.ParentAccountTransactionsID and tt.Code= 'ScaleupShareAmount'
				   and tdh.Code= (Case when  a.TransType='InterestPaymentAmount' then  'ScaleUpShareInterestAmount' 
			        when a.TransType='PenalPaymentAmount' then  'ScaleUpSharePenalAmount'
			        when a.TransType='BouncePaymentAmount' then  'ScaleUpShareBounceAmount' 
					when a.TransType='OverduePaymentAmount' then 'ScaleUpShareOverdueAmount' 
			       else '' end)
				   group by at.Id,atd.Id,tdh.Code,tdh.Code,at.InterestRate,at.DelayPenaltyRate,at.BounceCharge
			  ) ScaleUp
   )a --where a.InvoiceNo is not null 

   select * from  #tempCompanyInvoice

  DECLARE	@InvoiceData as table(InvoiceNo varchar(100))
	DECLARE @invoiceNo varchar(100)
	Insert into @InvoiceData
	exec [dbo].[spGetCurrentNumber] 'CompanyInvoiceNo'
	set @invoiceNo = (SELECT top 1 InvoiceNo from @InvoiceData)
   
   Insert into CompanyInvoices(CompanyId,InvoiceNo,InvoiceDate,InvoiceAmount,[Status],Created,CreatedBy,IsActive,IsDeleted)
   Select a.NBFCCompanyId,@invoiceNo,DATEADD(d, -1, DATEADD(m, DATEDIFF(m, 0, @startDate) + 1, 0)),sum(a.ScaleupShare),0,getdate(),'System',1,0 from #tempCompanyInvoice a group by NBFCCompanyId
  
   Insert into CompanyInvoiceDetails(CompanyInvoiceId,AccountTransactionId,InvoiceTransactionType,InvoiceAmt
       ,TotalAmount,PayableAmount,SharePercent,ScaleupShare,Created,CreatedBy,IsActive,IsDeleted,Status)
    Select SCOPE_IDENTITY(),a.AccountTransactionId, --a.PaymentDate,
	Case when a.InvoiceTransactionType='ProcessingFeePayment' then 1 
	     when a.InvoiceTransactionType='InterestPaymentAmount' then 2
		 when a.InvoiceTransactionType='OverduePaymentAmount' then 3
		 when a.InvoiceTransactionType='PenalPaymentAmount' then 4
		 when a.InvoiceTransactionType='BouncePaymentAmount' then 5 End
	,a.InvoiceAmt
	 ,a.TotalAmount,a.PayableAmount,a.SharePercent,a.ScaleupShare,getdate(),'System',1,0,'Pending'
	 from #tempCompanyInvoice a --group by NBFCCompanyId

end

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

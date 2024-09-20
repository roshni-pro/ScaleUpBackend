using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class LoanCompanyInvoice_SP_9May : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"CREATE OR ALTER  proc InsertMonthCompanyInvoice
as
begin

IF OBJECT_ID('tempdb..#tempPaidTrans') IS NOT NULL 
  DROP TABLE #tempPaidTrans

IF OBJECT_ID('tempdb..#tempFinalInvoice') IS NOT NULL 
  DROP TABLE #tempFinalInvoice

IF OBJECT_ID('tempdb..#tempCompanyInvoice') IS NOT NULL 
  DROP TABLE #tempCompanyInvoice


select l.ProductId,a.DisbursementDate,l.NBFCCompanyId,a.AnchorCompanyId,a.InvoiceNo,a.Id AccountTransactionId,a.ReferenceId,c.Code TransType
,sum(b.Amount) Amt
 Into #tempPaidTrans
 from AccountTransactions a  with(nolock)
 inner join LoanAccounts  l with(nolock) on a.LoanAccountId=l.Id 
 Inner join AccountTransactionDetails b with(nolock) on a.id=b.AccountTransactionId and b.IsActive=1 and b.IsDeleted=0 
 inner join TransactionDetailHeads c with(nolock) on b.TransactionDetailHeadId=c.Id 
  inner join TransactionStatuses s with(nolock) on s.id=a.TransactionStatusId
where a.TransactionStatusId=4 and a.IsActive=1 and a.IsDeleted=0
 and TransactionTypeId=2 and s.Code not in ('Failed','Canceled','Initiate')
 and not exists(select 1 from CompanyInvoiceDetails ci with(nolock) where ci.AccountTransactionId=a.id)
 Group by l.ProductId,a.DisbursementDate,l.NBFCCompanyId,a.AnchorCompanyId,a.InvoiceNo,a.Id,a.ReferenceId,c.Code

 
Select 
ProductId,Max(DisbursementDate) PaymentDate,NBFCCompanyId,AnchorCompanyId,InvoiceNo,AccountTransactionId,ReferenceId,TransType
,sum(Amt) Amt
 into #tempFinalInvoice from (
Select * from #tempPaidTrans
--where ReferenceId='202415'
--order by AccountTransactionId
Union all
select c.ProductId,b.TransactionDate,l.NBFCCompanyId,a.AnchorCompanyId,a.InvoiceNo,a.ParentAccountTransactionsID AccountTransactionId,a.ReferenceId,
Case when t.Code='OrderPayment' then d.Code else t.Code end TransType,sum(b.Amount) Amt 
from  AccountTransactions a with(nolock) 
 inner join LoanAccounts l with(nolock) on a.LoanAccountId=l.Id
 inner join TransactionTypes t with(nolock) on a.TransactionTypeId=t.Id
 Inner join #tempPaidTrans c on a.ParentAccountTransactionsID=c.AccountTransactionId and c.TransType='Order'
 Inner join AccountTransactionDetails b with(nolock) on a.id=b.AccountTransactionId  and b.IsActive=1 and b.IsDeleted=0 
 inner join TransactionDetailHeads d with(nolock) on b.TransactionDetailHeadId=d.Id 
 --where --a.ReferenceId='202415'
 -- exists(select 1 from #tempPaidTrans c where a.ParentAccountTransactionsID=c.AccountTransactionId)
 Group by c.ProductId,b.TransactionDate,l.NBFCCompanyId,a.AnchorCompanyId,a.InvoiceNo,a.ParentAccountTransactionsID,a.ReferenceId,d.Code,t.Code
 ) a 
 Group by ProductId,NBFCCompanyId,AnchorCompanyId,InvoiceNo,AccountTransactionId,ReferenceId,TransType

 Select --a.InvoiceNo,a.TransactionNo,
       a.NBFCCompanyId,
       a.AccountTransactionId,a.TransType InvoiceTransactionType ,a.InvoiceAmt
	   --,a.DisbursementDate
       ,a.principalAmt TotalAmount,a.PaymentAmt PayableAmount
	   ,case when a.IsSharable=1 then a.ScaleupRate else 0 end SharePercent	   
	   ,case when a.IsSharable=1 then 
	    case when a.TransType='BouncePaymentAmount' then a.ScaleupRate 
		    else  (a.PaymentAmt/a.AnchorRate) * a.ScaleupRate  end
	   else 0 end ScaleupShare
	   Into #tempCompanyInvoice
  from
 (
 select a.NBFCCompanyId,a.InvoiceNo,a.ReferenceId TransactionNo,a.AccountTransactionId,a.TransType,a.PaymentDate
  , abs(p.Amt)  InvoiceAmt
  , d.Amt  principalAmt
  ,abs(a.Amt) PaymentAmt 
  ,Case when a.TransType='Interest' or a.TransType='InterestPaymentAmount' then  b.AnnualInterestRate 
        when a.TransType='PenaltyCharges' or a.TransType='PenalPaymentAmount' then  b.PenaltyCharges 
        when a.TransType='BounceCharge' or a.TransType='BouncePaymentAmount' then  b.BounceCharges 
        when a.TransType='OverdueInterest' or a.TransType='OverduePaymentAmount' then  b.AnnualInterestRate 
       
	   else 0 end
   NBFCRate
     ,Case when a.TransType='Interest' or a.TransType='InterestPaymentAmount' then  c.AnnualInterestRate 
       when a.TransType='PenaltyCharges' or a.TransType='PenalPaymentAmount' then  c.DelayPenaltyRate 
       when a.TransType='BounceCharge' or a.TransType='BouncePaymentAmount' then  c.BounceCharge 
        when a.TransType='OverdueInterest' or a.TransType='OverduePaymentAmount' then  c.AnnualInterestRate

       else 0 end
   AnchorRate  
   ,
    (Case when a.TransType='Interest' or a.TransType='InterestPaymentAmount' then  c.AnnualInterestRate 
       when a.TransType='PenaltyCharges' or a.TransType='PenalPaymentAmount' then  c.DelayPenaltyRate 
       when a.TransType='BounceCharge' or a.TransType='BouncePaymentAmount' then  c.BounceCharge 
        when a.TransType='OverdueInterest' or a.TransType='OverduePaymentAmount' then  c.AnnualInterestRate
	   else 0 end
	) -
	(Case when a.TransType='Interest' or a.TransType='InterestPaymentAmount' then  b.AnnualInterestRate 
        when a.TransType='PenaltyCharges' or a.TransType='PenalPaymentAmount' then  b.PenaltyCharges 
        when a.TransType='BounceCharge' or a.TransType='BouncePaymentAmount' then  b.BounceCharges 
        when a.TransType='OverdueInterest' or a.TransType='OverduePaymentAmount' then  b.AnnualInterestRate 
        
	   else 0 end)
   ScaleupRate 
   
    
   ,Case when  a.TransType='InterestPaymentAmount' then b.IsInterestRateCoSharing 
        when a.TransType='PenalPaymentAmount' then b.IsPenaltyChargeCoSharing
        when a.TransType='BouncePaymentAmount' then  b.IsBounceChargeCoSharing 
		when a.TransType='OverduePaymentAmount' then 1 
		when a.TransType='Payment' then 0 
       else '' end IsSharable
  from #tempFinalInvoice a 
  left Join #tempFinalInvoice d on a.AccountTransactionId=d.AccountTransactionId
  and d.TransType = (Case when  a.TransType='InterestPaymentAmount' then  'Interest' 
        when a.TransType='PenalPaymentAmount' then  'PenaltyCharges'
        when a.TransType='BouncePaymentAmount' then  'BounceCharge' 
		when a.TransType='OverduePaymentAmount' then 'OverdueInterest' 
		when a.TransType='Payment' then 'Order' 
       else '' end)
 left Join #tempFinalInvoice p on a.AccountTransactionId=p.AccountTransactionId
  and p.TransType = 'Payment'
 outer apply
 (   
  Select top 1 b.AnnualInterestRate,b.BounceCharges,b.IsBounceChargeCoSharing,b.IsInterestRateCoSharing
  ,b.IsPenaltyChargeCoSharing,b.IsPlatformFeeCoSharing,b.PenaltyCharges,b.PlatformFee	
  from ScaleUP_Product.dbo.ProductNBFCCompany b where b.CompanyId=a.NBFCCompanyId and b.ProductId=a.ProductId
  and a.PaymentDate between b.AgreementStartDate and b.AgreementEndDate
  order by b.Id desc
 ) b 
 outer apply
 (
  Select top 1 c.AnnualInterestRate,c.BounceCharge,c.DelayPenaltyRate from  ScaleUP_Product.dbo.ProductAnchorCompany c 
  where c.CompanyId=a.AnchorCompanyId and c.ProductId=a.ProductId
  and a.PaymentDate between c.AgreementStartDate and c.AgreementEndDate  
  order by c.Id desc
 ) c
 where a.TransType like '%payment%' and a.TransType!='Payment'
 ) a
 order by a.InvoiceNo,a.AccountTransactionId
 

 if(exists(select * from #tempCompanyInvoice))
 begin
    DECLARE	@InvoiceData as table(InvoiceNo varchar(100))
	DECLARE @invoiceNo varchar(100)
	Insert into @InvoiceData
	exec [dbo].[spGetCurrentNumber] 'CompanyInvoiceNo'
	
	set @invoiceNo =(SELECT top 1 InvoiceNo from @InvoiceData)


   Insert into CompanyInvoices(CompanyId,InvoiceNo,InvoiceDate,InvoiceAmount,[Status],Created,CreatedBy,IsActive,IsDeleted)
   Select a.NBFCCompanyId,@invoiceNo,Getdate(),sum(a.ScaleupShare),0,getdate(),'System',1,0 from #tempCompanyInvoice a group by NBFCCompanyId

   Insert into CompanyInvoiceDetails(CompanyInvoiceId,AccountTransactionId,InvoiceTransactionType,InvoiceAmt
       ,TotalAmount,PayableAmount,SharePercent,ScaleupShare,Created,CreatedBy,IsActive,IsDeleted)
    Select SCOPE_IDENTITY(),a.AccountTransactionId,a.InvoiceTransactionType,a.InvoiceAmt
	 ,a.TotalAmount,a.PayableAmount,a.SharePercent,a.ScaleupShare,getdate(),'System',1,0
	 from #tempCompanyInvoice a group by NBFCCompanyId

 end

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

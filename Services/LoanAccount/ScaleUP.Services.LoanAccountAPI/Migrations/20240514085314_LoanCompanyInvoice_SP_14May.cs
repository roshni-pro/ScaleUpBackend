using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class LoanCompanyInvoice_SP_14May : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			var sp1 = @"
CREATE OR ALTER   Procedure [dbo].[GetCompanyInvoiceDetails]
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
		,Round(SUM(case when cd.InvoiceTransactionType = 1 then TotalAmount else 0 end),2) as ProcessingFeeTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 2 then TotalAmount else 0 end),2) as InterestTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 3 then TotalAmount else 0 end),2) as OverDueInterestTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 4 then TotalAmount else 0 end),2) as PenalTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 5 then TotalAmount else 0 end),2) as BounceTotal
		,Round(inv.InvoiceAmount,2) InvoiceAmount
		,Round(sum(cd.ScaleupShare),2) as ScaleupShare
		,la.NBFCCompanyId
		,Round(sum(cd.TotalAmount),2) as TotalAmount
		,cd.IsActive IsActive
		,'' StatusName
		,cast(0 as bit) IsCheckboxVisible
		,ac.GSTAmount GST
		,Isnull(ci.InvoiceUrl,'') InvoiceUrl
		from CompanyInvoices ci  with(nolock)
		inner join CompanyInvoiceDetails cd with(nolock) on ci.Id = cd.CompanyInvoiceId and ci.IsActive=1 and ci.IsDeleted=0 
		inner join AccountTransactions ac with(nolock) on ac.Id = cd.AccountTransactionId
		inner join Invoices inv with(nolock) on inv.Id=ac.InvoiceId
		inner join LoanAccounts la with(nolock) on la.Id = ac.LoanAccountId and la.IsActive=1 and la.IsDeleted=0
		where ci.InvoiceNo = @InvoiceNo
		 AND (
              (@RoleName = 'MakerUser' AND 1 = 1)
              OR
              (@RoleName = 'checkerUser' and ci.status=1 and ci.status!=3 AND cd.IsActive = 1 AND cd.IsDeleted = 0))
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

Go;
CREATE OR ALTER   Procedure [dbo].[GetCompanyInvoicesList]
	--declare
	@NBFCId bigint=3,
	@AnchorId dbo.Intvalues readonly,
	@FromDate datetime='2024-05-01',
	@ToDate datetime='2024-05-31',
	@Skip int=0,
	@Take int=10,
	@Status int=100
	As
	Begin
	--insert into @AnchorId values(2);
	select '' as NBFCName
		,ci.InvoiceNo
		,ci.InvoiceDate,ci.Status
		,ci.PaymentDate
		,ci.PaymentReferenceNo as PaymentRefNo
		,ci.CompanyId NBFCCompanyId
		,Round(isnull(cid.Amount,0),2) Amount
		,cid.ReferenceNo
		,cid.AnchorCompanyId
		,isnull(ci.InvoiceUrl,'') InvoiceUrl
		,'' StatusName
		,'' CompanyEmail
		,ci.Created CreatedDate
		,'' UserType
		,ci.Id CompanyInvoiceId
		,COUNT(*) over() as TotalCount
	from CompanyInvoices ci with(nolock)	
	outer apply (select act.AnchorCompanyId,max(act.ReferenceId) ReferenceNo,sum(cd.TotalAmount) Amount
				 from CompanyInvoiceDetails cd with(nolock)
				inner join AccountTransactions act with(nolock) on cd.AccountTransactionId=act.Id
				inner join @AnchorId a on act.AnchorCompanyId=a.IntValue
				 where ci.Id = cd.CompanyInvoiceId and ci.IsActive=1 and ci.IsDeleted=0 
				group by act.AnchorCompanyId--,act.ReferenceId 
	) cid
	where ci.CompanyId = @NBFCId
	and (@Status=ci.Status or @Status =100)
	and CAST(ci.InvoiceDate as date)>=@FromDate and CAST(ci.InvoiceDate as date)<=@ToDate
	--group by ci.Id,ci.CompanyId,ci.InvoiceNo,ci.InvoiceDate,ci.Status,ci.PaymentDate,ci.PaymentReferenceNo
	order by ci.InvoiceDate 
	OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
	End
Go

CREATE OR ALTER   Procedure [dbo].[GetCompanyInvoicesCharges]
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
		round(SUM(case when cd.InvoiceTransactionType = 1 then ScaleupShare else 0 end),2) as ProcessingFee,
		round(SUM(case when cd.InvoiceTransactionType = 2 then ScaleupShare else 0 end),2) as InterestCharges,
		round(SUM(case when cd.InvoiceTransactionType = 3 then ScaleupShare else 0 end),2) as OverDueInterest,
		round(SUM(case when cd.InvoiceTransactionType = 4 then ScaleupShare else 0 end),2) as PenalCharges,
		round(SUM(case when cd.InvoiceTransactionType = 5 then ScaleupShare else 0 end),2) as BounceCharges,
		round(SUM(cd.ScaleupShare)/1.09,2) as TotalTaxableAmount,
		Round(SUM(cd.ScaleupShare)/1.09*0.18,2) as TotalGstAmount,
		Round(SUM(cd.ScaleupShare)/1.09*1.18,2) as TotalInvoiceAmount
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
CREATE OR ALTER   proc [dbo].[InsertMonthCompanyInvoice]
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
 and not exists(select 1 from CompanyInvoiceDetails ci with(nolock) where ci.AccountTransactionId=a.id and ci.IsActive=1 and ci.IsDeleted=0)
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

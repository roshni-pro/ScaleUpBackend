using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _11_06_2024_sp_GetScaleUpInvoiceWithScaleUpShare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"
                    CREATE OR ALTER   Procedure [dbo].[GetScaleUpInvoiceWithScaleUpShare]
--declare
 @FromDate datetime='2024-01-01',
  @ToDate datetime='2024-06-30',
  @NbfcCompanyId bigint
as
begin
		IF OBJECT_ID('tempdb..#tempTrans') IS NOT NULL 
		  DROP TABLE #tempTrans
		select a.InvoiceNo,a.ReferenceId TransactionNo,a.Id AccountTransactionId,
		case when c.Code='DelayPenalty' then 'PenaltyCharges'
			when c.Code='OverdueInterestAmount' then 'OverdueInterest' 
		else c.Code end InvoiceTranType
		,sum(b.Amount) TotalAmount,a.DisbursementDate,a.DueDate,a.SettlementDate, max(TRY_CONVERT(datetime,b.TransactionDate)) TransactionDate
		,a.AnchorCompanyId,l.NBFCCompanyId,l.ProductId
		Into #tempTrans
		from AccountTransactions a  with(nolock)
		 inner join LoanAccounts  l with(nolock) on a.LoanAccountId=l.Id 
		 Inner join AccountTransactionDetails b with(nolock) on a.id=b.AccountTransactionId and b.IsActive=1 and b.IsDeleted=0 
		 inner join TransactionDetailHeads c with(nolock) on b.TransactionDetailHeadId=c.Id 
		 inner join TransactionStatuses s with(nolock) on s.id=a.TransactionStatusId
		where
		 --a.TransactionStatusId=4 and
		  a.IsActive=1 and a.IsDeleted=0  
		  and b.IsActive=1 and b.IsDeleted=0 and l.IsActive=1 and l.IsDeleted=0
		  and cast(b.TransactionDate as date) < @ToDate and cast(b.TransactionDate as date) >=@FromDate	or l.NBFCCompanyId = @NbfcCompanyId 
		  --and a.InvoiceNo='MP24258025'
		  group by a.InvoiceNo,a.ReferenceId ,a.Id ,c.Code 
		  ,a.DisbursementDate,a.DueDate,a.SettlementDate ,a.AnchorCompanyId
		  ,l.NBFCCompanyId,l.ProductId
		Select a.* 
		 ,case when a.IsSharable=1 then a.ScaleupRate else 0 end SharePercent	   
			   ,case when a.IsSharable=1 then 
			    case when a.InvoiceTranType='BouncePaymentAmount' then a.ScaleupRate 
				    else  (
					  a.TransAmount
					  --(case when a.PaidAmount is not null then a.PaidAmount else a.TransAmount end)
					  /a.AnchorRate) * a.ScaleupRate  end
			   else 0 end ScaleupShare
		from(
		select a.InvoiceNo,a.TransactionNo,a.AccountTransactionId,a.InvoiceTranType
		--, Convert(varchar,a.DisbursementDate,103) DisbursementDate, Convert(varchar,a.DueDate,103) DueDate,
		, a.DisbursementDate as DisbursementDate, a.DueDate as DueDate,
		--Convert(varchar,(case when p.TotalAmount is not null then p.TransactionDate else a.SettlementDate end),103) SettlementDate
		--Convert(varchar,( p.TransactionDate),103) SettlementDate
		 p.TransactionDate as  SettlementDate
		,cast(t.TotalAmount as float) PrincipleAmount, cast(a.TotalAmount as float) TransAmount,abs(cast(p.TotalAmount as float)) PaidAmount
		,isnull(cast(Case when a.InvoiceTranType='Interest' or a.InvoiceTranType='InterestPaymentAmount' then  b.AnnualInterestRate 
		        when a.InvoiceTranType='PenaltyCharges' or a.InvoiceTranType='PenalPaymentAmount' then  b.PenaltyCharges 
		        when a.InvoiceTranType='BounceCharge' or a.InvoiceTranType='BouncePaymentAmount' then  b.BounceCharges 
		        when a.InvoiceTranType='OverdueInterest' or a.InvoiceTranType='OverduePaymentAmount' then  b.AnnualInterestRate        
			   else 0 end as float) , 0)
		   NBFCRate
		     ,cast(Case when a.InvoiceTranType='Interest' or a.InvoiceTranType='InterestPaymentAmount' then  c.AnnualInterestRate 
		       when a.InvoiceTranType='PenaltyCharges' or a.InvoiceTranType='PenalPaymentAmount' then  c.DelayPenaltyRate 
		       when a.InvoiceTranType='BounceCharge' or a.InvoiceTranType='BouncePaymentAmount' then  c.BounceCharge 
		        when a.InvoiceTranType='OverdueInterest' or a.InvoiceTranType='OverduePaymentAmount' then  c.AnnualInterestRate
		       else 0 end as float) 
		   AnchorRate  
		   ,
		    cast((Case when a.InvoiceTranType='Interest' or a.InvoiceTranType='InterestPaymentAmount' then  c.AnnualInterestRate 
		       when a.InvoiceTranType='PenaltyCharges' or a.InvoiceTranType='PenalPaymentAmount' then  c.DelayPenaltyRate 
		       when a.InvoiceTranType='BounceCharge' or a.InvoiceTranType='BouncePaymentAmount' then  c.BounceCharge 
		        when a.InvoiceTranType='OverdueInterest' or a.InvoiceTranType='OverduePaymentAmount' then  c.AnnualInterestRate
			   else 0 end
			) -
			(Case when a.InvoiceTranType='Interest' or a.InvoiceTranType='InterestPaymentAmount'       then  b.AnnualInterestRate 
		          when a.InvoiceTranType='PenaltyCharges' or a.InvoiceTranType='PenalPaymentAmount'    then  b.PenaltyCharges 
		          when a.InvoiceTranType='BounceCharge' or a.InvoiceTranType='BouncePaymentAmount'     then  b.BounceCharges 
		          when a.InvoiceTranType='OverdueInterest' or a.InvoiceTranType='OverduePaymentAmount' then  b.AnnualInterestRate 
			   else 0 end) as float) 
		   ScaleupRate 
		   ,isnull(Case when a.InvoiceTranType='Interest' or a.InvoiceTranType='InterestPaymentAmount'       then	b.IsInterestRateCoSharing 
		         when a.InvoiceTranType='PenaltyCharges' or a.InvoiceTranType='PenalPaymentAmount'    then	b.IsPenaltyChargeCoSharing
		         when a.InvoiceTranType='BounceCharge' or a.InvoiceTranType='BouncePaymentAmount'     then	b.IsBounceChargeCoSharing 
				 when a.InvoiceTranType='OverdueInterest' or a.InvoiceTranType='OverduePaymentAmount' then	cast(1 as bit) 	 									
		         else cast(0 as bit)   end , 0) IsSharable
		from #tempTrans a
		cross apply
		(		
		select sum(atd.Amount) TotalAmount
          from AccountTransactions at  with(nolock)	
		 Inner join AccountTransactionDetails atd with(nolock) on at.id=atd.AccountTransactionId and atd.IsActive=1 and atd.IsDeleted=0 
		 inner join TransactionDetailHeads c with(nolock) on atd.TransactionDetailHeadId=c.Id 
		-- inner join TransactionStatuses s with(nolock) on s.id=a.TransactionStatusId
		 where c.Code='Order' and  a.InvoiceNo=at.InvoiceNo
		 -- group by a.InvoiceNo
		) t	
		--inner join  #tempTrans t on t.InvoiceNo=a.InvoiceNo and t.InvoiceTranType='Order'
		left Join #tempTrans p on a.InvoiceNo=p.InvoiceNo
		  and p.InvoiceTranType = (Case when  a.InvoiceTranType='Interest' then  'InterestPaymentAmount' 
		        when a.InvoiceTranType='PenaltyCharges' then  'PenalPaymentAmount'
		        when a.InvoiceTranType='BounceCharge' then  'BouncePaymentAmount' 
				when a.InvoiceTranType='OverdueInterest' then 'OverduePaymentAmount' 
				when a.InvoiceTranType='Order' then 'Payment' 
		       else '' end)
		 outer apply
		 (   
		  Select top 1 b.AnnualInterestRate,b.BounceCharges,b.IsBounceChargeCoSharing,b.IsInterestRateCoSharing
		  ,b.IsPenaltyChargeCoSharing,b.IsPlatformFeeCoSharing,b.PenaltyCharges,b.PlatformFee	
		  from ScaleUP_Product_QA.dbo.ProductNBFCCompany b where b.CompanyId=a.NBFCCompanyId and b.ProductId=a.ProductId
		  and a.TransactionDate between b.AgreementStartDate and b.AgreementEndDate
		  order by b.Id desc
		 ) b 
		 outer apply
		 (
		  Select top 1 c.AnnualInterestRate,c.BounceCharge,c.DelayPenaltyRate from  ScaleUP_Product_QA.dbo.ProductAnchorCompany c 
		  where c.CompanyId=a.AnchorCompanyId and c.ProductId=a.ProductId
		  and a.TransactionDate between c.AgreementStartDate and c.AgreementEndDate  
		  order by c.Id desc
		 ) c
		where a.InvoiceTranType  not like '%payment%' and a.InvoiceTranType not in ('Payment','Order')
		) a
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

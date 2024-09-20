using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _02July_SpInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER Procedure [dbo].[GetCompanyInvoicesList]
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
	outer apply (select act.AnchorCompanyId,max(act.ReferenceId) ReferenceNo,
				--Round(SUM(cd.ScaleupShare)/1.09*1.18,2) Amount
				Round(SUM(cd.ScaleupShare)*.91*1.18,2) Amount

				 from CompanyInvoiceDetails cd with(nolock)
				inner join AccountTransactions act with(nolock) on cd.AccountTransactionId=act.Id
				inner join @AnchorId a on act.AnchorCompanyId=a.IntValue
				 where ci.Id = cd.CompanyInvoiceId and cd.IsActive=1 and cd.IsDeleted=0 
				group by act.AnchorCompanyId--,act.ReferenceId 
	) cid
	where ci.CompanyId = @NBFCId and ci.IsActive=1 and ci.IsDeleted=0 
	and (@Status=ci.Status or @Status =100)
	and CAST(ci.InvoiceDate as date)>=@FromDate and CAST(ci.InvoiceDate as date)<=@ToDate
	--group by ci.Id,ci.CompanyId,ci.InvoiceNo,ci.InvoiceDate,ci.Status,ci.PaymentDate,ci.PaymentReferenceNo
	order by ci.InvoiceDate 
	OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
	End
GO



CREATE OR ALTER  proc [dbo].[InsertMonthCompanyInvoice]
as
begin
IF OBJECT_ID('tempdb..#tempPaidTrans') IS NOT NULL 
  DROP TABLE #tempPaidTrans
IF OBJECT_ID('tempdb..#tempCompanyInvoice') IS NOT NULL 
  DROP TABLE #tempCompanyInvoice
IF OBJECT_ID('tempdb..#tempAllTrans') IS NOT NULL 
  DROP TABLE #tempAllTrans

Declare @startDate DateTime= cast(dateadd(MONTH,-1,getdate()) as date) --'2024-06-01'
,@endDate DateTime=cast(getdate() as date) --'2024-07-01'

if(not Exists(select 1 from CompanyInvoices a where month(a.InvoiceDate)=month(@startDate)))
begin
 Select a.AnchorCompanyId,l.NBFCCompanyId,l.ProductId ,a.InvoiceNo,b.TransactionDate,b.Amount,c.Code  TransType,a.ParentAccountTransactionsID 
 Into #tempPaidTrans
 from  AccountTransactions a  with(nolock)
 inner join LoanAccounts l with(nolock) on a.LoanAccountId=l.Id
 Inner join AccountTransactionDetails b with(nolock) on a.id=b.AccountTransactionId and b.IsActive=1 and b.IsDeleted=0 
 inner join TransactionDetailHeads c with(nolock) on b.TransactionDetailHeadId=c.Id 
 and round(b.Amount,4)!=0
 where   b.TransactionDate<@endDate and c.Code like '%payment%' and  c.code!='Payment' --and b.TransactionDate>=@startDate
 and not exists(select 1 from CompanyInvoiceDetails ci with(nolock) where ci.AccountTransactionId=a.ParentAccountTransactionsID and ci.IsActive=1 and ci.IsDeleted=0)
  

 Select at.Id AccountTransactionId,at.InvoiceNo,c.Code,sum(atd.amount) OrderAmount
 Into #tempAllTrans
  From AccountTransactions at  with(nolock)
 Inner join AccountTransactionDetails atd with(nolock) on at.id=atd.AccountTransactionId and atd.IsActive=1 and atd.IsDeleted=0 
 inner join TransactionDetailHeads c with(nolock) on atd.TransactionDetailHeadId=c.Id 
 inner join #tempPaidTrans pd on at.InvoiceNo=pd.InvoiceNo
 where c.Code not like '%payment%' 
  group by at.Id ,c.Code,at.InvoiceNo


  Select a.InvoiceNo,
       a.NBFCCompanyId,
       a.AccountTransactionId,a.TransType InvoiceTransactionType ,a.InvoiceAmt
	   ,a.PaymentDate
       ,a.principalAmt TotalAmount,round(a.PaymentAmt,4) PayableAmount,a.AnchorRate
	   ,case when a.IsSharable=1 then a.ScaleupRate else 0 end SharePercent	   
	   ,case when a.IsSharable=1 then 
	    case when a.TransType='BouncePaymentAmount' then a.ScaleupRate 
		    else  (round(a.PaymentAmt,4)/a.AnchorRate) * a.ScaleupRate  end
	   else 0 end ScaleupShare
	   Into #tempCompanyInvoice
  from
  (
			 Select    
				 a.NBFCCompanyId,a.InvoiceNo,od.AccountTransactionId,a.TransType,a.TransactionDate PaymentDate
			  ,OtherCharges.amount    principalAmt
			  , od.OrderAmount  InvoiceAmt
			  ,abs(a.Amount) PaymentAmt 
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
			  Select top 1 b.AnnualInterestRate,b.BounceCharges,b.IsBounceChargeCoSharing,b.IsInterestRateCoSharing
			  ,b.IsPenaltyChargeCoSharing,b.IsPlatformFeeCoSharing,b.PenaltyCharges,b.PlatformFee	
			  from ScaleUP_Product.dbo.ProductNBFCCompany b where b.CompanyId=a.NBFCCompanyId and b.ProductId=a.ProductId
			  and a.TransactionDate between b.AgreementStartDate and b.AgreementEndDate
			  order by b.Id desc
			 ) b 
			 outer apply
			 (
			  Select top 1 c.AnnualInterestRate,c.BounceCharge,c.DelayPenaltyRate from  ScaleUP_Product.dbo.ProductAnchorCompany c 
			  where c.CompanyId=a.AnchorCompanyId and c.ProductId=a.ProductId
			  and a.TransactionDate between c.AgreementStartDate and c.AgreementEndDate  
			  order by c.Id desc
			 ) c
   )a --where a.InvoiceNo='UP242510291'


  DECLARE	@InvoiceData as table(InvoiceNo varchar(100))
	DECLARE @invoiceNo varchar(100)
	Insert into @InvoiceData
	exec [dbo].[spGetCurrentNumber] 'CompanyInvoiceNo'
	set @invoiceNo = (SELECT top 1 InvoiceNo from @InvoiceData)
   
   Insert into CompanyInvoices(CompanyId,InvoiceNo,InvoiceDate,InvoiceAmount,[Status],Created,CreatedBy,IsActive,IsDeleted)
   Select a.NBFCCompanyId,@invoiceNo,DATEADD(d, -1, DATEADD(m, DATEDIFF(m, 0, @startDate) + 1, 0)),sum(a.ScaleupShare),0,getdate(),'System',1,0 from #tempCompanyInvoice a group by NBFCCompanyId
  
   Insert into CompanyInvoiceDetails(CompanyInvoiceId,AccountTransactionId,InvoiceTransactionType,InvoiceAmt
       ,TotalAmount,PayableAmount,SharePercent,ScaleupShare,Created,CreatedBy,IsActive,IsDeleted)
    Select SCOPE_IDENTITY(),a.AccountTransactionId, --a.PaymentDate,
	Case when a.InvoiceTransactionType='ProcessingFeePayment' then 1 
	     when a.InvoiceTransactionType='InterestPaymentAmount' then 2
		 when a.InvoiceTransactionType='OverduePaymentAmount' then 3
		 when a.InvoiceTransactionType='PenalPaymentAmount' then 4
		 when a.InvoiceTransactionType='BouncePaymentAmount' then 5 End
	,a.InvoiceAmt
	 ,a.TotalAmount,a.PayableAmount,a.SharePercent,a.ScaleupShare,getdate(),'System',1,0
	 from #tempCompanyInvoice a --group by NBFCCompanyId

end

  
 /*
select l.ProductId,a.DisbursementDate,l.NBFCCompanyId,a.AnchorCompanyId,a.InvoiceNo,a.Id AccountTransactionId,a.ReferenceId,c.Code TransType
,sum(b.Amount) Amt
 Into #tempPaidTrans
 from AccountTransactions a  with(nolock)
 inner join LoanAccounts  l with(nolock) on a.LoanAccountId=l.Id 
 Inner join AccountTransactionDetails b with(nolock) on a.id=b.AccountTransactionId and b.IsActive=1 and b.IsDeleted=0 
 inner join TransactionDetailHeads c with(nolock) on b.TransactionDetailHeadId=c.Id 
  inner join TransactionStatuses s with(nolock) on s.id=a.TransactionStatusId
where --a.TransactionStatusId=4 and
 a.IsActive=1 and a.IsDeleted=0
 and TransactionTypeId=2 and s.Code not in ('Failed','Canceled','Initiate')
 and not exists(select 1 from CompanyInvoiceDetails ci with(nolock) where ci.AccountTransactionId=a.id and ci.IsActive=1 and ci.IsDeleted=0)
 Group by l.ProductId,a.DisbursementDate,l.NBFCCompanyId,a.AnchorCompanyId,a.InvoiceNo,a.Id,a.ReferenceId,c.Code

Select a.* into #tempFinalInvoice from 
(
Select a.*,max(PaymentDate) Over(partition by AccountTransactionId) MaxPaidDate  from 
(
Select 
ProductId,Max(DisbursementDate) PaymentDate,NBFCCompanyId,AnchorCompanyId,InvoiceNo,AccountTransactionId,ReferenceId,TransType
,sum(Amt) Amt
 from (
	Select *,null PaymentReqNo from #tempPaidTrans
	Union all
	select c.ProductId,b.TransactionDate,l.NBFCCompanyId,a.AnchorCompanyId,a.InvoiceNo,a.ParentAccountTransactionsID AccountTransactionId,a.ReferenceId,
	Case when t.Code='OrderPayment' then d.Code else t.Code end TransType,sum(b.Amount) Amt  ,b.PaymentReqNo
	from  AccountTransactions a with(nolock) 
	 inner join LoanAccounts l with(nolock) on a.LoanAccountId=l.Id
	 inner join TransactionTypes t with(nolock) on a.TransactionTypeId=t.Id
	 Inner join #tempPaidTrans c on a.ParentAccountTransactionsID=c.AccountTransactionId and c.TransType='Order'
	 Inner join AccountTransactionDetails b with(nolock) on a.id=b.AccountTransactionId  and b.IsActive=1 and b.IsDeleted=0 and Round(b.Amount,2)!=0
	 inner join TransactionDetailHeads d with(nolock) on b.TransactionDetailHeadId=d.Id 
	 Group by c.ProductId,b.TransactionDate,l.NBFCCompanyId,a.AnchorCompanyId,a.InvoiceNo,a.ParentAccountTransactionsID,a.ReferenceId,d.Code,t.Code,b.PaymentReqNo
 ) a 
 Group by ProductId,NBFCCompanyId,AnchorCompanyId,InvoiceNo,AccountTransactionId,ReferenceId,TransType,PaymentReqNo
 ) a  
 ) a 
 
  --and 
  --a.InvoiceNo='MP24251430'

  select * from #tempFinalInvoice

 Select a.InvoiceNo,a.TransactionNo,
       a.NBFCCompanyId,
       a.AccountTransactionId,a.TransType InvoiceTransactionType ,a.InvoiceAmt
	   ,a.PaymentDate
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
--  outer apply(select sum(atd.Amount) Amt  from  AccountTransactions at  with(nolock)
--				 Inner join AccountTransactionDetails atd with(nolock) on at.id=atd.AccountTransactionId and atd.IsActive=1 and atd.IsDeleted=0 
--				 inner join TransactionDetailHeads c with(nolock) on atd.TransactionDetailHeadId=c.Id 
--				  inner join TransactionStatuses s with(nolock) on s.id=at.TransactionStatusId
--   where at.TransactionStatusId=4 and at.IsActive=1 and at.IsDeleted=0
--  and TransactionTypeId=2 and s.Code not in ('Failed','Canceled','Initiate')
--  and a.AccountTransactionId=at.Id
--  Group by atd.Id
--) d 
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
	--Insert into @InvoiceData
	--exec [dbo].[spGetCurrentNumber] 'CompanyInvoiceNo'
	set @invoiceNo = 1 --(SELECT top 1 InvoiceNo from @InvoiceData)
   
   --Insert into CompanyInvoices(CompanyId,InvoiceNo,InvoiceDate,InvoiceAmount,[Status],Created,CreatedBy,IsActive,IsDeleted)
   Select a.NBFCCompanyId,@invoiceNo,Getdate(),sum(a.ScaleupShare),0,getdate(),'System',1,0 from #tempCompanyInvoice a group by NBFCCompanyId
  
   --Insert into CompanyInvoiceDetails(CompanyInvoiceId,AccountTransactionId,InvoiceTransactionType,InvoiceAmt
   --    ,TotalAmount,PayableAmount,SharePercent,ScaleupShare,Created,CreatedBy,IsActive,IsDeleted)
    Select SCOPE_IDENTITY(),a.AccountTransactionId, --a.PaymentDate,
	Case when a.InvoiceTransactionType='ProcessingFeePayment' then 1 
	     when a.InvoiceTransactionType='InterestPaymentAmount' then 2
		 when a.InvoiceTransactionType='OverduePaymentAmount' then 3
		 when a.InvoiceTransactionType='PenalPaymentAmount' then 4
		 when a.InvoiceTransactionType='BouncePaymentAmount' then 5 End
	,a.InvoiceAmt
	 ,a.TotalAmount,a.PayableAmount,a.SharePercent,a.ScaleupShare,getdate(),'System',1,0
	 from #tempCompanyInvoice a --group by NBFCCompanyId
 end
 */
end


";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

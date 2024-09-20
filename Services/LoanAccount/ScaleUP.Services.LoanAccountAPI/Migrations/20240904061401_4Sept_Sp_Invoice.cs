using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _4Sept_Sp_Invoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER   proc [dbo].[GetDSAMISList]
--declare
@DSACompanyId int = 0,
@StartDate datetime = null,
@EndDate datetime =null
AS
BEGIN
select  
		l.LeadId SalesAgentLeadId
		,l.Id SalesAgentLoanAccountId
		,l.UserId SalesAgentUserId
		,isnull(l.CityName,'') SalesAgentCity
		,l.Type SalesAgentType
		,l.CustomerName SalesAgentName
		,isnull(CustomerLeadId,0) CustomerLeadId
		,isnull(CustomerLoanAccountId,0) CustomerLoanAccountId
		,isnull(CustomerUserId,'') CustomerUserId
		,isnull(Customer.CustomerCode,'') CustomerCode
		,isnull(Customer.CustomerName,'') CustomerName
		,isnull(Customer.CustomerLoanAccountNo,'') CustomerLoanAccountNo
		,isnull(Customer.NBFCName,'') NBFCName
		,isnull(Customer.DisbursmentDate,'') DisbursmentDate
		,isnull(Customer.SanctionAmount,0) SanctionAmount
		,isnull(Customer.DisbursementAmount,0) DisbursementAmount
		,at.InterestRate PayoutPercentage
		,isnull(sum(case when h.Code = 'Commission' then atd.Amount end),0) PayoutAmount
		,isnull(sum(case when h.Code = 'GST18' then atd.Amount end),0) GSTAmount
		,isnull(sum(case when h.Code = 'Commission' or h.Code ='GST18' then atd.Amount end),0) TotalAmount
		,isnull(abs(sum(case when h.Code = 'Tds5' then atd.Amount end)),0) TDSAmount
		,isnull(sum(case when h.Code = 'Commission' or h.Code ='GST18' then atd.Amount end) - abs(sum(case when h.Code = 'Tds5' then atd.Amount end)),0) NetPayoutAmount
	    from LoanAccounts l with(nolock)
join AccountTransactions at with(nolock) on at.LoanAccountId = l.Id and l.IsActive =1 and l.IsDeleted=0 and at.IsActive =1 and at.IsDeleted=0
join AccountTransactionDetails atd with(nolock) on atd.AccountTransactionId = at.Id and atd.IsActive =1 and atd.IsDeleted=0
join TransactionDetailHeads h with(nolock) on h.Id = atd.TransactionDetailHeadId and h.IsActive =1 and h.IsDeleted=0
cross apply
(
	select top 1 
	 l2.LeadId CustomerLeadId
	,l2.Id CustomerLoanAccountId
	,l2.UserId CustomerUserId
	,l2.AccountCode CustomerLoanAccountNo
	,l2.NBFCIdentificationCode NBFCName
	,at2.Created DisbursmentDate
	,atd2.Amount DisbursementAmount
	,atd2.Amount SanctionAmount
	,cl.UserUniqueCode CustomerCode
	,l2.CustomerName
	from LoanAccounts l2 with(nolock)
	join AccountTransactions at2 with(nolock) on at2.LoanAccountId = l2.Id and l2.IsActive =1 and l2.IsDeleted=0 and at2.IsActive =1 and at2.IsDeleted=0
	join AccountTransactionDetails atd2 with(nolock) on atd2.AccountTransactionId = at2.Id and atd2.IsActive =1 and atd2.IsDeleted=0
	join TransactionDetailHeads h2 with(nolock) on h2.Id = atd2.TransactionDetailHeadId and h2.IsActive =1 and h2.IsDeleted=0
	left join LoanAccountCompanyLead cl with(nolock) on cl.LoanAccountId =l2.Id and cl.IsActive =1 and cl.IsDeleted=0
	where at2.Id = at.ParentAccountTransactionsID
	and h2.Code ='DisbursementAmount'
	and ((@StartDate is null and @EndDate is null) or (cast(at2.Created as date)>=cast(@StartDate as date) and cast(at2.Created as date)<=cast(@EndDate as date)))
)Customer
where Type in('DSA','DSAUser','Connector')
and (@DSACompanyId =0 or @DSACompanyId=l.ProductId)
group by l.LeadId 
		,l.Id 
		,l.UserId 
		,l.CityName 
		,l.Type 
		,l.CustomerName 
		,CustomerLeadId
		,CustomerLoanAccountId
		,CustomerUserId
		,Customer.CustomerCode
		,Customer.CustomerName
		,Customer.CustomerLoanAccountNo
		,Customer.NBFCName
		,Customer.DisbursmentDate
		,Customer.SanctionAmount
		,Customer.DisbursementAmount
		,at.InterestRate 
order by l.CustomerName 
end 
GO



CREATE OR ALTER   proc [dbo].[GetCompanyInvoiceDetails]
--declare
@InvoiceNo varchar(100)='SU25012',
@RoleName varchar(100)='MakerUser'
AS
Begin	
if(OBJECT_ID('tempdb..#TempInvoiceDetail') is not null)
drop table #TempInvoiceDetail

select
cd.AccountTransactionId
,ac.ReferenceId ReferenceNo
, ci.Id CompanyInvoiceId
,ac.CustomerUniqueCode as AnchorCode
,ac.AnchorCompanyId
,'' AnchorName
,'' as NBFCName
,inv.InvoiceNo
,inv.InvoiceDate
,ci.InvoiceDate CompanyInvoiceDate
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
,Round((sum(cd.ScaleupShare))*0.18,2) GST
--,ac.GSTAmount GST
,Isnull(ci.InvoiceUrl,'') InvoiceUrl
,Round(sum(case when cd.InvoiceTransactionType = 1 then cd.ScaleupShare else 0 end),2) as processingFeeScaleupShare
,Round(SUM(case when cd.InvoiceTransactionType = 2 then cd.ScaleupShare else 0 end),2) as interestScaleupShare
,Round(SUM(case when cd.InvoiceTransactionType = 3 then cd.ScaleupShare else 0 end),2) as overDueInterestScaleupShare
,Round(SUM(case when cd.InvoiceTransactionType = 4 then cd.ScaleupShare else 0 end),2) as penalScaleupShare
,Round(SUM(case when cd.InvoiceTransactionType = 5 then cd.ScaleupShare else 0 end),2) as bounceScaleupShare
,isnull(bat.TopUpNumber,'') topUpNumber
--,isnull(ThirdParty.ThirdPartyTxnId,'') ThirdPartyTxnId
into #TempInvoiceDetail
from CompanyInvoices ci  with(nolock)
inner join CompanyInvoiceDetails cd with(nolock) on ci.Id = cd.CompanyInvoiceId and ci.IsActive=1 and ci.IsDeleted=0 
inner join AccountTransactions ac with(nolock) on ac.Id = cd.AccountTransactionId 
inner join Invoices inv with(nolock) on inv.Id=ac.InvoiceId
inner join LoanAccounts la with(nolock) on la.Id = ac.LoanAccountId and la.IsActive=1 and la.IsDeleted=0
left join BlackSoilAccountTransactions bat with(nolock) on bat.LoanInvoiceId = inv.Id  and bat.IsActive=1 and bat.IsDeleted=0
where ci.InvoiceNo = @InvoiceNo 
--AND ((@RoleName = 'MakerUser') OR (@RoleName = 'checkerUser' and ci.status=1 and ci.status!=3 AND cd.IsActive = 1 AND cd.IsDeleted = 0))
and cd.IsActive = 1 AND cd.IsDeleted = 0
group by  cd.AccountTransactionId,ac.CustomerUniqueCode
,la.AnchorName
,inv.InvoiceNo
,inv.InvoiceDate
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
	,bat.TopUpNumber
	,ci.InvoiceDate
	--,ThirdParty.ThirdPartyTxnId


select cd.*
,STRING_AGG(ThirdParty.ThirdPartyTxnId,',') ThirdPartyTxnId
from #TempInvoiceDetail cd
outer apply 
(
	select  distinct P.ThirdPartyTxnId ThirdPartyTxnId 
	from AccountTransactionDetails  A  With(nolock) 
	Inner Join TransactionDetailHeads M ON A.TransactionDetailHeadId=M.Id 
	Left Outer Join LoanAccountRepayments P ON A.PaymentReqNo=P.BankRefNo 
	Where a.IsActive =1 and a.IsDeleted=0
	and (m.Code ='InterestPaymentAmount' or m.Code ='PenalPaymentAmount' or m.Code ='OverduePaymentAmount' or m.Code ='BouncePaymentAmount')
	and month(cd.CompanyInvoiceDate) = month(a.TransactionDate) and Year(cd.CompanyInvoiceDate) = Year(a.TransactionDate)
	and  A. AccountTransactionId in 
	(
		Select at.Id From  AccountTransactions at
		Inner Join TransactionTypes tt ON tt.Id=at.TransactionTypeId
		Where tt.Code='OrderPayment' And ParentAccountTransactionsID = cd.AccountTransactionId
	)
)ThirdParty
group by  cd.AccountTransactionId
,AnchorCode
,AnchorName
,InvoiceNo
,InvoiceDate
,InvoiceAmount
,NBFCCompanyId
,CompanyInvoiceId
,status
,AnchorCompanyId
,cd.AccountTransactionId
,ReferenceNo
,IsActive
,cd.BounceTotal,cd.GST,cd.interestScaleupShare,cd.InterestTotal,cd.InvoiceUrl
,processingFeeScaleupShare
,interestScaleupShare
,overDueInterestScaleupShare
,penalScaleupShare
,bounceScaleupShare
,NBFCName
,ProcessingFeeTotal
,OverDueInterestTotal
,PenalTotal
,ScaleupShare
,TotalAmount
,StatusName
,IsCheckboxVisible
,topUpNumber
,cd.CompanyInvoiceDate
End
GO



CREATE OR ALTER     proc [dbo].[InsertMonthCompanyInvoice_New]
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
 Select l.NBFCCompanyId,v.Id InvoiceId,v.InvoiceNo,b.TransactionDate,b.Amount,c.Code  TransType,a.ParentAccountTransactionsID ,b.PaymentReqNo
 Into #tempPaidTrans
 from  AccountTransactions a  with(nolock)
 inner join TransactionTypes tt with(nolock) on a.TransactionTypeId=tt.Id
 inner join LoanAccounts l with(nolock) on a.LoanAccountId=l.Id
 Inner join AccountTransactionDetails b with(nolock) on a.id=b.AccountTransactionId and b.IsActive=1 and b.IsDeleted=0 
 inner join TransactionDetailHeads c with(nolock) on b.TransactionDetailHeadId=c.Id 
 inner join Invoices v with(nolock) on a.InvoiceId=v.Id
 and round(b.Amount,4)!=0 and l.IsActive=1 and l.IsDeleted =0 and a.IsActive=1 and a.IsDeleted =0 and b.IsActive=1 and b.IsDeleted =0
 where   b.TransactionDate<@endDate and c.Code like '%payment%' and  c.code!='Payment' and b.TransactionDate>=@startDate
 and tt.Code not like '%scaleup%' --and a.InvoiceNo is not null
 --and not exists(select 1 from CompanyInvoiceDetails ci with(nolock) where ci.AccountTransactionId=a.ParentAccountTransactionsID and ci.IsActive=1 and ci.IsDeleted=0
 --and round(abs(ci.PayableAmount),2) = round(abs(b.Amount),2))
 Select at.Id AccountTransactionId,v.Id InvoiceId,v.InvoiceNo,c.Code,sum(atd.amount) OrderAmount
 Into #tempAllTrans
  From AccountTransactions at  with(nolock)
 Inner join AccountTransactionDetails atd with(nolock) on at.id=atd.AccountTransactionId and atd.IsActive=1 and atd.IsDeleted=0 
 inner join TransactionDetailHeads c with(nolock) on atd.TransactionDetailHeadId=c.Id 
 inner join Invoices v with(nolock) on v.Id=at.InvoiceId 
 inner join #tempPaidTrans pd on v.Id=pd.InvoiceId
 where c.Code not like '%payment%' and c.Code not like '%scaleup%' and at.IsActive=1 and at.IsDeleted =0 and atd.IsActive=1 and atd.IsDeleted =0
  group by at.Id ,c.Code,v.Id,v.InvoiceNo
  Select a.InvoiceNo
       ,a.NBFCCompanyId
       ,a.AccountTransactionId
	   ,a.TransType InvoiceTransactionType 
	   ,a.InvoiceAmt
	   ,a.PaymentDate
       ,a.principalAmt TotalAmount,round(a.PaymentAmt,4) PayableAmount
	   ,SharePercent
	   ,ScaleupShare
	   Into #tempCompanyInvoice
  from
  (
			 Select    
				 a.InvoiceNo
				 ,a.NBFCCompanyId
				 ,a.ParentAccountTransactionsID AccountTransactionId
				 ,a.TransType
				 ,a.TransactionDate PaymentDate
			  ,OtherCharges.principalAmt
			  , od.InvoiceAmt
			  ,abs(a.Amount) PaymentAmt 
			  ,ScaleUp.AccountTransactionId sc
			  ,ScaleUp.Code
			  ,ScaleUp.SharePercent
			  ,ScaleUp.ScaleupShare
			  from 
			  #tempPaidTrans a
			  outer apply
			  (
			      Select sum(at.OrderAmount) InvoiceAmt From #tempAllTrans at
			       where at.InvoiceId=a.InvoiceId and at.Code= 'Order' 
			  ) od
			  outer apply
			  (
			      Select sum(at.OrderAmount) principalAmt From #tempAllTrans at
			       where at.InvoiceNo=a.InvoiceNo and at.Code= (Case when  a.TransType='InterestPaymentAmount' then  'Interest' 
			        when a.TransType='PenalPaymentAmount' then  'DelayPenalty'
			        when a.TransType='BouncePaymentAmount' then  'BounceCharge' 
					when a.TransType='OverduePaymentAmount' then 'OverdueInterestAmount' 
			       else '' end)
			  ) OtherCharges
			  outer apply
			  (
			      Select top 1 at.Id AccountTransactionId,atd.Id,tdh.Code,sum(atd.Amount) ScaleupShare 
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
				   and a.PaymentReqNo = atd.PaymentReqNo
				   --and atd.TransactionDate<@endDate  and atd.TransactionDate>=@startDate
				   group by at.Id,atd.Id,tdh.Code,tdh.Code,at.InterestRate,at.DelayPenaltyRate,at.BounceCharge
			  ) ScaleUp
   )a where ScaleupShare is not null
   select * from  #tempCompanyInvoice --where InvoiceNo ='MP242533106'
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
	 from #tempCompanyInvoice a 
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

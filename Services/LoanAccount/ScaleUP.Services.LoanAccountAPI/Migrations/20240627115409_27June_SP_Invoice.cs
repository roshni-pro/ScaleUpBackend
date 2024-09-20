using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _27June_SP_Invoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER     Procedure [dbo].[GetCompanyInvoicesList]
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
GO




CREATE OR ALTER     Procedure [dbo].[GetCompanyInvoiceDetails]
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




CREATE OR ALTER     Procedure [dbo].[GetCompanyInvoicesCharges]
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
		Round(SUM(cd.ScaleupShare)*.91,2) as TotalTaxableAmount,
		Round(SUM(cd.ScaleupShare)*.91*0.18,2) as TotalGstAmount,
		Round(SUM(cd.ScaleupShare)*.91*1.18,2) as TotalInvoiceAmount
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



CREATE OR ALTER   Procedure [dbo].[GetAnchorMISList]
	--declare
	@AnchorId  bigint=0
	,@Status varchar(200)='ALL'
	,@FromDate datetime='2024-06-12'
	,@ToDate datetime='2024-06-12'
As
Begin
--set @FromDate='2000-03-07'
--set @ToDate='2024-09-07'

		DECLARE @StartDate date,
				@EndDate date
		IF((@FromDate is null or @FromDate='') and (@ToDate is null or @ToDate=''))
			Begin
				SELECT @StartDate = MIN(c.Created),  @EndDate = max(c.Created) FROM LoanAccounts c with(nolock) where c.IsActive=1 and c.IsDeleted=0
			End
Select distinct
t.ReferenceId, LA.AccountCode as LoanID, LA.AnchorName
, i.OrderNo , i.Status InvoiceStatus, i.InvoiceNo, i.InvoiceDate, i.InvoiceAmount   
, LA.ShopName BusinessName
, lc.UserUniqueCode as AnchorCode  
,'?' NBFCName
, t.DisbursementDate ,tp.paymentAmount DisbursalAmount 
, cast(0 as float) ServiceFee, cast(0 as float)*(18/100) GST, '?'BeneficiaryName, '?' BeneficiaryAccountNumber	, i.NBFCUTR as UTR, i.NBFCStatus as Status
, LA.LeadId, LA.NBFCCompanyId, LA.ProductId, LA.AnchorCompanyId 
from LoanAccounts LA with(nolock)
Inner join LoanAccountCompanyLead lc with(nolock) on LA.Id=lc.LoanAccountId and lc.IsActive=1 and lc.IsDeleted=0
Inner Join AccountTransactions t with(nolock) on LA.Id=t.LoanAccountId and t.IsActive=1 and t.IsDeleted=0
Inner Join Invoices i with(nolock) ON LA.Id=i.LoanAccountId and i.Id =t.InvoiceId  and i.IsActive=1 and i.IsDeleted=0
inner join TransactionTypes ty on ty.Id = t.TransactionTypeId
outer apply(select st.amount paymentAmount from BlackSoilAccountTransactions tp with(nolock)
Inner join InvoiceDisbursalProcesseds st with(nolock) on tp.id=st.AccountTransactionId 
 where tp.LoanInvoiceId=i.Id) tp
Where LA.IsActive=1 and LA.IsDeleted=0
and ty.Code = 'OrderPlacement' --and i.invoiceNo in ('MP24253465')
and (i.Status=@Status or @Status='All')
and (t.AnchorCompanyId = @AnchorId or @AnchorId=0)
and (CAST(t.DisbursementDate as date)>=@FromDate and CAST(t.DisbursementDate as date)<=@ToDate)
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

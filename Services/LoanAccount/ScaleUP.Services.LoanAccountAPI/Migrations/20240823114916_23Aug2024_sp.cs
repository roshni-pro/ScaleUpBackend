﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _23Aug2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"


	CREATE OR ALTER PROC [dbo].[GetDueDisbursmentDetails]
	AS
	BEGIN
		declare @messageBeforeDays int =4;
			select 
			i.OrderNo
			, Round(sum(ad.Amount),2) Amount
			,i.Status Status
			,l.MobileNo
			,LEFT(l.CustomerName, CHARINDEX(' ', l.CustomerName + ' ') - 1) UserName
			,l.AnchorName
			,cast(a.DueDate as date) DueDate
			,i.InvoiceNo
			,'..XX'+RIGHT(lb.AccountNumber, 4) BankAccountNo
			from Invoices i with(nolock)
			inner join AccountTransactions a  with(nolock) on a.InvoiceId=i.Id
			inner join TransactionTypes t  with(nolock) on a.TransactionTypeId=t.Id 
			inner join AccountTransactionDetails ad  with(nolock) on ad.AccountTransactionId=a.Id 
			inner join TransactionDetailHeads h  with(nolock) on h.id=ad.TransactionDetailHeadId
			inner join LoanAccounts l with(nolock) on a.LoanAccountId=l.Id
			inner join LoanBankDetails lb  with(nolock) on a.LoanAccountId  = lb.LoanAccountId 
			inner join dbo.GetTransactionTypeId() fn on fn.id =a.TransactionTypeId
			where a.IsActive=1 And a.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0 
			and i.Status in ('Disbursed','Pending','Due') and a.DisbursementDate is not null
			and (ad.TransactionDate IS NULL OR CAST(ad.TransactionDate as date) <= cast(getdate() as date))
			and cast(a.DueDate as date)<=DATEADD(day,@messageBeforeDays,cast(getdate() as date))
			--and ad.IsPayableBy=1
			Group BY  i.OrderNo,l.MobileNo,l.CustomerName,l.AnchorName,cast(a.DueDate as date),i.InvoiceNo,i.Status,lb.AccountNumber
	END

GO


   CREATE OR ALTER procedure [dbo].[SendOverdueMessageJobData]
	AS
	BEGIN
	select 
		i.OrderNo
		, Round(sum(ad.Amount),2) Amount
		,l.MobileNo
		,LEFT(l.CustomerName, CHARINDEX(' ', l.CustomerName + ' ') - 1) CustomerName
		,l.AnchorName
		,cast(a.DueDate as date) DueDate
		,i.InvoiceNo
		,i.Status
		from Invoices i with(nolock)
		inner join AccountTransactions a  with(nolock) on a.InvoiceId=i.Id
		inner join TransactionTypes t  with(nolock) on a.TransactionTypeId=t.Id 
		inner join AccountTransactionDetails ad  with(nolock) on ad.AccountTransactionId=a.Id 
		inner join TransactionDetailHeads h  with(nolock) on h.id=ad.TransactionDetailHeadId
		inner join LoanAccounts l with(nolock) on a.LoanAccountId=l.Id
		inner join dbo.GetTransactionTypeId() fn on fn.id =a.TransactionTypeId
		where a.IsActive=1 And a.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0 
		and i.Status = 'overdue' and a.DisbursementDate is not null
		--and (ad.TransactionDate IS NULL OR CAST(ad.TransactionDate as date) <= cast(getdate() as date))
		--and ad.IsPayableBy=1
		Group BY  i.OrderNo,l.MobileNo,l.CustomerName,l.AnchorName,cast(a.DueDate as date),i.InvoiceNo,i.Status
		order by OrderNo
	END

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
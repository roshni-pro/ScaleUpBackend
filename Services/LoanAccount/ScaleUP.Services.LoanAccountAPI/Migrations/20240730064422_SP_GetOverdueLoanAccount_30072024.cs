using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class SP_GetOverdueLoanAccount_30072024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
CREATE OR ALTER Proc [dbo].[GetOverdueLoanAccount] 
	--declare 
	@LoanAccountIdLists dbo.intvalues readonly    
	AS
	Begin
		--insert into @LoanAccountIdLists values(9)
		--insert into @LoanAccountIdLists values(23)
		--insert into @LoanAccountIdLists values(4)

		select Distinct a.LoanAccountId, case when s.code is not null then cast(1 as bit) else cast(0 as bit) end TransactionStatus
		from AccountTransactions a with(nolock) 
		inner join TransactionStatuses s with(nolock) on a.TransactionStatusId = s.Id 
		where 
		  a.LoanAccountId IN (Select * From @LoanAccountIdLists) 
		  and s.Code = 'Overdue' 
		  and a.IsActive = 1 
		  and a.IsDeleted = 0
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

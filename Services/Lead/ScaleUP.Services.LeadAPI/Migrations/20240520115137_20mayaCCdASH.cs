using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _20mayaCCdASH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var SP = @"

CREATE OR ALTER   procedure [dbo].[AccountDashboard]  
	--declare  
	 @CityId dbo.Intvalues readonly,          
	 @AnchorId dbo.Intvalues readonly,  
	 @FromDate datetime ='2023-01-01',          
	 @ToDate datetime ='2024-05-017',  
	 @ProductId bigint =1,
	 @ProductType varchar(50) ='BusinessLoan'
as  
begin   
	--  insert into @CityId values(1),(9),(10) ,(11) ,(13) ,(14) ,(17) ,(26) ,(29) ,(31) ,(49) 
	--insert into @AnchorId values(2)
	--insert into @AnchorId values(14) 
	  SELECT l.Id LeadId,l.Created ,l.ProductId,l.Status 
	  FROM leads l  with(nolock)  
	  inner join CompanyLead cl with(nolock) on l.Id = cl.LeadId   
	  inner join @cityid c on  c.IntValue = l.CityId  
	  inner join @AnchorId a on  a.IntValue = cl.CompanyId  
	  and l.ProductId = @ProductId  and l.IsDeleted=0 and l.IsActive=1
	  and ( ( cast(l.Created as date) between cast(@FromDate as date) and  cast(@ToDate as date)) ) 
	  and 
	  (
      (@ProductType = 'BusinessLoan' AND Status IN ('Pending', 'LoanApproved', 'LoanInitiated', 'LoanActivated', 'LoanRejected', 'LoanAvailed'))
      OR (@ProductType = 'CreditLine' AND Status IN ('Pending','LineInitiated','LineActivated','LineApproved','LineRejected'))
		)
end 
";
            migrationBuilder.Sql(SP);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

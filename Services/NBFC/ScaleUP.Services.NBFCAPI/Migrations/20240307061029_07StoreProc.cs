using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.NBFCAPI.Migrations
{
    /// <inheritdoc />
    public partial class _07StoreProc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

GO

DROP TYPE [dbo].[NBFCCompanyOffer]
GO
/****** Object:  UserDefinedTableType [dbo].[NBFCCompanyOffer]    Script Date: 07-03-2024 18:23:49 ******/
CREATE TYPE [dbo].[NBFCCompanyOffer] AS TABLE(
	[companyId] [bigint] NULL,
	[leadId] [bigint] NULL,
	[VintageDays] [int] NULL,
	[AvgMonthlyBuying] [float] NULL,
	[CibilScore] [int] NULL,
	[CustomerType] [nvarchar](200) NULL
)
GO
/****** Object:  StoredProcedure [dbo].[GetQualifiedOffer]    Script Date: 07-03-2024 18:23:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER Procedure [dbo].[GetQualifiedOffer]  
--Declare
@NBFCCompanyOffers dbo.NBFCCompanyOffer readonly
As  
begin
	select   
	       l.LeadId,
	       l.CompanyId,
		    Isnull(case when 
				    case 
						when 
						l.VintageDays BETWEEN ccc.MinVintageDays and ccc.MaxVintageDays and l.AvgMonthlyBuying>0 then ROUND((l.AvgMonthlyBuying*ccc.MultiPlier),0)
						else 0
					 end 
					 >= ccc.MaxCreditLimit then ROUND(ccc.MaxCreditLimit,0)
			     when 
					case  
						when l.VintageDays BETWEEN ccc.MinVintageDays and ccc.MaxVintageDays and l.AvgMonthlyBuying>0 
						then Round((l.AvgMonthlyBuying*ccc.MultiPlier),0)
						else 0
					 end 
					 < ccc.MinCreditLimit then ROUND(ccc.MinCreditLimit,0)  
				  when 
					case  
						when l.VintageDays BETWEEN ccc.MinVintageDays and ccc.MaxVintageDays and l.AvgMonthlyBuying>0 
						then round((l.AvgMonthlyBuying*ccc.MultiPlier),0)
						else 0
					 end 
				    = 0 then ROUND(ccc.MinCreditLimit,0)
				   else 
				    case
				       when l.VintageDays BETWEEN ccc.MinVintageDays and ccc.MaxVintageDays and l.AvgMonthlyBuying>0 
						then round((l.AvgMonthlyBuying*ccc.MultiPlier),0)
						else 0
			         end
				 end,0)
				  OfferAmount
		         from @NBFCCompanyOffers l 
			     left join OfferSelfConfigurations  ccc   on l.companyId=ccc.CompanyId 
			     and l.CibilScore BETWEEN ccc.MinCibilScore and ccc.MaxCibilScore 
				 and l.VintageDays BETWEEN ccc.MinVintageDays and ccc.MaxVintageDays 
				-- and l.CustomerType=ccc.CustomerType 
				 and  ccc.IsActive=1 and ccc.IsDeleted=0
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

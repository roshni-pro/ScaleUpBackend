using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.CompanyAPI.Migrations
{
    /// <inheritdoc />
    public partial class _07StoreProc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			//Test
            var sp1 = @"CREATE OR ALTER Procedure [dbo].[spGetCurrentNumber] 
--Declare
	@EntityName nvarchar(100) ='Company'
	--@StateId int 
AS 

BEGIN
set NoCount on
--SET TRANSACTION ISOLATION LEVEL SERIALIZABLE 
--SET TRANSACTION ISOLATION LEVEL SNAPSHOT

--BEGIN TRANSACTION
--SAVE TRANSACTION GetNextSequence;
 --BEGIN TRY
	Declare 
			@number bigint,						
			@prefix nvarchar(10)='',
			@suffix nvarchar(10)='',
			@nextNo bigint=1,
			@numberStr nvarchar(50),
			@NumberConfigId int,
			@query nvarchar(max),
			@defaultNo bigint,
			@startFrom bigint,
			@entityId int,
			@separator nvarchar(20),
		    @StateAlias nvarchar(50)

	select @query = EntityQuery, @defaultNo = DefaultNo, @entityId=Id, @Separator = Separator from EntityMasters  WITH (NOLOCK) where EntityName = @EntityName	

	--print 'Query: ' +  @query

	--select @StateAlias = AliasName from states  WITH (NOLOCK) where Stateid = @StateId
	set @StateAlias ='CN'
	--print 'Alias: ' +  @StateAlias

	select @NumberConfigId = cc.Id,
		   @prefix = ISNULL(cc.Prefix,''), 
		   @suffix = ISNULL(cc.Suffix,''),
		   @nextNo = ISNULL(cc.NextNumber, 1),
		   @startFrom = ISNULL(cc.StartFrom, 1)
		from EntitySerialMasters cc  WITH (UPDLOCK)
		where EntityId = @entityId --and StateId = @StateId

		--print '@prefix: ' +  @prefix

	if(@NumberConfigId is null)
	Begin
		--print 'numberconfig is null '

		--select *from EntitySerialMasters
		insert into EntitySerialMasters (EntityId,StartFrom,NextNumber,StateId, Created
		,CreatedBy	,LastModified	,LastModifiedBy	,Deleted	,DeletedBy,	IsActive,	IsDeleted
		) values
		(@entityId, 1,  @defaultNo, 0,getdate(),
		null,null,null,null,null,1,0)
		set @NumberConfigId= (select SCOPE_IDENTITY())
		set @nextNo = @defaultNo

	End

	set @number = @nextNo
	if(@suffix is not null and @suffix <> '')
		set	@numberStr = @StateAlias+ isnull(@Separator,'') +isnull(@prefix, '') + isnull(@Separator,'') +  cast(@nextNo as nvarchar(50)) + isnull(@Separator,'') + isnull(@suffix,'')
	else
		set	@numberStr = @StateAlias+ isnull(@Separator,'') + isnull(@prefix, '') + isnull(@Separator,'') +  cast(@nextNo as nvarchar(50))
		

	declare @numCount int = 1
	declare @numCnt int

	while(@numCount > 0)
	Begin

		EXECUTE sp_executesql @query, N'@numberStr nvarchar(100), @numCnt int OUTPUT', @numberStr = @numberStr,  @numCnt = @numCnt OUTPUT
		set @numCount = @numCnt

		if(@numCount > 0)
		Begin 
			--print 'numcount > 0 for ' + @numberStr

			set @number = @number + 1;
			if(@suffix is not null and @suffix <> '')
				set	@numberStr =  @StateAlias+ isnull(@Separator,'') +isnull(@prefix, '') + isnull(@Separator,'') +  cast(@number as nvarchar(50)) + isnull(@Separator,'') + isnull(@suffix,'')
			else
				set	@numberStr = @StateAlias+ isnull(@Separator,'') + isnull(@prefix, '') + isnull(@Separator,'') +  cast(@number as nvarchar(50))
			--set	@numberStr = @prefix + isnull(@Separator,'') + cast(@number as nvarchar(50)) + isnull(@Separator,'') + @suffix
	    End
	
	end
	
	set @nextNo = @number -1
	
	update EntitySerialMasters set NextNumber = @number +1 where Id = @NumberConfigId
	
	-- set	@numberStr = @prefix + isnull(@Separator,'') + cast(@number as nvarchar(50)) + isnull(@Separator,'') + @suffix

	if(@suffix is not null and @suffix <> '')
			set	@numberStr =  @StateAlias+ isnull(@Separator,'') + isnull(@prefix, '') + isnull(@Separator,'') +cast(@number as nvarchar(50)) + isnull(@Separator,'') + isnull(@suffix,'')
		else
			set	@numberStr = @StateAlias+ isnull(@Separator,'') +isnull(@prefix, '') + isnull(@Separator,'') +  cast(@number as nvarchar(50))

	select  @numberStr -- as CurrentNumber

END";
            migrationBuilder.Sql(sp1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

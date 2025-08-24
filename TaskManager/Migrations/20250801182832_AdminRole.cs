using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class AdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT Id FROM AspNetRoles WHERE Id = '54e29374-2be9-4404-a02c-a513e4393096')
                BEGIN
	                INSERT AspNetRoles (Id, [Name], [NormalizedName])
	                VALUES ('54e29374-2be9-4404-a02c-a513e4393096', 'admin', 'ADMIN');
                END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM AspNetRoles
                                WHERE Id = '54e29374-2be9-4404-a02c-a513e4393096';");
        }
    }
}

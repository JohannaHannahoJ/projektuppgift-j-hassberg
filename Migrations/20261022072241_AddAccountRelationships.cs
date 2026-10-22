using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projektuppgift_j_hassberg.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountClassId",
                table: "Accounts",
                column: "AccountClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_AccountClasses_AccountClassId",
                table: "Accounts",
                column: "AccountClassId",
                principalTable: "AccountClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_AccountClasses_AccountClassId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_AccountClassId",
                table: "Accounts");
        }
    }
}

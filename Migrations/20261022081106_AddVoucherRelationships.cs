using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projektuppgift_j_hassberg.Migrations
{
    /// <inheritdoc />
    public partial class AddVoucherRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VoucherRows_AccountId",
                table: "VoucherRows",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherRows_VoucherId",
                table: "VoucherRows",
                column: "VoucherId");

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherRows_Accounts_AccountId",
                table: "VoucherRows",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VoucherRows_Vouchers_VoucherId",
                table: "VoucherRows",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VoucherRows_Accounts_AccountId",
                table: "VoucherRows");

            migrationBuilder.DropForeignKey(
                name: "FK_VoucherRows_Vouchers_VoucherId",
                table: "VoucherRows");

            migrationBuilder.DropIndex(
                name: "IX_VoucherRows_AccountId",
                table: "VoucherRows");

            migrationBuilder.DropIndex(
                name: "IX_VoucherRows_VoucherId",
                table: "VoucherRows");
        }
    }
}

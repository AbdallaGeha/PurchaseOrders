using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseOrders.Migrations
{
    /// <inheritdoc />
    public partial class violationstatementcascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderViolationDiscounts_PurchaseOrderStatements_PurchaseOrderStatementId",
                table: "PurchaseOrderViolationDiscounts");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderViolationDiscounts_PurchaseOrderStatements_PurchaseOrderStatementId",
                table: "PurchaseOrderViolationDiscounts",
                column: "PurchaseOrderStatementId",
                principalTable: "PurchaseOrderStatements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderViolationDiscounts_PurchaseOrderStatements_PurchaseOrderStatementId",
                table: "PurchaseOrderViolationDiscounts");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderViolationDiscounts_PurchaseOrderStatements_PurchaseOrderStatementId",
                table: "PurchaseOrderViolationDiscounts",
                column: "PurchaseOrderStatementId",
                principalTable: "PurchaseOrderStatements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

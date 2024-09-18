using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBanHang.Migrations
{
    /// <inheritdoc />
    public partial class AddNewCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PaymentId",
                table: "OrderDetails",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_PaymentId",
                table: "OrderDetails",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Payments_PaymentId",
                table: "OrderDetails",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Payments_PaymentId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_PaymentId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "OrderDetails");
        }
    }
}

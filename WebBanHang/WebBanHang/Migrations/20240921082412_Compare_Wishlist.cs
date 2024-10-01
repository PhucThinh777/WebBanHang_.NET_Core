using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBanHang.Migrations
{
    public partial class Compare_Wishlist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tạo bảng Wishlists
            migrationBuilder.CreateTable(
                name: "Wishlists",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true) // hoặc kiểu dữ liệu tương ứng
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wishlists_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tạo chỉ mục cho cột ProductId trong bảng Wishlists
            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_ProductId",
                table: "Wishlists",
                column: "ProductId");

            // Tạo bảng Compares
            migrationBuilder.CreateTable(
                name: "Compares",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(nullable: false),
                    UserId = table.Column<string>(nullable: true) // hoặc kiểu dữ liệu tương ứng cho UserId
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compares_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tạo chỉ mục cho cột ProductId trong bảng Compares
            migrationBuilder.CreateIndex(
                name: "IX_Compares_ProductId",
                table: "Compares",
                column: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa bảng Compares khi rollback migration
            migrationBuilder.DropTable(
                name: "Compares");

            // Xóa bảng Wishlists khi rollback migration
            migrationBuilder.DropTable(
                name: "Wishlists");
        }
    }
}

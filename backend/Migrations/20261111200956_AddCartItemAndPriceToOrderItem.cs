using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItemAndPriceToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "t_order_item",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "t_cart_item",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_cart_item", x => new { x.user_id, x.item_id });
                    table.ForeignKey(
                        name: "FK_t_cart_item_t_menu_item_item_id",
                        column: x => x.item_id,
                        principalTable: "t_menu_item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_cart_item_t_user_user_id",
                        column: x => x.user_id,
                        principalTable: "t_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_cart_item_item_id",
                table: "t_cart_item",
                column: "item_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_cart_item");

            migrationBuilder.DropColumn(
                name: "price",
                table: "t_order_item");
        }
    }
}

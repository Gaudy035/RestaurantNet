using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "t_delivery");

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "t_order",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "category_id",
                table: "t_menu_item",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "t_category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_category", x => x.category_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_menu_item_category_id",
                table: "t_menu_item",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_t_menu_item_t_category_category_id",
                table: "t_menu_item",
                column: "category_id",
                principalTable: "t_category",
                principalColumn: "category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_menu_item_t_category_category_id",
                table: "t_menu_item");

            migrationBuilder.DropTable(
                name: "t_category");

            migrationBuilder.DropIndex(
                name: "IX_t_menu_item_category_id",
                table: "t_menu_item");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "t_order");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "t_menu_item");

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "t_delivery",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}

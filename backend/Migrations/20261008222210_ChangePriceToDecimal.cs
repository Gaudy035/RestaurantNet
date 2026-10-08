using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangePriceToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "t_order",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "t_menu_item",
                type: "numeric(7,2)",
                precision: 7,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "price",
                table: "t_order",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(8,2)",
                oldPrecision: 8,
                oldScale: 2);

            migrationBuilder.AlterColumn<double>(
                name: "price",
                table: "t_menu_item",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,2)",
                oldPrecision: 7,
                oldScale: 2);
        }
    }
}

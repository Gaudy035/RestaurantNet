using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureIndexesOnLocationEmployeeConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_t_location_employee",
                table: "t_location_employee");

            migrationBuilder.AlterColumn<string>(
                name: "position",
                table: "t_location_employee",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AddPrimaryKey(
                name: "PK_t_location_employee",
                table: "t_location_employee",
                columns: new[] { "user_id", "location_id", "position" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_t_location_employee",
                table: "t_location_employee");

            migrationBuilder.AlterColumn<string>(
                name: "position",
                table: "t_location_employee",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7);

            migrationBuilder.AddPrimaryKey(
                name: "PK_t_location_employee",
                table: "t_location_employee",
                columns: new[] { "user_id", "location_id" });
        }
    }
}

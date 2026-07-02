using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_ingredient",
                columns: table => new
                {
                    ingredient_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    allergen = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_ingredient", x => x.ingredient_id);
                });

            migrationBuilder.CreateTable(
                name: "t_location",
                columns: table => new
                {
                    location_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    city = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_location", x => x.location_id);
                });

            migrationBuilder.CreateTable(
                name: "t_menu_item",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_available = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_menu_item", x => x.item_id);
                });

            migrationBuilder.CreateTable(
                name: "t_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    last_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_user", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "t_table",
                columns: table => new
                {
                    table_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    location_id = table.Column<int>(type: "integer", nullable: false),
                    seats = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_table", x => x.table_id);
                    table.ForeignKey(
                        name: "FK_t_table_t_location_location_id",
                        column: x => x.location_id,
                        principalTable: "t_location",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_item_ingredient",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    ingredient_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_item_ingredient", x => new { x.ingredient_id, x.item_id });
                    table.ForeignKey(
                        name: "FK_t_item_ingredient_t_ingredient_ingredient_id",
                        column: x => x.ingredient_id,
                        principalTable: "t_ingredient",
                        principalColumn: "ingredient_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_item_ingredient_t_menu_item_item_id",
                        column: x => x.item_id,
                        principalTable: "t_menu_item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_client",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_client", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_t_client_t_user_user_id",
                        column: x => x.user_id,
                        principalTable: "t_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_employee",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_employee", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_t_employee_t_user_user_id",
                        column: x => x.user_id,
                        principalTable: "t_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_refresh_token",
                columns: table => new
                {
                    token_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    token_value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_refresh_token", x => x.token_id);
                    table.ForeignKey(
                        name: "FK_t_refresh_token_t_user_user_id",
                        column: x => x.user_id,
                        principalTable: "t_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_booking",
                columns: table => new
                {
                    booking_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    table_id = table.Column<int>(type: "integer", nullable: false),
                    client_id = table.Column<int>(type: "integer", nullable: true),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_booking", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK_t_booking_t_client_client_id",
                        column: x => x.client_id,
                        principalTable: "t_client",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_booking_t_table_table_id",
                        column: x => x.table_id,
                        principalTable: "t_table",
                        principalColumn: "table_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_order",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    location_id = table.Column<int>(type: "integer", nullable: false),
                    client_id = table.Column<int>(type: "integer", nullable: true),
                    order_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    order_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_order", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_t_order_t_client_client_id",
                        column: x => x.client_id,
                        principalTable: "t_client",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_order_t_location_location_id",
                        column: x => x.location_id,
                        principalTable: "t_location",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "t_location_employee",
                columns: table => new
                {
                    location_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_location_employee", x => new { x.user_id, x.location_id });
                    table.ForeignKey(
                        name: "FK_t_location_employee_t_employee_user_id",
                        column: x => x.user_id,
                        principalTable: "t_employee",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_location_employee_t_location_location_id",
                        column: x => x.location_id,
                        principalTable: "t_location",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_delivery",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    city = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_delivery", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_t_delivery_t_order_order_id",
                        column: x => x.order_id,
                        principalTable: "t_order",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_order_item",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_order_item", x => new { x.order_id, x.item_id });
                    table.ForeignKey(
                        name: "FK_t_order_item_t_menu_item_item_id",
                        column: x => x.item_id,
                        principalTable: "t_menu_item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_order_item_t_order_order_id",
                        column: x => x.order_id,
                        principalTable: "t_order",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_booking_client_id",
                table: "t_booking",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_booking_table_id",
                table: "t_booking",
                column: "table_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_item_ingredient_item_id",
                table: "t_item_ingredient",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_location_employee_location_id",
                table: "t_location_employee",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_order_client_id",
                table: "t_order",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_order_location_id",
                table: "t_order",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_order_item_item_id",
                table: "t_order_item",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_refresh_token_token_value",
                table: "t_refresh_token",
                column: "token_value",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_refresh_token_user_id",
                table: "t_refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_table_location_id",
                table: "t_table",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_user_email",
                table: "t_user",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_booking");

            migrationBuilder.DropTable(
                name: "t_delivery");

            migrationBuilder.DropTable(
                name: "t_item_ingredient");

            migrationBuilder.DropTable(
                name: "t_location_employee");

            migrationBuilder.DropTable(
                name: "t_order_item");

            migrationBuilder.DropTable(
                name: "t_refresh_token");

            migrationBuilder.DropTable(
                name: "t_table");

            migrationBuilder.DropTable(
                name: "t_ingredient");

            migrationBuilder.DropTable(
                name: "t_employee");

            migrationBuilder.DropTable(
                name: "t_menu_item");

            migrationBuilder.DropTable(
                name: "t_order");

            migrationBuilder.DropTable(
                name: "t_client");

            migrationBuilder.DropTable(
                name: "t_location");

            migrationBuilder.DropTable(
                name: "t_user");
        }
    }
}

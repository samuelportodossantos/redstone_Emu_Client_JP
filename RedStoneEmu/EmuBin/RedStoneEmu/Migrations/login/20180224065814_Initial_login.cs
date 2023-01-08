using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RedStoneEmu.Migrations.login
{
    public partial class Initial_login : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_server_info",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    enable = table.Column<bool>(nullable: false),
                    global_ip = table.Column<string>(nullable: true),
                    local_ip = table.Column<string>(nullable: true),
                    server_id = table.Column<int>(nullable: false),
                    server_name = table.Column<string>(nullable: true),
                    server_type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_server_info", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "login_log",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    datetime = table.Column<DateTime>(nullable: false),
                    ip_address = table.Column<string>(nullable: true),
                    mac_address = table.Column<string>(nullable: true),
                    username = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_log", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_server_info_local_ip",
                table: "game_server_info",
                column: "local_ip",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_server_info_server_id",
                table: "game_server_info",
                column: "server_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_server_info_server_name",
                table: "game_server_info",
                column: "server_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_login_log_datetime",
                table: "login_log",
                column: "datetime",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_server_info");

            migrationBuilder.DropTable(
                name: "login_log");
        }
    }
}

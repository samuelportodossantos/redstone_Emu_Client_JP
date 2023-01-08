using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RedStoneEmu.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    UniqueID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Endurance = table.Column<byte>(nullable: false),
                    ItemIndex = table.Column<short>(nullable: false),
                    Number = table.Column<byte>(nullable: false),
                    StackableFlag = table.Column<byte>(nullable: false),
                    Values = table.Column<byte[]>(nullable: true),
                    OPs = table.Column<byte[]>(nullable: true),
                    unk_bool1 = table.Column<bool>(nullable: false),
                    unk_bool2 = table.Column<bool>(nullable: false),
                    unk_bool3 = table.Column<bool>(nullable: false),
                    unk_flag1 = table.Column<byte>(nullable: false),
                    unk_flag2 = table.Column<byte>(nullable: false),
                    unk_flag3 = table.Column<byte>(nullable: false),
                    unk_flag4 = table.Column<byte>(nullable: false),
                    unk_flag5 = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.UniqueID);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    BaseCP = table.Column<uint>(nullable: false),
                    BaseHP = table.Column<uint>(nullable: false),
                    BeltItem = table.Column<string>(nullable: true),
                    CAResistance = table.Column<string>(nullable: true),
                    DeathPenarty = table.Column<int>(nullable: false),
                    Defence = table.Column<short>(nullable: false),
                    Direct = table.Column<short>(nullable: false),
                    EXP = table.Column<uint>(nullable: false),
                    EquipmentItem = table.Column<string>(nullable: true),
                    GMLevel = table.Column<int>(nullable: false),
                    Gold = table.Column<uint>(nullable: false),
                    GuildIndex = table.Column<short>(nullable: false),
                    InventoryItem = table.Column<string>(nullable: true),
                    IsRun = table.Column<bool>(nullable: false),
                    Job = table.Column<short>(nullable: false),
                    LevelHPCPBobuns = table.Column<short>(nullable: false),
                    Level = table.Column<short>(nullable: false),
                    MapSerial = table.Column<short>(nullable: false),
                    MaxPower = table.Column<short>(nullable: false),
                    MinPower = table.Column<short>(nullable: false),
                    MiniPet1 = table.Column<byte>(nullable: false),
                    MiniPet2 = table.Column<byte>(nullable: false),
                    MResistance = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    NowCP = table.Column<int>(nullable: false),
                    NowHP = table.Column<uint>(nullable: false),
                    PosX = table.Column<short>(nullable: false),
                    PosY = table.Column<short>(nullable: false),
                    RebornNumber = table.Column<byte>(nullable: false),
                    SkillPoint = table.Column<uint>(nullable: false),
                    StateHPCPBonus = table.Column<short>(nullable: false),
                    StatusPoint = table.Column<uint>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    Tendency = table.Column<short>(nullable: false),
                    UserID = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_Name",
                table: "Players",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}

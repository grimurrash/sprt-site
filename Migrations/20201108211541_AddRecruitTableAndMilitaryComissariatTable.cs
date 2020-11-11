using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NewSprt.Migrations
{
    public partial class AddRecruitTableAndMilitaryComissariatTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MilitaryComissariats",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    InnerCode = table.Column<string>(nullable: true),
                    ShortName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MilitaryComissariats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recruits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ConscriptionPeriodId = table.Column<int>(nullable: false),
                    RecruitId = table.Column<int>(nullable: false),
                    UniqueRecruitNumber = table.Column<string>(nullable: true),
                    DeliveryDate = table.Column<DateTime>(nullable: false),
                    LastName = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    Patronymic = table.Column<string>(nullable: true),
                    MilitaryComissariatCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recruits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recruits_ConscriptionPeriods_ConscriptionPeriodId",
                        column: x => x.ConscriptionPeriodId,
                        principalTable: "ConscriptionPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recruits_MilitaryComissariats_MilitaryComissariatCode",
                        column: x => x.MilitaryComissariatCode,
                        principalTable: "MilitaryComissariats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recruits_ConscriptionPeriodId",
                table: "Recruits",
                column: "ConscriptionPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Recruits_MilitaryComissariatCode",
                table: "Recruits",
                column: "MilitaryComissariatCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recruits");

            migrationBuilder.DropTable(
                name: "MilitaryComissariats");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NewSprt.Migrations
{
    public partial class AddLastEventInDismissalsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastEventCode",
                table: "Dismissals",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEventDate",
                table: "Dismissals",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastEventCode",
                table: "Dismissals");

            migrationBuilder.DropColumn(
                name: "LastEventDate",
                table: "Dismissals");
        }
    }
}

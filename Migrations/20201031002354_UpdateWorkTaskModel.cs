using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NewSprt.Migrations
{
    public partial class UpdateWorkTaskModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkTasks_WorkTaskStatuses_StatusId",
                table: "WorkTasks");

            migrationBuilder.DropTable(
                name: "WorkTaskStatuses");

            migrationBuilder.DropIndex(
                name: "IX_WorkTasks_StatusId",
                table: "WorkTasks");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "WorkTasks");

            migrationBuilder.RenameColumn(
                name: "TimelineForCompliance",
                table: "WorkTasks",
                newName: "AdditionToDeadlines");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchive",
                table: "WorkTasks",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchive",
                table: "WorkTasks");

            migrationBuilder.RenameColumn(
                name: "AdditionToDeadlines",
                table: "WorkTasks",
                newName: "TimelineForCompliance");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "WorkTasks",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WorkTaskStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTaskStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTasks_StatusId",
                table: "WorkTasks",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTasks_WorkTaskStatuses_StatusId",
                table: "WorkTasks",
                column: "StatusId",
                principalTable: "WorkTaskStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NewSprt.Migrations
{
    public partial class AddDactyloscopyStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DactyloscopyStatusId",
                table: "Recruits",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "DactyloscopyStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DactyloscopyStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recruits_DactyloscopyStatusId",
                table: "Recruits",
                column: "DactyloscopyStatusId");
            migrationBuilder.Sql("INSERT INTO public.\"DactyloscopyStatuses\"(\"Id\", \"Name\") VALUES (1, 'Не отобран')");
            migrationBuilder.Sql("INSERT INTO public.\"DactyloscopyStatuses\"(\"Id\", \"Name\") VALUES (2, 'Отработан')");
            migrationBuilder.Sql("INSERT INTO public.\"DactyloscopyStatuses\"(\"Id\", \"Name\") VALUES (3, 'Отправлен ранее')");
            migrationBuilder.AddForeignKey(
                name: "FK_Recruits_DactyloscopyStatuses_DactyloscopyStatusId",
                table: "Recruits",
                column: "DactyloscopyStatusId",
                principalTable: "DactyloscopyStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recruits_DactyloscopyStatuses_DactyloscopyStatusId",
                table: "Recruits");

            migrationBuilder.DropTable(
                name: "DactyloscopyStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Recruits_DactyloscopyStatusId",
                table: "Recruits");

            migrationBuilder.DropColumn(
                name: "DactyloscopyStatusId",
                table: "Recruits");
        }
    }
}

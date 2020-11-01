using Microsoft.EntityFrameworkCore.Migrations;

namespace NewSprt.Migrations
{
    public partial class UpdateUserModelAndWorkTaskModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkTasks_Users_ManagerId",
                table: "WorkTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkTasks_Users_ResponsibleId",
                table: "WorkTasks");

            migrationBuilder.RenameColumn(
                name: "ResponsibleId",
                table: "WorkTasks",
                newName: "TaskResponsibleId");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "WorkTasks",
                newName: "TaskManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkTasks_ResponsibleId",
                table: "WorkTasks",
                newName: "IX_WorkTasks_TaskResponsibleId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkTasks_ManagerId",
                table: "WorkTasks",
                newName: "IX_WorkTasks_TaskManagerId");

            migrationBuilder.RenameColumn(
                name: "Fio",
                table: "Users",
                newName: "FullName");

            migrationBuilder.AddColumn<string>(
                name: "AuthorizationToken",
                table: "Users",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTasks_Users_TaskManagerId",
                table: "WorkTasks",
                column: "TaskManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTasks_Users_TaskResponsibleId",
                table: "WorkTasks",
                column: "TaskResponsibleId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkTasks_Users_TaskManagerId",
                table: "WorkTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkTasks_Users_TaskResponsibleId",
                table: "WorkTasks");

            migrationBuilder.DropColumn(
                name: "AuthorizationToken",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "TaskResponsibleId",
                table: "WorkTasks",
                newName: "ResponsibleId");

            migrationBuilder.RenameColumn(
                name: "TaskManagerId",
                table: "WorkTasks",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkTasks_TaskResponsibleId",
                table: "WorkTasks",
                newName: "IX_WorkTasks_ResponsibleId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkTasks_TaskManagerId",
                table: "WorkTasks",
                newName: "IX_WorkTasks_ManagerId");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Users",
                newName: "Fio");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTasks_Users_ManagerId",
                table: "WorkTasks",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTasks_Users_ResponsibleId",
                table: "WorkTasks",
                column: "ResponsibleId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platforma_pentru_tranzactii_auto.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commentarii_Anunt_ID_Anunt",
                table: "Commentarii");

            migrationBuilder.DropForeignKey(
                name: "FK_Commentarii_AspNetUsers_UserId",
                table: "Commentarii");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Commentarii",
                table: "Commentarii");

            migrationBuilder.RenameTable(
                name: "Commentarii",
                newName: "Comentarii");

            migrationBuilder.RenameIndex(
                name: "IX_Commentarii_UserId",
                table: "Comentarii",
                newName: "IX_Comentarii_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Commentarii_ID_Anunt",
                table: "Comentarii",
                newName: "IX_Comentarii_ID_Anunt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Comentarii",
                table: "Comentarii",
                column: "ID_Comentariu");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarii_Anunt_ID_Anunt",
                table: "Comentarii",
                column: "ID_Anunt",
                principalTable: "Anunt",
                principalColumn: "ID_Anunt",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarii_AspNetUsers_UserId",
                table: "Comentarii",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarii_Anunt_ID_Anunt",
                table: "Comentarii");

            migrationBuilder.DropForeignKey(
                name: "FK_Comentarii_AspNetUsers_UserId",
                table: "Comentarii");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Comentarii",
                table: "Comentarii");

            migrationBuilder.RenameTable(
                name: "Comentarii",
                newName: "Commentarii");

            migrationBuilder.RenameIndex(
                name: "IX_Comentarii_UserId",
                table: "Commentarii",
                newName: "IX_Commentarii_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Comentarii_ID_Anunt",
                table: "Commentarii",
                newName: "IX_Commentarii_ID_Anunt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Commentarii",
                table: "Commentarii",
                column: "ID_Comentariu");

            migrationBuilder.AddForeignKey(
                name: "FK_Commentarii_Anunt_ID_Anunt",
                table: "Commentarii",
                column: "ID_Anunt",
                principalTable: "Anunt",
                principalColumn: "ID_Anunt",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Commentarii_AspNetUsers_UserId",
                table: "Commentarii",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

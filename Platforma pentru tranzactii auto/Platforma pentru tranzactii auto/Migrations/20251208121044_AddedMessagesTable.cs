using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Platforma_pentru_tranzactii_auto.Migrations
{
    /// <inheritdoc />
    public partial class AddedMessagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mesaje",
                columns: table => new
                {
                    ID_Mesaj = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Continut = table.Column<string>(type: "text", nullable: false),
                    DataTrimiterii = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpeditorId = table.Column<int>(type: "integer", nullable: false),
                    DestinatarId = table.Column<int>(type: "integer", nullable: false),
                    ID_Anunt = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesaje", x => x.ID_Mesaj);
                    table.ForeignKey(
                        name: "FK_Mesaje_Anunt_ID_Anunt",
                        column: x => x.ID_Anunt,
                        principalTable: "Anunt",
                        principalColumn: "ID_Anunt",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mesaje_AspNetUsers_DestinatarId",
                        column: x => x.DestinatarId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mesaje_AspNetUsers_ExpeditorId",
                        column: x => x.ExpeditorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mesaje_DestinatarId",
                table: "Mesaje",
                column: "DestinatarId");

            migrationBuilder.CreateIndex(
                name: "IX_Mesaje_ExpeditorId",
                table: "Mesaje",
                column: "ExpeditorId");

            migrationBuilder.CreateIndex(
                name: "IX_Mesaje_ID_Anunt",
                table: "Mesaje",
                column: "ID_Anunt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mesaje");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Platforma_pentru_tranzactii_auto.Migrations
{
    /// <inheritdoc />
    public partial class AddedImageGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImaginiAnunt",
                columns: table => new
                {
                    ID_Imagine = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Imagine = table.Column<byte[]>(type: "bytea", nullable: false),
                    ID_Anunt = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImaginiAnunt", x => x.ID_Imagine);
                    table.ForeignKey(
                        name: "FK_ImaginiAnunt_Anunt_ID_Anunt",
                        column: x => x.ID_Anunt,
                        principalTable: "Anunt",
                        principalColumn: "ID_Anunt",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImaginiAnunt_ID_Anunt",
                table: "ImaginiAnunt",
                column: "ID_Anunt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImaginiAnunt");
        }
    }
}

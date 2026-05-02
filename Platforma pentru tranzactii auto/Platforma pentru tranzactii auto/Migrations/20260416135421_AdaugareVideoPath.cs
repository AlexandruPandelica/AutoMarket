using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platforma_pentru_tranzactii_auto.Migrations
{
    /// <inheritdoc />
    public partial class AdaugareVideoPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoPath",
                table: "Anunt",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoPath",
                table: "Anunt");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlyBurger.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class kreiranjestudenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Index = table.Column<string>(type: "TEXT", maxLength: 9, nullable: false),
                    Ime = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Prezime = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Index);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}

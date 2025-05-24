using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusLostAndFound.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelWithDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Items");
        }
    }
}

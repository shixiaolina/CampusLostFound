using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusLostAndFound.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimFieldsToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClaimDate",
                table: "Items",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimReason",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimerContact",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimerId",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimerName",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClaimed",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaimDate",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ClaimReason",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ClaimerContact",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ClaimerId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ClaimerName",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "IsClaimed",
                table: "Items");
        }
    }
}

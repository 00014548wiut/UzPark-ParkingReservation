using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingReservation.Migrations
{
    /// <inheritdoc />
    public partial class initialCreatedOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedId",
                table: "Locations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedId",
                table: "Locations");
        }
    }
}

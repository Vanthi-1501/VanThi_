using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VanThi_.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingServiceAndRoomTypeImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StayId1",
                table: "ServiceOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "RoomTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingServices_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrders_StayId1",
                table: "ServiceOrders",
                column: "StayId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookingServices_BookingId",
                table: "BookingServices",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingServices_ServiceId",
                table: "BookingServices",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrders_Stays_StayId1",
                table: "ServiceOrders",
                column: "StayId1",
                principalTable: "Stays",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrders_Stays_StayId1",
                table: "ServiceOrders");

            migrationBuilder.DropTable(
                name: "BookingServices");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrders_StayId1",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "StayId1",
                table: "ServiceOrders");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "RoomTypes");
        }
    }
}

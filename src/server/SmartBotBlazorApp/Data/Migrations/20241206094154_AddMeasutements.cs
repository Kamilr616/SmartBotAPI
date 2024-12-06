using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBotBlazorApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasutements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RobotId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemperatureC = table.Column<double>(type: "float", nullable: false),
                    AccelerationX = table.Column<double>(type: "float", nullable: false),
                    AccelerationY = table.Column<double>(type: "float", nullable: false),
                    AccelerationZ = table.Column<double>(type: "float", nullable: false),
                    RotationX = table.Column<double>(type: "float", nullable: false),
                    RotationY = table.Column<double>(type: "float", nullable: false),
                    RotationZ = table.Column<double>(type: "float", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AvgDistance = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Measurements");
        }
    }
}

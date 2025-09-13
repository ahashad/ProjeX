using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeX.Infrastructure.Migrations
{
    public partial class AddAssignmentDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "ActualAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "ActualAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActualAssignment_Employee_DateRange",
                table: "ActualAssignments",
                columns: new[] { "EmployeeId", "StartDate", "EndDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ActualAssignment_Employee_DateRange",
                table: "ActualAssignments");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "ActualAssignments");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "ActualAssignments");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseFieldsToTeamPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PlannedHoteling",
                table: "PlannedTeamSlots",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlannedOthers",
                table: "PlannedTeamSlots",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlannedTickets",
                table: "PlannedTeamSlots",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotHoteling",
                table: "ActualAssignments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotOthers",
                table: "ActualAssignments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotTickets",
                table: "ActualAssignments",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedHoteling",
                table: "PlannedTeamSlots");

            migrationBuilder.DropColumn(
                name: "PlannedOthers",
                table: "PlannedTeamSlots");

            migrationBuilder.DropColumn(
                name: "PlannedTickets",
                table: "PlannedTeamSlots");

            migrationBuilder.DropColumn(
                name: "SnapshotHoteling",
                table: "ActualAssignments");

            migrationBuilder.DropColumn(
                name: "SnapshotOthers",
                table: "ActualAssignments");

            migrationBuilder.DropColumn(
                name: "SnapshotTickets",
                table: "ActualAssignments");
        }
    }
}

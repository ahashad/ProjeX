using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalarySnapshotToActualAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotCommissionPercent",
                table: "ActualAssignments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotMonthlyIncentive",
                table: "ActualAssignments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SnapshotSalary",
                table: "ActualAssignments",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SnapshotCommissionPercent",
                table: "ActualAssignments");

            migrationBuilder.DropColumn(
                name: "SnapshotMonthlyIncentive",
                table: "ActualAssignments");

            migrationBuilder.DropColumn(
                name: "SnapshotSalary",
                table: "ActualAssignments");
        }
    }
}

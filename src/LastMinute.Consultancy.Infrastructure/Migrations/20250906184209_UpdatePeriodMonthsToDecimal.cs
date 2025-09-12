using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastMinute.Consultancy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePeriodMonthsToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PeriodMonths",
                table: "PlannedTeamSlots",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PeriodMonths",
                table: "PlannedTeamSlots",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}

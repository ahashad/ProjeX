using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjeX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingProjectColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid?>(
                name: "AccountId", 
                table: "Projects", 
                type: "uniqueidentifier", 
                nullable: true);

            migrationBuilder.AddColumn<DateTime?>(
                name: "ActualEndDate", 
                table: "Projects", 
                type: "datetime2", 
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualMargin", 
                table: "Projects", 
                type: "decimal(18,2)", 
                nullable: false, 
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime?>(
                name: "ApprovedAt", 
                table: "Projects", 
                type: "datetime2", 
                nullable: true);

            migrationBuilder.AddColumn<Guid?>(
                name: "ApprovedById", 
                table: "Projects", 
                type: "uniqueidentifier", 
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ContractValue", 
                table: "Projects", 
                type: "decimal(18,2)", 
                nullable: false, 
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency", 
                table: "Projects", 
                type: "nvarchar(max)", 
                nullable: false, 
                defaultValue: "USD");

            migrationBuilder.AddColumn<string>(
                name: "Description", 
                table: "Projects", 
                type: "nvarchar(max)", 
                nullable: false, 
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved", 
                table: "Projects", 
                type: "bit", 
                nullable: false, 
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTerms", 
                table: "Projects", 
                type: "nvarchar(max)", 
                nullable: false, 
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PlannedMargin", 
                table: "Projects", 
                type: "decimal(18,2)", 
                nullable: false, 
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid?>(
                name: "ProjectManagerId", 
                table: "Projects", 
                type: "uniqueidentifier", 
                nullable: true);

            // Add indexes only for existing tables (Employees)
            migrationBuilder.CreateIndex(
                name: "IX_Projects_ApprovedById",
                table: "Projects",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectManagerId",
                table: "Projects",
                column: "ProjectManagerId");

            // Add foreign key constraints only for existing tables
            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Employees_ApprovedById",
                table: "Projects",
                column: "ApprovedById",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Employees_ProjectManagerId",
                table: "Projects",
                column: "ProjectManagerId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Employees_ApprovedById",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Employees_ProjectManagerId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ApprovedById",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ProjectManagerId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ActualEndDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ActualMargin",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ContractValue",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PaymentTerms",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PlannedMargin",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectManagerId",
                table: "Projects");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrPuSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_TotalLeavesDiscarded_Emp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalLeavesDiscarded",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalLeavesDiscarded",
                table: "Employees");
        }
    }
}

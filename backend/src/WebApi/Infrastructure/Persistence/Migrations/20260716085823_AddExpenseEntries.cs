using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_expense_reports_Id_Year_Month",
                table: "expense_reports",
                columns: new[] { "Id", "Year", "Month" });

            migrationBuilder.CreateTable(
                name: "expense_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpenseReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportYear = table.Column<int>(type: "integer", nullable: false),
                    ReportMonth = table.Column<int>(type: "integer", nullable: false),
                    ExpenseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    BillingMerchantName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BillingStreet = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BillingPostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BillingCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_entries", x => x.Id);
                    table.CheckConstraint("CK_expense_entries_amount_positive", "\"Amount\" > 0");
                    table.CheckConstraint("CK_expense_entries_currency_eur", "\"Currency\" = 'EUR'");
                    table.CheckConstraint(
                        "CK_expense_entries_expense_date_report_month",
                        "EXTRACT(YEAR FROM \"ExpenseDate\") = \"ReportYear\" " +
                        "AND EXTRACT(MONTH FROM \"ExpenseDate\") = \"ReportMonth\"");
                    table.CheckConstraint("CK_expense_entries_report_month_valid", "\"ReportMonth\" BETWEEN 1 AND 12");
                    table.ForeignKey(
                        name: "FK_expense_entries_expense_reports_period",
                        columns: x => new { x.ExpenseReportId, x.ReportYear, x.ReportMonth },
                        principalTable: "expense_reports",
                        principalColumns: new[] { "Id", "Year", "Month" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_expense_entries_ExpenseDate",
                table: "expense_entries",
                column: "ExpenseDate");

            migrationBuilder.CreateIndex(
                name: "IX_expense_entries_ExpenseReportId",
                table: "expense_entries",
                column: "ExpenseReportId");

            migrationBuilder.CreateIndex(
                name: "IX_expense_entries_ExpenseReportId_IsDeleted",
                table: "expense_entries",
                columns: new[] { "ExpenseReportId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_expense_entries_ExpenseReportId_IsDeleted_ExpenseDate",
                table: "expense_entries",
                columns: new[] { "ExpenseReportId", "IsDeleted", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_expense_entries_ExpenseReportId_ReportYear_ReportMonth",
                table: "expense_entries",
                columns: new[] { "ExpenseReportId", "ReportYear", "ReportMonth" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expense_entries");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_expense_reports_Id_Year_Month",
                table: "expense_reports");
        }
    }
}

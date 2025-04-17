using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarBlog.Data.Migrations
{
    public partial class MoreVisitStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "visit_record",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "visit_record",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "isp",
                table: "visit_record",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "province",
                table: "visit_record",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "referrer",
                table: "visit_record",
                type: "TEXT",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "region_code",
                table: "visit_record",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "response_time_ms",
                table: "visit_record",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "status_code",
                table: "visit_record",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "idx_visit_city",
                table: "visit_record",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "idx_visit_country",
                table: "visit_record",
                column: "country");

            migrationBuilder.CreateIndex(
                name: "idx_visit_isp",
                table: "visit_record",
                column: "isp");

            migrationBuilder.CreateIndex(
                name: "idx_visit_path",
                table: "visit_record",
                column: "request_path");

            migrationBuilder.CreateIndex(
                name: "idx_visit_province",
                table: "visit_record",
                column: "province");

            migrationBuilder.CreateIndex(
                name: "idx_visit_region_code",
                table: "visit_record",
                column: "region_code");

            migrationBuilder.CreateIndex(
                name: "idx_visit_status",
                table: "visit_record",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "idx_visit_time",
                table: "visit_record",
                column: "time");

            migrationBuilder.CreateIndex(
                name: "idx_visit_time_status",
                table: "visit_record",
                columns: new[] { "time", "status_code" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_visit_city",
                table: "visit_record");

            migrationBuilder.DropIndex(
                name: "idx_visit_country",
                table: "visit_record");

            migrationBuilder.DropIndex(
                name: "idx_visit_isp",
                table: "visit_record");

            migrationBuilder.DropIndex(
                name: "idx_visit_path",
                table: "visit_record");

            migrationBuilder.DropIndex(
                name: "idx_visit_province",
                table: "visit_record");

            migrationBuilder.DropIndex(
                name: "idx_visit_region_code",
                table: "visit_record");

            migrationBuilder.DropIndex(
                name: "idx_visit_status",
                table: "visit_record");

            migrationBuilder.DropIndex(
                name: "idx_visit_time",
                table: "visit_record");

            migrationBuilder.DropIndex(
                name: "idx_visit_time_status",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "city",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "country",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "isp",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "province",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "referrer",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "region_code",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "response_time_ms",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "status_code",
                table: "visit_record");
        }
    }
}

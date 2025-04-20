using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarBlog.Data.Migrations
{
    public partial class AddIpInfoValueObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "idx_visit_province",
                table: "visit_record");

            migrationBuilder.DropIndex(
                name: "idx_visit_region_code",
                table: "visit_record");

            migrationBuilder.RenameColumn(
                name: "region_code",
                table: "visit_record",
                newName: "ip_info_region_code");

            migrationBuilder.RenameColumn(
                name: "province",
                table: "visit_record",
                newName: "ip_info_province");

            migrationBuilder.RenameColumn(
                name: "isp",
                table: "visit_record",
                newName: "ip_info_isp");

            migrationBuilder.RenameColumn(
                name: "country",
                table: "visit_record",
                newName: "ip_info_country");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "visit_record",
                newName: "ip_info_city");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ip_info_region_code",
                table: "visit_record",
                newName: "region_code");

            migrationBuilder.RenameColumn(
                name: "ip_info_province",
                table: "visit_record",
                newName: "province");

            migrationBuilder.RenameColumn(
                name: "ip_info_isp",
                table: "visit_record",
                newName: "isp");

            migrationBuilder.RenameColumn(
                name: "ip_info_country",
                table: "visit_record",
                newName: "country");

            migrationBuilder.RenameColumn(
                name: "ip_info_city",
                table: "visit_record",
                newName: "city");

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
                name: "idx_visit_province",
                table: "visit_record",
                column: "province");

            migrationBuilder.CreateIndex(
                name: "idx_visit_region_code",
                table: "visit_record",
                column: "region_code");
        }
    }
}

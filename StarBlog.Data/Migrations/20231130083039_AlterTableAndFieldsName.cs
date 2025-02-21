using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarBlog.Data.Migrations
{
    public partial class AlterTableAndFieldsName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VisitRecords",
                table: "VisitRecords");

            migrationBuilder.RenameTable(
                name: "VisitRecords",
                newName: "visit_record");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "visit_record",
                newName: "time");

            migrationBuilder.RenameColumn(
                name: "Ip",
                table: "visit_record",
                newName: "ip");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "visit_record",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserAgent",
                table: "visit_record",
                newName: "user_agent");

            migrationBuilder.RenameColumn(
                name: "RequestQueryString",
                table: "visit_record",
                newName: "request_query_string");

            migrationBuilder.RenameColumn(
                name: "RequestPath",
                table: "visit_record",
                newName: "request_path");

            migrationBuilder.RenameColumn(
                name: "RequestMethod",
                table: "visit_record",
                newName: "request_method");

            migrationBuilder.AddPrimaryKey(
                name: "pk_visit_record",
                table: "visit_record",
                column: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_visit_record",
                table: "visit_record");

            migrationBuilder.RenameTable(
                name: "visit_record",
                newName: "VisitRecords");

            migrationBuilder.RenameColumn(
                name: "time",
                table: "VisitRecords",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "ip",
                table: "VisitRecords",
                newName: "Ip");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "VisitRecords",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_agent",
                table: "VisitRecords",
                newName: "UserAgent");

            migrationBuilder.RenameColumn(
                name: "request_query_string",
                table: "VisitRecords",
                newName: "RequestQueryString");

            migrationBuilder.RenameColumn(
                name: "request_path",
                table: "VisitRecords",
                newName: "RequestPath");

            migrationBuilder.RenameColumn(
                name: "request_method",
                table: "VisitRecords",
                newName: "RequestMethod");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VisitRecords",
                table: "VisitRecords",
                column: "Id");
        }
    }
}

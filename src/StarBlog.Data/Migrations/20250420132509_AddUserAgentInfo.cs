using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarBlog.Data.Migrations
{
    public partial class AddUserAgentInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_device_brand",
                table: "visit_record",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_device_family",
                table: "visit_record",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "user_agent_info_device_is_spider",
                table: "visit_record",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_device_model",
                table: "visit_record",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_os_family",
                table: "visit_record",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_os_major",
                table: "visit_record",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_os_minor",
                table: "visit_record",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_os_patch",
                table: "visit_record",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_os_patch_minor",
                table: "visit_record",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_user_agent_family",
                table: "visit_record",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_user_agent_major",
                table: "visit_record",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_user_agent_minor",
                table: "visit_record",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_agent_info_user_agent_patch",
                table: "visit_record",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_agent_info_device_brand",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_device_family",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_device_is_spider",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_device_model",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_os_family",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_os_major",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_os_minor",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_os_patch",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_os_patch_minor",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_user_agent_family",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_user_agent_major",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_user_agent_minor",
                table: "visit_record");

            migrationBuilder.DropColumn(
                name: "user_agent_info_user_agent_patch",
                table: "visit_record");
        }
    }
}

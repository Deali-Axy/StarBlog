using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarBlog.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ArticleIllustrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cover_fit",
                table: "post",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                defaultValue: "contain");

            migrationBuilder.AddColumn<bool>(
                name: "cover_hidden",
                table: "post",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "cover_url",
                table: "post",
                type: "TEXT",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cover_fit",
                table: "post");

            migrationBuilder.DropColumn(
                name: "cover_hidden",
                table: "post");

            migrationBuilder.DropColumn(
                name: "cover_url",
                table: "post");
        }
    }
}

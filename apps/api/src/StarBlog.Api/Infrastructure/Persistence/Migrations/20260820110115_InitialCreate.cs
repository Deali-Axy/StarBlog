using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarBlog.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "anonymous_user",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    url = table.Column<string>(type: "TEXT", nullable: true),
                    ip = table.Column<string>(type: "TEXT", nullable: true),
                    created_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_anonymous_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    parent_id = table.Column<int>(type: "INTEGER", nullable: false),
                    visible = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "config",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    key = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    value = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_config", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "link",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    url = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    visible = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_link", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "link_exchange",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    url = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    web_master = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    verified = table.Column<bool>(type: "INTEGER", nullable: false),
                    reason = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    apply_time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_link_exchange", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    type = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    dedup_key = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    payload = table.Column<string>(type: "TEXT", nullable: false),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    attempt = table.Column<int>(type: "INTEGER", nullable: false),
                    max_attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    next_attempt_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    locked_until = table.Column<DateTime>(type: "TEXT", nullable: true),
                    locked_by = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    last_error = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_message", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "photo",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    location = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    file_path = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    height = table.Column<long>(type: "INTEGER", nullable: false),
                    width = table.Column<long>(type: "INTEGER", nullable: false),
                    create_time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_photo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "post_publication",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    post_id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    channel_id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    platform = table.Column<int>(type: "INTEGER", nullable: false),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    rendered_content = table.Column<string>(type: "TEXT", nullable: false),
                    cover_url = table.Column<string>(type: "TEXT", nullable: true),
                    external_id = table.Column<string>(type: "TEXT", nullable: true),
                    external_url = table.Column<string>(type: "TEXT", nullable: true),
                    error_message = table.Column<string>(type: "TEXT", nullable: true),
                    creation_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    published_time = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_publication", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "publication_channel",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    platform = table.Column<int>(type: "INTEGER", nullable: false),
                    enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    app_id = table.Column<string>(type: "TEXT", nullable: true),
                    app_secret = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    author = table.Column<string>(type: "TEXT", nullable: true),
                    theme = table.Column<string>(type: "TEXT", nullable: true),
                    publish_url = table.Column<string>(type: "TEXT", nullable: true),
                    creation_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_publication_channel", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    password = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "visit_record",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ip = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ip_info_region_code = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ip_info_country = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ip_info_province = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ip_info_city = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ip_info_isp = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    request_path = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    request_query_string = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    request_method = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    user_agent = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    user_agent_info_os_family = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    user_agent_info_os_major = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    user_agent_info_os_minor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    user_agent_info_os_patch = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    user_agent_info_os_patch_minor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    user_agent_info_device_brand = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    user_agent_info_device_family = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    user_agent_info_device_model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    user_agent_info_device_is_spider = table.Column<bool>(type: "INTEGER", nullable: false),
                    user_agent_info_user_agent_family = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    user_agent_info_user_agent_major = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    user_agent_info_user_agent_minor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    user_agent_info_user_agent_patch = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    status_code = table.Column<int>(type: "INTEGER", nullable: false),
                    response_time_ms = table.Column<int>(type: "INTEGER", nullable: false),
                    referrer = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visit_record", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comment",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    parent_id = table.Column<string>(type: "TEXT", nullable: true),
                    post_id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    anonymous_user_id = table.Column<string>(type: "TEXT", nullable: true),
                    user_agent = table.Column<string>(type: "TEXT", nullable: true),
                    content = table.Column<string>(type: "TEXT", nullable: false),
                    visible = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_need_audit = table.Column<bool>(type: "INTEGER", nullable: false),
                    reason = table.Column<string>(type: "TEXT", nullable: true),
                    created_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comment", x => x.id);
                    table.ForeignKey(
                        name: "fk_comment_anonymous_user_anonymous_user_id",
                        column: x => x.anonymous_user_id,
                        principalTable: "anonymous_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_comment_comment_parent_id",
                        column: x => x.parent_id,
                        principalTable: "comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "featured_category",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    category_id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    icon_css_class = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_featured_category", x => x.id);
                    table.ForeignKey(
                        name: "fk_featured_category_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    slug = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    is_publish = table.Column<bool>(type: "INTEGER", nullable: false),
                    summary = table.Column<string>(type: "TEXT", nullable: true),
                    content = table.Column<string>(type: "TEXT", nullable: true),
                    path = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    creation_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    category_id = table.Column<int>(type: "INTEGER", nullable: false),
                    categories = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "featured_photo",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    photo_id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_featured_photo", x => x.id);
                    table.ForeignKey(
                        name: "fk_featured_photo_photos_photo_id",
                        column: x => x.photo_id,
                        principalTable: "photo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "featured_post",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    post_id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_featured_post", x => x.id);
                    table.ForeignKey(
                        name: "fk_featured_post_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_translation",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    post_id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    language = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    summary = table.Column<string>(type: "TEXT", nullable: true),
                    content = table.Column<string>(type: "TEXT", nullable: true),
                    creation_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    last_update_time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_translation", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_translation_post_post_id",
                        column: x => x.post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "top_post",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    post_id = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_top_post", x => x.id);
                    table.ForeignKey(
                        name: "fk_top_post_post_post_id",
                        column: x => x.post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_anonymous_user_email",
                table: "anonymous_user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_comment_anonymous_user_id",
                table: "comment",
                column: "anonymous_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_parent_id",
                table: "comment",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_post_id",
                table: "comment",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_config_key",
                table: "config",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_featured_category_category_id",
                table: "featured_category",
                column: "category_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_featured_photo_photo_id",
                table: "featured_photo",
                column: "photo_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_featured_post_post_id",
                table: "featured_post",
                column: "post_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_link_exchange_url",
                table: "link_exchange",
                column: "url");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_dedup_key",
                table: "outbox_message",
                column: "dedup_key",
                unique: true,
                filter: "dedup_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_status_next_attempt_at",
                table: "outbox_message",
                columns: new[] { "status", "next_attempt_at" });

            migrationBuilder.CreateIndex(
                name: "ix_post_category_id",
                table: "post",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_is_publish",
                table: "post",
                column: "is_publish");

            migrationBuilder.CreateIndex(
                name: "ix_post_slug",
                table: "post",
                column: "slug",
                unique: true,
                filter: "slug IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_post_publication_post_id",
                table: "post_publication",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_translation_post_id_language",
                table: "post_translation",
                columns: new[] { "post_id", "language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_top_post_post_id",
                table: "top_post",
                column: "post_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_name",
                table: "user",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_visit_path",
                table: "visit_record",
                column: "request_path");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comment");

            migrationBuilder.DropTable(
                name: "config");

            migrationBuilder.DropTable(
                name: "featured_category");

            migrationBuilder.DropTable(
                name: "featured_photo");

            migrationBuilder.DropTable(
                name: "featured_post");

            migrationBuilder.DropTable(
                name: "link");

            migrationBuilder.DropTable(
                name: "link_exchange");

            migrationBuilder.DropTable(
                name: "outbox_message");

            migrationBuilder.DropTable(
                name: "post_publication");

            migrationBuilder.DropTable(
                name: "post_translation");

            migrationBuilder.DropTable(
                name: "publication_channel");

            migrationBuilder.DropTable(
                name: "top_post");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "visit_record");

            migrationBuilder.DropTable(
                name: "anonymous_user");

            migrationBuilder.DropTable(
                name: "photo");

            migrationBuilder.DropTable(
                name: "post");

            migrationBuilder.DropTable(
                name: "category");
        }
    }
}

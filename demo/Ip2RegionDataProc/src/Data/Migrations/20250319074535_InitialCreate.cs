using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ip2RegionDataProc.src.Data.Migrations {
    /// <inheritdoc />
    public partial class InitialCreate : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    event_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    event_type = table.Column<string>(type: "TEXT", nullable: false),
                    username = table.Column<string>(type: "TEXT", nullable: false),
                    timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    entity_name = table.Column<string>(type: "TEXT", nullable: true),
                    entity_id = table.Column<string>(type: "TEXT", nullable: true),
                    original_values = table.Column<string>(type: "TEXT", nullable: true),
                    current_values = table.Column<string>(type: "TEXT", nullable: true),
                    changes = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => { table.PrimaryKey("pk_audit_logs", x => x.id); });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "audit_logs");
        }
    }
}
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TeamStore.Keeper.Migrations
{
    public partial class FreshDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationIdentities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AzureAdObjectIdentifier = table.Column<string>(type: "TEXT", nullable: true),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    AzureAdName = table.Column<string>(type: "TEXT", nullable: true),
                    AzureAdNameIdentifier = table.Column<string>(type: "TEXT", nullable: true),
                    Upn = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationIdentities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessIdentifiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<int>(type: "INTEGER", nullable: true),
                    IdentityId = table.Column<int>(type: "INTEGER", nullable: true),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedById = table.Column<int>(type: "INTEGER", nullable: true),
                    ProjectForeignKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessIdentifiers_ApplicationIdentities_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessIdentifiers_ApplicationIdentities_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "ApplicationIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessIdentifiers_ApplicationIdentities_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessIdentifiers_Projects_ProjectForeignKey",
                        column: x => x.ProjectForeignKey,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<int>(type: "INTEGER", nullable: true),
                    Discriminator = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedById = table.Column<int>(type: "INTEGER", nullable: true),
                    ProjectForeignKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Domain = table.Column<string>(type: "TEXT", nullable: true),
                    Login = table.Column<string>(type: "TEXT", nullable: true),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_ApplicationIdentities_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "ApplicationIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_ApplicationIdentities_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "ApplicationIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_Projects_ProjectForeignKey",
                        column: x => x.ProjectForeignKey,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssetForeignKey = table.Column<int>(type: "INTEGER", nullable: true),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NewValue = table.Column<string>(type: "TEXT", nullable: true),
                    OldValue = table.Column<string>(type: "TEXT", nullable: true),
                    RemoteIpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_ApplicationIdentities_ActedByUserId",
                        column: x => x.ActedByUserId,
                        principalTable: "ApplicationIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Assets_AssetForeignKey",
                        column: x => x.AssetForeignKey,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessIdentifiers_CreatedById",
                table: "AccessIdentifiers",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AccessIdentifiers_IdentityId",
                table: "AccessIdentifiers",
                column: "IdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessIdentifiers_ModifiedById",
                table: "AccessIdentifiers",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_AccessIdentifiers_ProjectForeignKey",
                table: "AccessIdentifiers",
                column: "ProjectForeignKey");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CreatedById",
                table: "Assets",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ModifiedById",
                table: "Assets",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ProjectForeignKey",
                table: "Assets",
                column: "ProjectForeignKey");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ActedByUserId",
                table: "Events",
                column: "ActedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_AssetForeignKey",
                table: "Events",
                column: "AssetForeignKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessIdentifiers");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "ApplicationIdentities");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}

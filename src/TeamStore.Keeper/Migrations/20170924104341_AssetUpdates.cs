using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TeamStore.Keeper.Migrations
{
    public partial class AssetUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Assets");

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Assets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Assets",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ModifiedById",
                table: "Assets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "Assets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CreatedById",
                table: "Assets",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ModifiedById",
                table: "Assets",
                column: "ModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_ApplicationIdentities_CreatedById",
                table: "Assets",
                column: "CreatedById",
                principalTable: "ApplicationIdentities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_ApplicationIdentities_ModifiedById",
                table: "Assets",
                column: "ModifiedById",
                principalTable: "ApplicationIdentities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_ApplicationIdentities_CreatedById",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_ApplicationIdentities_ModifiedById",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_CreatedById",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_ModifiedById",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Assets");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Assets",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}

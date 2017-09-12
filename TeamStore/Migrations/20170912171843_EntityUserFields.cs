using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TeamStore.Migrations
{
    public partial class EntityUserFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Events",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "AccessIdentifiers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdentityId",
                table: "AccessIdentifiers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedById",
                table: "AccessIdentifiers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_UserId",
                table: "Events",
                column: "UserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_AccessIdentifiers_ApplicationIdentities_CreatedById",
                table: "AccessIdentifiers",
                column: "CreatedById",
                principalTable: "ApplicationIdentities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessIdentifiers_ApplicationIdentities_IdentityId",
                table: "AccessIdentifiers",
                column: "IdentityId",
                principalTable: "ApplicationIdentities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessIdentifiers_ApplicationIdentities_ModifiedById",
                table: "AccessIdentifiers",
                column: "ModifiedById",
                principalTable: "ApplicationIdentities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_ApplicationIdentities_UserId",
                table: "Events",
                column: "UserId",
                principalTable: "ApplicationIdentities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessIdentifiers_ApplicationIdentities_CreatedById",
                table: "AccessIdentifiers");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessIdentifiers_ApplicationIdentities_IdentityId",
                table: "AccessIdentifiers");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessIdentifiers_ApplicationIdentities_ModifiedById",
                table: "AccessIdentifiers");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_ApplicationIdentities_UserId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_UserId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_AccessIdentifiers_CreatedById",
                table: "AccessIdentifiers");

            migrationBuilder.DropIndex(
                name: "IX_AccessIdentifiers_IdentityId",
                table: "AccessIdentifiers");

            migrationBuilder.DropIndex(
                name: "IX_AccessIdentifiers_ModifiedById",
                table: "AccessIdentifiers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "AccessIdentifiers");

            migrationBuilder.DropColumn(
                name: "IdentityId",
                table: "AccessIdentifiers");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "AccessIdentifiers");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TeamStore.Migrations
{
    public partial class ApplicationUserIpUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessIpAddress",
                table: "ApplicationIdentities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AzureAdName",
                table: "ApplicationIdentities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignInIpAddress",
                table: "ApplicationIdentities",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessIpAddress",
                table: "ApplicationIdentities");

            migrationBuilder.DropColumn(
                name: "AzureAdName",
                table: "ApplicationIdentities");

            migrationBuilder.DropColumn(
                name: "SignInIpAddress",
                table: "ApplicationIdentities");
        }
    }
}

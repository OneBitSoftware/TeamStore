using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TeamStore.Migrations
{
    public partial class ApplicationUserEventsIpUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "Events",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteIpAddress",
                table: "Events",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AzureAdName",
                table: "ApplicationIdentities",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RemoteIpAddress",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "AzureAdName",
                table: "ApplicationIdentities");
        }
    }
}

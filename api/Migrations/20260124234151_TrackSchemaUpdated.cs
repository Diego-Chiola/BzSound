using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class TrackSchemaUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5b2aaae0-bace-408f-869b-0f773b0adbd0");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d390c202-0f2f-4ece-8c77-3a9bfd4fcd45");

            migrationBuilder.DropForeignKey("FK_Tracks_AspNetUsers_UserId", "Tracks");
            migrationBuilder.DropForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", "AspNetUserTokens");
            migrationBuilder.DropForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", "AspNetUserRoles");
            migrationBuilder.DropForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", "AspNetUserRoles");
            migrationBuilder.DropForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", "AspNetUserLogins");
            migrationBuilder.DropForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", "AspNetUserClaims");
            migrationBuilder.DropForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", "AspNetRoleClaims");

            migrationBuilder.DropPrimaryKey("PK_AspNetUserTokens", "AspNetUserTokens");
            migrationBuilder.DropPrimaryKey("PK_AspNetUserRoles", "AspNetUserRoles");
            migrationBuilder.DropPrimaryKey("PK_AspNetUserLogins", "AspNetUserLogins");
            migrationBuilder.DropPrimaryKey("PK_AspNetUserClaims", "AspNetUserClaims");
            migrationBuilder.DropPrimaryKey("PK_AspNetRoleClaims", "AspNetRoleClaims");
            migrationBuilder.DropPrimaryKey("PK_AspNetUsers", "AspNetUsers");
            migrationBuilder.DropPrimaryKey("PK_AspNetRoles", "AspNetRoles");
            migrationBuilder.DropIndex(
                name: "IX_Tracks_UserId",
                table: "Tracks");

            migrationBuilder.AlterColumn<Guid>("Id", "AspNetUsers", "uniqueidentifier", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(450)");
            migrationBuilder.AlterColumn<Guid>("UserId", "AspNetUserTokens", "uniqueidentifier", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(450)");
            migrationBuilder.AlterColumn<Guid>("UserId", "AspNetUserRoles", "uniqueidentifier", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(450)");
            migrationBuilder.AlterColumn<Guid>("RoleId", "AspNetUserRoles", "uniqueidentifier", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(450)");
            migrationBuilder.AlterColumn<Guid>("UserId", "AspNetUserLogins", "uniqueidentifier", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(450)");
            migrationBuilder.AlterColumn<Guid>("UserId", "AspNetUserClaims", "uniqueidentifier", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(450)");
            migrationBuilder.AlterColumn<Guid>("RoleId", "AspNetRoleClaims", "uniqueidentifier", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(450)");
            migrationBuilder.AlterColumn<Guid>("Id", "AspNetRoles", "uniqueidentifier", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(450)");
            migrationBuilder.AlterColumn<Guid>("UserId", "Tracks", "uniqueidentifier", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(450)", oldNullable: true);

            // re-add PK/Indexes
            migrationBuilder.AddPrimaryKey("PK_AspNetRoles", "AspNetRoles", "Id");
            migrationBuilder.AddPrimaryKey("PK_AspNetUsers", "AspNetUsers", "Id");
            migrationBuilder.AddPrimaryKey("PK_AspNetUserClaims", "AspNetUserClaims", new[] { "Id" });
            migrationBuilder.AddPrimaryKey("PK_AspNetUserLogins", "AspNetUserLogins", new[] { "LoginProvider", "ProviderKey" });
            migrationBuilder.AddPrimaryKey("PK_AspNetUserRoles", "AspNetUserRoles", new[] { "UserId", "RoleId" });
            migrationBuilder.AddPrimaryKey("PK_AspNetRoleClaims", "AspNetRoleClaims", "Id");
            migrationBuilder.CreateIndex("IX_Tracks_UserId", "Tracks", "UserId");

            // re-add FKs
            migrationBuilder.AddForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", "AspNetRoleClaims", "RoleId", "AspNetRoles", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", "AspNetUserClaims", "UserId", "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", "AspNetUserLogins", "UserId", "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", "AspNetUserRoles", "RoleId", "AspNetRoles", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", "AspNetUserRoles", "UserId", "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", "AspNetUserTokens", "UserId", "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey("FK_Tracks_AspNetUsers_UserId", "Tracks", "UserId", "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // add new Track columns
            migrationBuilder.AddColumn<string>("ContentType", "Tracks", "nvarchar(max)", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<double>("Duration", "Tracks", "float", nullable: true);
            migrationBuilder.AddColumn<long>("FileSize", "Tracks", "bigint", nullable: false, defaultValue: 0L);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "AspNetRoles",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "RoleId",
                table: "AspNetRoleClaims",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "IdentityRole",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityRole", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "IdentityRole",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5b2aaae0-bace-408f-869b-0f773b0adbd0", null, "User", "USER" },
                    { "d390c202-0f2f-4ece-8c77-3a9bfd4fcd45", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentityRole");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Tracks");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Tracks",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "AspNetUserRoles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AspNetUserRoles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AspNetUserClaims",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "AspNetRoles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "AspNetRoleClaims",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5b2aaae0-bace-408f-869b-0f773b0adbd0", null, "User", "USER" },
                    { "d390c202-0f2f-4ece-8c77-3a9bfd4fcd45", null, "Admin", "ADMIN" }
                });
        }
    }
}

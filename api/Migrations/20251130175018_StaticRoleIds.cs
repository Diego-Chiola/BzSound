using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class StaticRoleIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8c0770c4-97d3-4479-b45a-47be2300037b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b3adf9a7-ff6d-491c-aed6-74e67abb8f09");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
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
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5b2aaae0-bace-408f-869b-0f773b0adbd0");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d390c202-0f2f-4ece-8c77-3a9bfd4fcd45");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "8c0770c4-97d3-4479-b45a-47be2300037b", null, "Admin", "ADMIN" },
                    { "b3adf9a7-ff6d-491c-aed6-74e67abb8f09", null, "User", "USER" }
                });
        }
    }
}

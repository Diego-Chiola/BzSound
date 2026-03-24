using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class TrackModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "Tracks",
                newName: "Format");

            migrationBuilder.AlterColumn<long>(
                name: "Duration",
                table: "Tracks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Format",
                table: "Tracks",
                newName: "ContentType");

            migrationBuilder.AlterColumn<double>(
                name: "Duration",
                table: "Tracks",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}

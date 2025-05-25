using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_GameRooms_GameRoomId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "HasGuessedCorrectly",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Players");

            migrationBuilder.AlterColumn<string>(
                name: "GameRoomId",
                table: "Players",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "GameRooms",
                keyColumn: "RoomId",
                keyValue: null,
                column: "RoomId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "RoomId",
                table: "GameRooms",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "GameRooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_GameRooms_RoomId",
                table: "GameRooms",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_UserId",
                table: "Players",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRooms_CreatorId",
                table: "GameRooms",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameRooms_Users_CreatorId",
                table: "GameRooms",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_GameRooms_GameRoomId",
                table: "Players",
                column: "GameRoomId",
                principalTable: "GameRooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Users_UserId",
                table: "Players",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRooms_Users_CreatorId",
                table: "GameRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_GameRooms_GameRoomId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Users_UserId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_UserId",
                table: "Players");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_GameRooms_RoomId",
                table: "GameRooms");

            migrationBuilder.DropIndex(
                name: "IX_GameRooms_CreatorId",
                table: "GameRooms");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "GameRooms");

            migrationBuilder.AlterColumn<int>(
                name: "GameRoomId",
                table: "Players",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "HasGuessedCorrectly",
                table: "Players",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Players",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "RoomId",
                table: "GameRooms",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_GameRooms_GameRoomId",
                table: "Players",
                column: "GameRoomId",
                principalTable: "GameRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

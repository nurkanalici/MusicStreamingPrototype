using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStreamingPrototype.API.Migrations
{
    
    public partial class RenameArchiveTable : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Archives_Tracks_TrackId",
                table: "Archives");

            migrationBuilder.DropForeignKey(
                name: "FK_Archives_Users_UserId",
                table: "Archives");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Archives",
                table: "Archives");

            migrationBuilder.RenameTable(
                name: "Archives",
                newName: "UserTrackArchive");

            migrationBuilder.RenameIndex(
                name: "IX_Archives_TrackId",
                table: "UserTrackArchive",
                newName: "IX_UserTrackArchive_TrackId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTrackArchive",
                table: "UserTrackArchive",
                columns: new[] { "UserId", "TrackId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrackArchive_Tracks_TrackId",
                table: "UserTrackArchive",
                column: "TrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrackArchive_Users_UserId",
                table: "UserTrackArchive",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTrackArchive_Tracks_TrackId",
                table: "UserTrackArchive");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTrackArchive_Users_UserId",
                table: "UserTrackArchive");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTrackArchive",
                table: "UserTrackArchive");

            migrationBuilder.RenameTable(
                name: "UserTrackArchive",
                newName: "Archives");

            migrationBuilder.RenameIndex(
                name: "IX_UserTrackArchive_TrackId",
                table: "Archives",
                newName: "IX_Archives_TrackId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Archives",
                table: "Archives",
                columns: new[] { "UserId", "TrackId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Archives_Tracks_TrackId",
                table: "Archives",
                column: "TrackId",
                principalTable: "Tracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Archives_Users_UserId",
                table: "Archives",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

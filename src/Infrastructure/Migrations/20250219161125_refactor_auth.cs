using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class refactor_auth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivedTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthSessions",
                table: "AuthSessions");

            migrationBuilder.DropColumn(
                name: "CurrentToken",
                table: "AuthSessions");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Accounts");

            migrationBuilder.RenameTable(
                name: "AuthSessions",
                newName: "AuthSession");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AuthSession",
                newName: "CurrentTokenId");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "AuthSession",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthSession",
                table: "AuthSession",
                columns: new[] { "AccountId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_AuthSession_Accounts_AccountId",
                table: "AuthSession",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthSession_Accounts_AccountId",
                table: "AuthSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthSession",
                table: "AuthSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "AuthSession");

            migrationBuilder.RenameTable(
                name: "AuthSession",
                newName: "AuthSessions");

            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "Users");

            migrationBuilder.RenameColumn(
                name: "CurrentTokenId",
                table: "AuthSessions",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "CurrentToken",
                table: "AuthSessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthSessions",
                table: "AuthSessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ArchivedTokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedTokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_ArchivedTokens_AuthSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AuthSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedTokens_SessionId",
                table: "ArchivedTokens",
                column: "SessionId");
        }
    }
}

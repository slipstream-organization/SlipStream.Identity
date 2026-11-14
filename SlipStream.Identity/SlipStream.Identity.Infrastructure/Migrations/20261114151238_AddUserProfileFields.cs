using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlipStream.Identity.Infrastructure.Migrations
{
    public partial class AddUserProfileFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FavouriteDriver",
                table: "users",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FavouriteTeam",
                table: "users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Handle",
                table: "users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedHandle",
                table: "users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE users
                SET "Handle" = 'user_' || substr(replace("Id"::text, '-', ''), 1, 12),
                    "NormalizedHandle" = upper('user_' || substr(replace("Id"::text, '-', ''), 1, 12))
                WHERE "Handle" = '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_users_NormalizedHandle",
                table: "users",
                column: "NormalizedHandle",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_NormalizedHandle",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "users");

            migrationBuilder.DropColumn(
                name: "FavouriteDriver",
                table: "users");

            migrationBuilder.DropColumn(
                name: "FavouriteTeam",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Handle",
                table: "users");

            migrationBuilder.DropColumn(
                name: "NormalizedHandle",
                table: "users");
        }
    }
}

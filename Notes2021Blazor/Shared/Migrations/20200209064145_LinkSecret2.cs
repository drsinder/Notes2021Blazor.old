using Microsoft.EntityFrameworkCore.Migrations;

namespace Notes2021Blazor.Shared.Migrations
{
    public partial class LinkSecret2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5069c754-4365-4c51-9c04-4a96ddf71a63");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3045b25-8c9f-4371-8766-b42446b540c9");

            migrationBuilder.AddColumn<string>(
                name: "Secret",
                table: "LinkQueue",
                maxLength: 50,
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "20fc3e46-a7d2-441f-ab3f-66ffa17b3047", "51e8cdb1-b6a4-4368-ad93-8619e1df8cae", "User", "USER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "25eb3d03-8032-4357-9311-31f11105bd26", "0d9894ac-2de6-4905-833f-2c4be4e54f24", "Admin", "ADMIN" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "20fc3e46-a7d2-441f-ab3f-66ffa17b3047");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "25eb3d03-8032-4357-9311-31f11105bd26");

            migrationBuilder.DropColumn(
                name: "Secret",
                table: "LinkQueue");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "5069c754-4365-4c51-9c04-4a96ddf71a63", "1bd84932-7868-40cf-bb82-3d19e4d72039", "User", "USER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "c3045b25-8c9f-4371-8766-b42446b540c9", "90eaa2d8-c760-45e7-81b7-5640865ebc95", "Admin", "ADMIN" });
        }
    }
}

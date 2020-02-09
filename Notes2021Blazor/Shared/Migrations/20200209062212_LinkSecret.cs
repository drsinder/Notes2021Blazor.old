using Microsoft.EntityFrameworkCore.Migrations;

namespace Notes2021Blazor.Shared.Migrations
{
    public partial class LinkSecret : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "426a75a5-4d6c-4c87-b517-c6d4f367fccc");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c6a2cb86-572c-429d-86f1-3fdc563ba0d9");

            migrationBuilder.AddColumn<string>(
                name: "Secret",
                table: "LinkedFile",
                maxLength: 50,
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "5069c754-4365-4c51-9c04-4a96ddf71a63", "1bd84932-7868-40cf-bb82-3d19e4d72039", "User", "USER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "c3045b25-8c9f-4371-8766-b42446b540c9", "90eaa2d8-c760-45e7-81b7-5640865ebc95", "Admin", "ADMIN" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5069c754-4365-4c51-9c04-4a96ddf71a63");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c3045b25-8c9f-4371-8766-b42446b540c9");

            migrationBuilder.DropColumn(
                name: "Secret",
                table: "LinkedFile");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "c6a2cb86-572c-429d-86f1-3fdc563ba0d9", "05ab070c-7651-4b37-9f2e-6da89e8562b2", "User", "USER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "426a75a5-4d6c-4c87-b517-c6d4f367fccc", "f186352f-84bc-4af0-b108-3a5c8efb9dc6", "Admin", "ADMIN" });
        }
    }
}

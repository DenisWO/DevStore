using Microsoft.EntityFrameworkCore.Migrations;

namespace DevStore.Identity.API.Migrations
{
    public partial class InsertedRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "3057cec7-36f0-42e7-9a70-fd25a911c5a2", "028a8208-f6ab-4284-8ae7-ec5a85d640b7", "Client", "CLIENT" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "ea34fd43-afb2-4f87-9e56-5a1ab7acd38c", "b85547ba-a7e9-4411-b6c0-2df21fe7f868", "Admin", "ADMINISTRATOR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3057cec7-36f0-42e7-9a70-fd25a911c5a2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ea34fd43-afb2-4f87-9e56-5a1ab7acd38c");
        }
    }
}

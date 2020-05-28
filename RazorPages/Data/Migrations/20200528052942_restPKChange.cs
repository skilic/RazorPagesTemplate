using Microsoft.EntityFrameworkCore.Migrations;

namespace VMenu.Data.Migrations
{
    public partial class restPKChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Restaurants",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "RestaurantID",
                table: "Restaurants");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "Restaurants",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Restaurants",
                table: "Restaurants",
                column: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Restaurants",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Restaurants");

            migrationBuilder.AddColumn<int>(
                name: "RestaurantID",
                table: "Restaurants",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Restaurants",
                table: "Restaurants",
                column: "RestaurantID");
        }
    }
}

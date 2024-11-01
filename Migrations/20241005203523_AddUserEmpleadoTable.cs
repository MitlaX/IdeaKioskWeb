using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaKioskWeb.Migrations
{
    public partial class AddUserEmpleadoTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroEmpleado",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserEmpleados",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    NumeroEmpleado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEmpleados", x => new { x.UserId, x.NumeroEmpleado });
                    table.ForeignKey(
                        name: "FK_UserEmpleados_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEmpleados_Empleados_NumeroEmpleado",
                        column: x => x.NumeroEmpleado,
                        principalTable: "Empleados",
                        principalColumn: "NumeroEmpleado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserEmpleados_NumeroEmpleado",
                table: "UserEmpleados",
                column: "NumeroEmpleado");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserEmpleados");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NumeroEmpleado",
                table: "AspNetUsers");
        }
    }
}

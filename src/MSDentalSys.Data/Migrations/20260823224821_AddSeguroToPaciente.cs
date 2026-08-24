using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MSDentalSys.Data.Migrations
{
    /// <inheritdoc hereda la documentación/>
    public partial class AddSeguroToPaciente : Migration
    {
        /// <inheritdoc hereda la documentación/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeguroId",
                table: "Pacientes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_SeguroId",
                table: "Pacientes",
                column: "SeguroId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Seguros_SeguroId",
                table: "Pacientes",
                column: "SeguroId",
                principalTable: "Seguros",
                principalColumn: "SeguroId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Seguros_SeguroId",
                table: "Pacientes");

            migrationBuilder.DropIndex(
                name: "IX_Pacientes_SeguroId",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "SeguroId",
                table: "Pacientes");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AltCascadeHistorico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VeiculoHistorico_Veiculo",
                schema: "public",
                table: "veiculoshistorico");

            migrationBuilder.AddForeignKey(
                name: "FK_VeiculoHistorico_Veiculo",
                schema: "public",
                table: "veiculoshistorico",
                column: "veiculo_id",
                principalSchema: "public",
                principalTable: "veiculo",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VeiculoHistorico_Veiculo",
                schema: "public",
                table: "veiculoshistorico");

            migrationBuilder.AddForeignKey(
                name: "FK_VeiculoHistorico_Veiculo",
                schema: "public",
                table: "veiculoshistorico",
                column: "veiculo_id",
                principalSchema: "public",
                principalTable: "veiculo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

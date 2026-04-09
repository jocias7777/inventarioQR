using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioQR.Migrations
{
    /// <inheritdoc />
    public partial class AddOperaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Operaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    PosicionOrigenId = table.Column<int>(type: "int", nullable: true),
                    PosicionDestinoId = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    AsignadoA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCompletado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operaciones_Posiciones_PosicionDestinoId",
                        column: x => x.PosicionDestinoId,
                        principalTable: "Posiciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Operaciones_Posiciones_PosicionOrigenId",
                        column: x => x.PosicionOrigenId,
                        principalTable: "Posiciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Operaciones_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Operaciones_PosicionDestinoId",
                table: "Operaciones",
                column: "PosicionDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Operaciones_PosicionOrigenId",
                table: "Operaciones",
                column: "PosicionOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_Operaciones_ProductoId",
                table: "Operaciones",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Operaciones");
        }
    }
}

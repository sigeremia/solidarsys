using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SolidarSYS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitasTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "visitas",
                columns: table => new
                {
                    id_visita = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_beneficiario = table.Column<int>(type: "integer", nullable: false),
                    data_visita = table.Column<DateOnly>(type: "date", nullable: false),
                    responsavel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    observacoes = table.Column<string>(type: "text", nullable: true),
                    data_cadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visitas", x => x.id_visita);
                    table.ForeignKey(
                        name: "FK_visitas_beneficiarios_id_beneficiario",
                        column: x => x.id_beneficiario,
                        principalTable: "beneficiarios",
                        principalColumn: "id_beneficiario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_visitas_id_beneficiario_data_visita",
                table: "visitas",
                columns: new[] { "id_beneficiario", "data_visita" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visitas");
        }
    }
}

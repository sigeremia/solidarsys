using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SolidarSYS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDoadores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "beneficiarios",
                columns: table => new
                {
                    id_beneficiario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: false),
                    endereco = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    id_comunidade = table.Column<int>(type: "integer", nullable: false),
                    total_membros_familia = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_beneficiarios", x => x.id_beneficiario);
                    table.ForeignKey(
                        name: "FK_beneficiarios_comunidades_id_comunidade",
                        column: x => x.id_comunidade,
                        principalTable: "comunidades",
                        principalColumn: "id_comunidade",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "doadores",
                columns: table => new
                {
                    id_doador = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nome_fantasia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    documento = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    telefone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    data_cadastro = table.Column<DateOnly>(type: "date", nullable: false),
                    observacao = table.Column<string>(type: "text", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doadores", x => x.id_doador);
                });

            migrationBuilder.CreateTable(
                name: "doacoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_doador = table.Column<int>(type: "integer", nullable: false),
                    data = table.Column<DateOnly>(type: "date", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data_cadastro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doacoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_doacoes_doadores_id_doador",
                        column: x => x.id_doador,
                        principalTable: "doadores",
                        principalColumn: "id_doador",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_beneficiarios_cpf",
                table: "beneficiarios",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_beneficiarios_id_comunidade",
                table: "beneficiarios",
                column: "id_comunidade");

            migrationBuilder.CreateIndex(
                name: "IX_doacoes_id_doador_data",
                table: "doacoes",
                columns: new[] { "id_doador", "data" });

            migrationBuilder.CreateIndex(
                name: "IX_doadores_documento",
                table: "doadores",
                column: "documento",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beneficiarios");

            migrationBuilder.DropTable(
                name: "doacoes");

            migrationBuilder.DropTable(
                name: "doadores");
        }
    }
}

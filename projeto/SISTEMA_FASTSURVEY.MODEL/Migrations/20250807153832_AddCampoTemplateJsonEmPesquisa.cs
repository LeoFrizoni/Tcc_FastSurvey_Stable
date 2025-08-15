using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SISTEMA_FASTSURVEY.MODEL.Migrations
{
    /// <inheritdoc />
    public partial class AddCampoTemplateJsonEmPesquisa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tipopergunta",
                columns: table => new
                {
                    tipoperguntaid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipopergunta = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    desabilitado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tipopergunta_pkey", x => x.tipoperguntaid);
                });

            migrationBuilder.CreateTable(
                name: "tipopesquisa",
                columns: table => new
                {
                    tipopesquisaid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipopesquisa = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    desabilitado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tipopesquisa_pkey", x => x.tipopesquisaid);
                });

            migrationBuilder.CreateTable(
                name: "tipousuario",
                columns: table => new
                {
                    usuarioid = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('usuarios_usuarioid_seq'::regclass)"),
                    tipousuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("usuarios_pkey", x => x.usuarioid);
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    tokenid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    dataregistro = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    dataexpirado = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("tokens_pkey", x => x.tokenid);
                });

            migrationBuilder.CreateTable(
                name: "login",
                columns: table => new
                {
                    loginid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    senha = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dataregistro = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    tipousuarioid = table.Column<int>(type: "integer", nullable: true),
                    tipousuariotexto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("login_pkey", x => x.loginid);
                    table.ForeignKey(
                        name: "fk_login_tipousuarioid",
                        column: x => x.tipousuarioid,
                        principalTable: "tipousuario",
                        principalColumn: "usuarioid");
                });

            migrationBuilder.CreateTable(
                name: "pesquisas",
                columns: table => new
                {
                    pesquisaid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    loginid = table.Column<int>(type: "integer", nullable: false),
                    tipopesquisaid = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    templateJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pesquisas_pkey", x => x.pesquisaid);
                    table.ForeignKey(
                        name: "fk_pesquisas_loginid",
                        column: x => x.loginid,
                        principalTable: "login",
                        principalColumn: "loginid");
                    table.ForeignKey(
                        name: "pesquisas_tipopesquisaid_fkey",
                        column: x => x.tipopesquisaid,
                        principalTable: "tipopesquisa",
                        principalColumn: "tipopesquisaid");
                });

            migrationBuilder.CreateTable(
                name: "anexos",
                columns: table => new
                {
                    anexoid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pesquisaid = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    extensao = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("anexos_pkey", x => x.anexoid);
                    table.ForeignKey(
                        name: "anexos_pesquisaid_fkey",
                        column: x => x.pesquisaid,
                        principalTable: "pesquisas",
                        principalColumn: "pesquisaid");
                });

            migrationBuilder.CreateTable(
                name: "perguntas",
                columns: table => new
                {
                    perguntaid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipoperguntaid = table.Column<int>(type: "integer", nullable: false),
                    pesquisaid = table.Column<int>(type: "integer", nullable: false),
                    texto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("perguntas_pkey", x => x.perguntaid);
                    table.ForeignKey(
                        name: "perguntas_pesquisaid_fkey",
                        column: x => x.pesquisaid,
                        principalTable: "pesquisas",
                        principalColumn: "pesquisaid");
                    table.ForeignKey(
                        name: "perguntas_tipoperguntaid_fkey",
                        column: x => x.tipoperguntaid,
                        principalTable: "tipopergunta",
                        principalColumn: "tipoperguntaid");
                });

            migrationBuilder.CreateTable(
                name: "opcoespergunta",
                columns: table => new
                {
                    opcaoid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    perguntaid = table.Column<int>(type: "integer", nullable: false),
                    texto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("opcoespergunta_pkey", x => x.opcaoid);
                    table.ForeignKey(
                        name: "opcoespergunta_perguntaid_fkey",
                        column: x => x.perguntaid,
                        principalTable: "perguntas",
                        principalColumn: "perguntaid");
                });

            migrationBuilder.CreateTable(
                name: "respostas",
                columns: table => new
                {
                    respostaid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    perguntaid = table.Column<int>(type: "integer", nullable: false),
                    texto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    dataresposta = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("respostas_pkey", x => x.respostaid);
                    table.ForeignKey(
                        name: "respostas_perguntaid_fkey",
                        column: x => x.perguntaid,
                        principalTable: "perguntas",
                        principalColumn: "perguntaid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_anexos_pesquisaid",
                table: "anexos",
                column: "pesquisaid");

            migrationBuilder.CreateIndex(
                name: "IX_login_tipousuarioid",
                table: "login",
                column: "tipousuarioid");

            migrationBuilder.CreateIndex(
                name: "IX_opcoespergunta_perguntaid",
                table: "opcoespergunta",
                column: "perguntaid");

            migrationBuilder.CreateIndex(
                name: "IX_perguntas_pesquisaid",
                table: "perguntas",
                column: "pesquisaid");

            migrationBuilder.CreateIndex(
                name: "IX_perguntas_tipoperguntaid",
                table: "perguntas",
                column: "tipoperguntaid");

            migrationBuilder.CreateIndex(
                name: "IX_pesquisas_loginid",
                table: "pesquisas",
                column: "loginid");

            migrationBuilder.CreateIndex(
                name: "IX_pesquisas_tipopesquisaid",
                table: "pesquisas",
                column: "tipopesquisaid");

            migrationBuilder.CreateIndex(
                name: "IX_respostas_perguntaid",
                table: "respostas",
                column: "perguntaid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anexos");

            migrationBuilder.DropTable(
                name: "opcoespergunta");

            migrationBuilder.DropTable(
                name: "respostas");

            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropTable(
                name: "perguntas");

            migrationBuilder.DropTable(
                name: "pesquisas");

            migrationBuilder.DropTable(
                name: "tipopergunta");

            migrationBuilder.DropTable(
                name: "login");

            migrationBuilder.DropTable(
                name: "tipopesquisa");

            migrationBuilder.DropTable(
                name: "tipousuario");
        }
    }
}

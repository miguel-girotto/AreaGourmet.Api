using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AreaGourmet.Api.Migrations
{
    /// <inheritdoc />
    public partial class GestaoIntegrada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enderecos_Clientes_ClienteId",
                table: "Enderecos");

            migrationBuilder.DropForeignKey(
                name: "FK_EstoqueBebidas_Produtos_ProdutoId",
                table: "EstoqueBebidas");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensPedido_Pedidos_PedidoId",
                table: "ItensPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensPedido_Produtos_ProdutoId",
                table: "ItensPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Enderecos_EnderecoId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Usuarios_UsuarioId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Categorias_CategoriaId",
                table: "Produtos");

            migrationBuilder.AddColumn<bool>(
                name: "PodeCadastrarClientes",
                table: "Usuarios",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VersaoSessao",
                table: "Usuarios",
                type: "varchar(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "DisponivelVenda",
                table: "Produtos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ChaveOperacao",
                table: "Pedidos",
                type: "varchar(36)",
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ClienteHistorico",
                table: "Pedidos",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoHistorico",
                table: "Pedidos",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NomeProdutoHistorico",
                table: "ItensPedido",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Clientes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Categorias",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    Acao = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataHora = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Auditorias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Consumos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    EstoqueBebidaId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consumos_EstoqueBebidas_EstoqueBebidaId",
                        column: x => x.EstoqueBebidaId,
                        principalTable: "EstoqueBebidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consumos_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ControleOperacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Versao = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControleOperacoes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Movimentacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EstoqueBebidaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PedidoId = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<string>(type: "varchar(24)", maxLength: 24, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SaldoApos = table.Column<decimal>(type: "decimal(65,30)", precision: 65, scale: 30, nullable: false),
                    Observacao = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataHora = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movimentacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movimentacoes_EstoqueBebidas_EstoqueBebidaId",
                        column: x => x.EstoqueBebidaId,
                        principalTable: "EstoqueBebidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movimentacoes_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movimentacoes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "ControleOperacoes",
                columns: new[] { "Id", "Versao" },
                values: new object[] { 1, 0L });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ChaveOperacao",
                table: "Pedidos",
                column: "ChaveOperacao",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_DataHora",
                table: "Pedidos",
                column: "DataHora");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_UsuarioId",
                table: "Auditorias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumos_EstoqueBebidaId",
                table: "Consumos",
                column: "EstoqueBebidaId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumos_ProdutoId_EstoqueBebidaId",
                table: "Consumos",
                columns: new[] { "ProdutoId", "EstoqueBebidaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_DataHora",
                table: "Movimentacoes",
                column: "DataHora");

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_EstoqueBebidaId",
                table: "Movimentacoes",
                column: "EstoqueBebidaId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_PedidoId",
                table: "Movimentacoes",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacoes_UsuarioId",
                table: "Movimentacoes",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enderecos_Clientes_ClienteId",
                table: "Enderecos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EstoqueBebidas_Produtos_ProdutoId",
                table: "EstoqueBebidas",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensPedido_Pedidos_PedidoId",
                table: "ItensPedido",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensPedido_Produtos_ProdutoId",
                table: "ItensPedido",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId",
                table: "Pedidos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Enderecos_EnderecoId",
                table: "Pedidos",
                column: "EnderecoId",
                principalTable: "Enderecos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Usuarios_UsuarioId",
                table: "Pedidos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Categorias_CategoriaId",
                table: "Produtos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enderecos_Clientes_ClienteId",
                table: "Enderecos");

            migrationBuilder.DropForeignKey(
                name: "FK_EstoqueBebidas_Produtos_ProdutoId",
                table: "EstoqueBebidas");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensPedido_Pedidos_PedidoId",
                table: "ItensPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensPedido_Produtos_ProdutoId",
                table: "ItensPedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Enderecos_EnderecoId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Usuarios_UsuarioId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Categorias_CategoriaId",
                table: "Produtos");

            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "Consumos");

            migrationBuilder.DropTable(
                name: "ControleOperacoes");

            migrationBuilder.DropTable(
                name: "Movimentacoes");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_ChaveOperacao",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_DataHora",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "PodeCadastrarClientes",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "VersaoSessao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DisponivelVenda",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "ChaveOperacao",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ClienteHistorico",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "EnderecoHistorico",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "NomeProdutoHistorico",
                table: "ItensPedido");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Categorias");

            migrationBuilder.AddForeignKey(
                name: "FK_Enderecos_Clientes_ClienteId",
                table: "Enderecos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EstoqueBebidas_Produtos_ProdutoId",
                table: "EstoqueBebidas",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensPedido_Pedidos_PedidoId",
                table: "ItensPedido",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensPedido_Produtos_ProdutoId",
                table: "ItensPedido",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId",
                table: "Pedidos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Enderecos_EnderecoId",
                table: "Pedidos",
                column: "EnderecoId",
                principalTable: "Enderecos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Usuarios_UsuarioId",
                table: "Pedidos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Categorias_CategoriaId",
                table: "Produtos",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Api.Services;
public class DadosDemo(AppDbContext db, Operacoes op)
{
    public Task<int> Gerar(int usuario) => op.Transacao(async () =>
    {
        var categorias = new Dictionary<string, Categoria>();
        foreach (var nome in new[] { "Marmitas", "Bebidas", "Adicionais", "Insumos" })
        {
            var c = await db.Categorias.SingleOrDefaultAsync(c => c.Nome == nome);
            if (c is null) { c = new Categoria { Nome = nome }; db.Categorias.Add(c); await db.SaveChangesAsync(); }
            categorias[nome] = c;
        }
        var dados = new[] {
            ("Marmita Executiva", "Marmitas", 18.90m, "executiva", true), ("Marmita Fitness", "Marmitas", 17.90m, "fitness", true),
            ("Frango Grelhado", "Marmitas", 16.50m, "frango", true), ("Feijoada Completa", "Marmitas", 22.90m, "feijoada", true),
            ("Suco Natural Laranja", "Bebidas", 8.90m, "suco", true), ("Refrigerante Guaraná", "Bebidas", 6.50m, "guarana", true),
            ("Farofa Crocante", "Adicionais", 3.50m, "farofa", true), ("Embalagem de marmita", "Insumos", 1.00m, "embalagem", false) };
        foreach (var (nome, categoria, preco, imagem, venda) in dados)
        {
            var p = await db.Produtos.SingleOrDefaultAsync(p => p.Nome == nome && p.CategoriaId == categorias[categoria].Id);
            if (p is null)
            {
                p = new Produto { Nome = nome, CategoriaId = categorias[categoria].Id, PrecoAtual = preco, DisponivelVenda = venda, Imagem = $"/img/{imagem}.svg" };
                db.Produtos.Add(p); await db.SaveChangesAsync();
            }
            if (categoria is "Bebidas" or "Insumos" && !await db.EstoqueBebidas.AnyAsync(e => e.ProdutoId == p.Id))
            {
                var e = new EstoqueBebida { ProdutoId = p.Id, QuantidadeAtual = 100, QuantidadeMinima = 10 };
                db.EstoqueBebidas.Add(e); await db.SaveChangesAsync();
                db.Movimentacoes.Add(new MovimentacaoEstoque { EstoqueBebidaId = e.Id, UsuarioId = usuario, Tipo = "ENTRADA", Quantidade = 100, SaldoApos = 100, Observacao = "Carga de demonstração" });
            }
        }
        var embalagem = await db.EstoqueBebidas.SingleAsync(e => e.Produto.Nome == "Embalagem de marmita" && e.Produto.CategoriaId == categorias["Insumos"].Id);
        foreach (var p in await db.Produtos.Where(p => p.CategoriaId == categorias["Marmitas"].Id).ToListAsync())
            if (!await db.Consumos.AnyAsync(c => c.ProdutoId == p.Id && c.EstoqueBebidaId == embalagem.Id)) db.Consumos.Add(new ConsumoEstoque { ProdutoId = p.Id, EstoqueBebidaId = embalagem.Id, Quantidade = 1 });
        if (!await db.Clientes.AnyAsync(c => c.Telefone == "18999990000"))
        {
            var c = new Cliente { Nome = "Cliente Demonstração", Telefone = "18999990000" };
            c.Enderecos.Add(new Endereco { Logradouro = "Rua de Demonstração", Numero = "123", Bairro = "Centro", Rotulo = "Casa" });
            c.Enderecos.Add(new Endereco { Logradouro = "Avenida de Demonstração", Numero = "456", Bairro = "Centro", Rotulo = "Trabalho" });
            db.Clientes.Add(c);
        }
        op.Auditar(usuario, "Dados de demonstração inseridos sem excluir ou sobrescrever registros"); return 0;
    });
}

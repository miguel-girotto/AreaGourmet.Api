using System.Data;
using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using AreaGourmet.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Services;

public class RegraException(string message) : Exception(message);

public class Operacoes(AppDbContext db)
{
    // A linha de controle serializa alterações relacionadas, inclusive entre instâncias.
    // ReadCommitted evita leituras de snapshots anteriores à obtenção da trava.
    public async Task<T> Transacao<T>(Func<Task<T>> action)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await db.Database.ExecuteSqlRawAsync("UPDATE ControleOperacoes SET Versao = Versao + 1 WHERE Id = 1");
        var result = await action();
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return result;
    }
    public void Auditar(int? usuario, string acao) => db.Auditorias.Add(new Auditoria { UsuarioId = usuario, Acao = acao });
    public static void Exigir(bool condition, string message) { if (!condition) throw new RegraException(message); }

    public Task<int> CriarPedido(PedidoForm form, int usuario) => Transacao(async () =>
    {
        Exigir(Guid.TryParse(form.ChaveOperacao, out _), "Identificador da operação inválido. Reabra a tela de vendas.");
        var existing = await db.Pedidos.SingleOrDefaultAsync(p => p.ChaveOperacao == form.ChaveOperacao);
        if (existing is not null)
        {
            Exigir(existing.UsuarioId == usuario, "Esta operação pertence a outro operador.");
            return existing.Id;
        }
        Exigir(form.Itens.Count is > 0 and <= 100, "Adicione de 1 a 100 itens.");
        Exigir(form.Itens.All(i => i.Quantidade is > 0 and <= 999), "Quantidade inválida.");
        Exigir(new[] { "Mesa", "Retirada", "Delivery" }.Contains(form.TipoAtendimento), "Modalidade inválida.");
        Exigir(new[] { "PIX", "Cartão", "Dinheiro" }.Contains(form.FormaPagamento), "Pagamento inválido.");
        Exigir(form.TaxaEntrega is >= 0 and <= 9999.99m && decimal.Round(form.TaxaEntrega, 2) == form.TaxaEntrega, "Taxa de entrega inválida.");
        Exigir(form.TipoAtendimento != "Mesa" || form.NumeroMesa is > 0 and <= 999, "Informe uma mesa de 1 a 999.");
        Exigir(form.TipoAtendimento == "Delivery" || form.TaxaEntrega == 0, "Apenas Delivery aceita taxa de entrega.");
        var cliente = form.ClienteId.HasValue ? await db.Clientes.FindAsync(form.ClienteId) : null;
        Exigir(!form.ClienteId.HasValue || cliente is { Ativo: true }, "Cliente indisponível.");
        Endereco? endereco = null;
        if (form.TipoAtendimento == "Delivery")
        {
            Exigir(cliente is not null && form.EnderecoId.HasValue, "Delivery exige cliente e endereço.");
            endereco = await db.Enderecos.FindAsync(form.EnderecoId);
            Exigir(endereco is not null && endereco.ClienteId == cliente!.Id, "O endereço deve pertencer ao cliente selecionado.");
        }
        var ids = form.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await db.Produtos.Include(p => p.Categoria).Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        Exigir(produtos.Count == ids.Count && produtos.Values.All(p => p.Ativo && p.DisponivelVenda && p.Categoria.Ativo), "Há produto indisponível na comanda.");
        var pedido = new Pedido
        {
            ChaveOperacao = form.ChaveOperacao, UsuarioId = usuario, ClienteId = cliente?.Id,
            ClienteHistorico = cliente?.Nome, EnderecoId = endereco?.Id,
            EnderecoHistorico = endereco is null ? null : $"{endereco.Logradouro}, {endereco.Numero} — {endereco.Bairro} {endereco.Complemento}",
            TipoAtendimento = form.TipoAtendimento, NumeroMesa = form.TipoAtendimento == "Mesa" ? form.NumeroMesa : null,
            TaxaEntrega = form.TipoAtendimento == "Delivery" ? form.TaxaEntrega : 0,
            FormaPagamento = form.FormaPagamento, Status = "ABERTO", DataHora = DateTime.UtcNow
        };
        foreach (var item in form.Itens.GroupBy(i => i.ProdutoId))
        {
            var quantidade = item.Sum(i => i.Quantidade);
            Exigir(quantidade <= 999, "Quantidade máxima por produto: 999.");
            var produto = produtos[item.Key];
            Exigir(produto.PrecoAtual is > 0 and <= 999999.99m, "Produto com preço inválido. Solicite revisão do administrador.");
            pedido.Itens.Add(new ItemPedido { ProdutoId = produto.Id, NomeProdutoHistorico = produto.Nome, Quantidade = quantidade, PrecoUnitario = produto.PrecoAtual, SubtotalItem = decimal.Round(produto.PrecoAtual * quantidade, 2) });
        }
        pedido.Subtotal = pedido.Itens.Sum(i => i.SubtotalItem);
        pedido.ValorTotal = pedido.Subtotal + pedido.TaxaEntrega;
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();
        pedido.NumeroComanda = pedido.Id;
        if (form.Finalizar) { await Baixar(pedido, usuario); pedido.Status = "FINALIZADO"; }
        Auditar(usuario, $"Pedido {pedido.Id} criado: {pedido.Status}");
        return pedido.Id;
    });

    async Task Baixar(Pedido pedido, int usuario)
    {
        var ids = pedido.Itens.Select(i => i.ProdutoId).ToList();
        var estoques = await db.EstoqueBebidas.Where(e => ids.Contains(e.ProdutoId)).ToListAsync();
        var consumos = await db.Consumos.Where(c => ids.Contains(c.ProdutoId)).ToListAsync();
        var demandas = new Dictionary<int, decimal>();
        foreach (var item in pedido.Itens)
        {
            var direto = estoques.SingleOrDefault(e => e.ProdutoId == item.ProdutoId);
            if (direto is not null) demandas[direto.Id] = demandas.GetValueOrDefault(direto.Id) + item.Quantidade;
            foreach (var c in consumos.Where(c => c.ProdutoId == item.ProdutoId))
                demandas[c.EstoqueBebidaId] = demandas.GetValueOrDefault(c.EstoqueBebidaId) + c.Quantidade * item.Quantidade;
        }
        foreach (var demanda in demandas.OrderBy(d => d.Key))
        {
            var estoque = await db.EstoqueBebidas.Include(e => e.Produto).SingleAsync(e => e.Id == demanda.Key);
            Exigir(estoque.QuantidadeAtual >= demanda.Value, $"Estoque insuficiente: {estoque.Produto.Nome}.");
            estoque.QuantidadeAtual -= demanda.Value;
            Registrar(estoque, usuario, "VENDA", demanda.Value, "Baixa automática", pedido.Id);
        }
    }
    public Task<int> AlterarStatus(int id, string status, int usuario, bool admin) => Transacao(async () =>
    {
        var p = await db.Pedidos.Include(p => p.Itens).SingleOrDefaultAsync(p => p.Id == id);
        Exigir(p is not null, "Pedido não encontrado.");
        if (p!.Status == status) return p.Id;
        Exigir(p.Status != "CANCELADO", "Pedido cancelado não pode ser reaberto.");
        if (status == "CANCELADO")
        {
            Exigir(admin, "Apenas administrador pode cancelar pedidos.");
            if (p.Status == "FINALIZADO")
            {
                var baixas = await db.Movimentacoes.Where(m => m.PedidoId == id && m.Tipo == "VENDA").ToListAsync();
                foreach (var baixa in baixas)
                {
                    var estoque = await db.EstoqueBebidas.FindAsync(baixa.EstoqueBebidaId);
                    estoque!.QuantidadeAtual += baixa.Quantidade;
                    Registrar(estoque, usuario, "ESTORNO", baixa.Quantidade, "Cancelamento do pedido", id);
                }
            }
        }
        else
        {
            Exigir(p.Status != "FINALIZADO", "Pedido finalizado só pode ser cancelado pelo administrador.");
            var permitido = (p.Status == "ABERTO" && status == "EM PREPARO") ||
                (p.Status == "EM PREPARO" && status == "SAIU PARA ENTREGA" && p.TipoAtendimento == "Delivery") || status == "FINALIZADO";
            Exigir(permitido, "Transição de status inválida.");
            if (status == "FINALIZADO") await Baixar(p, usuario);
        }
        p.Status = status;
        Auditar(usuario, $"Pedido {p.Id}: {status}");
        return p.Id;
    });
    void Registrar(EstoqueBebida estoque, int usuario, string tipo, decimal quantidade, string observacao, int? pedido = null) =>
        db.Movimentacoes.Add(new MovimentacaoEstoque { EstoqueBebidaId = estoque.Id, UsuarioId = usuario, PedidoId = pedido, Tipo = tipo, Quantidade = quantidade, SaldoApos = estoque.QuantidadeAtual, Observacao = observacao });

    public Task<int> Movimentar(MovimentoForm form, int usuario) => Transacao(async () =>
    {
        Exigir(form.Quantidade > 0 && form.Quantidade <= 999999.999m, "Quantidade inválida.");
        Exigir(form.Tipo is "ENTRADA" or "SAIDA", "Movimento inválido.");
        Exigir(decimal.Round(form.Quantidade, 3) == form.Quantidade && decimal.Round(form.QuantidadeMinima, 3) == form.QuantidadeMinima, "Use até três casas decimais nas quantidades.");
        Exigir(await db.Produtos.AnyAsync(p => p.Id == form.ProdutoId), "Produto não encontrado.");
        var estoque = await db.EstoqueBebidas.SingleOrDefaultAsync(e => e.ProdutoId == form.ProdutoId);
        if (estoque is null)
        {
            Exigir(form.Tipo == "ENTRADA", "Cadastre o saldo inicial com uma entrada.");
            estoque = new EstoqueBebida { ProdutoId = form.ProdutoId };
            db.EstoqueBebidas.Add(estoque);
            await db.SaveChangesAsync();
        }
        var novo = estoque.QuantidadeAtual + (form.Tipo == "ENTRADA" ? form.Quantidade : -form.Quantidade);
        Exigir(novo >= 0, "Saída superior ao saldo disponível.");
        estoque.QuantidadeAtual = novo;
        estoque.QuantidadeMinima = form.QuantidadeMinima;
        Registrar(estoque, usuario, form.Tipo, form.Quantidade, form.Observacao);
        Auditar(usuario, $"Estoque {estoque.Id}: {form.Tipo}");
        return estoque.Id;
    });
}

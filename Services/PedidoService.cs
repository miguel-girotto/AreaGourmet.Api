using System.Data;
using AreaGourmet.Data;
using AreaGourmet.Models;
using AreaGourmet.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Services;

public class PedidoService(AppDbContext db)
{
    public async Task<Pedido> Finalizar(PedidoForm form, int usuarioId)
    {
        if (form.ChaveOperacao == Guid.Empty) throw new InvalidOperationException("Operação inválida. Recarregue a página.");
        var anterior = await db.Pedidos.AsNoTracking().FirstOrDefaultAsync(p => p.ChaveOperacao == form.ChaveOperacao);
        if (anterior != null)
        {
            if (anterior.UsuarioId != usuarioId) throw new InvalidOperationException("Operação não pertence a este usuário.");
            return anterior;
        }
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (!await db.Usuarios.AnyAsync(x => x.Id == usuarioId && x.Ativo)) throw new InvalidOperationException("Operador inativo.");
        if (form.TipoAtendimento == "Mesa" && form.MesaNumero is null) throw new InvalidOperationException("Informe o número da mesa.");
        Cliente? cliente = null;
        if (form.ClienteId.HasValue) cliente = await db.Clientes.FirstOrDefaultAsync(x => x.Id == form.ClienteId && x.Ativo)
            ?? throw new InvalidOperationException("Cliente indisponível.");
        Endereco? endereco = null;
        if (form.TipoAtendimento == "Delivery")
        {
            if (cliente is null) throw new InvalidOperationException("Selecione um cliente para entrega.");
            endereco = await db.Enderecos.FirstOrDefaultAsync(x => x.Id == form.EnderecoId && x.ClienteId == cliente.Id && x.Ativo)
                ?? throw new InvalidOperationException("Selecione um endereço válido deste cliente.");
        }
        var pedido = new Pedido
        {
            ChaveOperacao = form.ChaveOperacao,
            UsuarioId = usuarioId,
            TipoAtendimento = form.TipoAtendimento,
            MesaNumero = form.TipoAtendimento == "Mesa" ? form.MesaNumero : null,
            ClienteId = cliente?.Id,
            EnderecoId = endereco?.Id,
            EnderecoEntrega = endereco?.Resumo ?? "",
            TaxaEntrega = form.TipoAtendimento == "Delivery" ? 5m : 0m,
            FormatoPagamento = form.FormatoPagamento
        };
        var itens = form.Itens.GroupBy(x => x.ProdutoId).Select(x => new { Id = x.Key, Qtd = x.Sum(i => i.Quantidade) }).ToList();
        if (itens.Count == 0 || itens.Any(x => x.Qtd is < 1 or > 999)) throw new InvalidOperationException("Quantidade inválida.");
        foreach (var item in itens.OrderBy(x => x.Id))
        {
            var produto = await db.Produtos.SingleOrDefaultAsync(x => x.Id == item.Id && x.Ativo)
                ?? throw new InvalidOperationException("Produto indisponível.");
            if (produto.ControlaEstoque && produto.EstoqueInsumo < item.Qtd) throw new InvalidOperationException($"Estoque insuficiente: {produto.Nome}.");
            if (produto.ControlaEstoque) produto.EstoqueInsumo -= item.Qtd;
            produto.Versao++;
            pedido.Itens.Add(new ItemPedido
            {
                ProdutoId = produto.Id,
                Quantidade = item.Qtd,
                PrecoPraticado = produto.PrecoUnitario,
                NomeProduto = produto.Nome,
                CategoriaPraticada = produto.Categoria
            });
        }
        pedido.ValorTotal = pedido.Itens.Sum(x => x.PrecoPraticado * x.Quantidade) + pedido.TaxaEntrega;
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return pedido;
    }
}

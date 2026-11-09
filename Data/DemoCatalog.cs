using System.Data;
using AreaGourmet.Models;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Data;
public static class DemoCatalog {
    // Exemplos acadêmicos: não representam cardápio ou preços confirmados pelo restaurante.
    public static Produto[] CriarProdutos() => [
        P("Marmita Executiva","Marmitas",18.90m,"meal.jpg"),
        P("Marmita Fitness","Marmitas",17.90m,"fitness.jpg"),
        P("Frango Grelhado","Marmitas",16.50m,"chicken.jpg"),
        P("Feijoada Completa","Marmitas",22.90m,"beans.jpg"),
        P("Suco Natural Laranja","Bebidas",8.90m,"juice.jpg"),
        P("Refrigerante Guaraná","Bebidas",6.50m,"soda.jpg"),
        P("Farofa Crocante","Adicionais",3.50m,"extra.jpg"),
        P("Marmita Caseira Pequena","Marmitas",16.90m,"meal.jpg"),
        P("Marmita Caseira Grande","Marmitas",24.90m,"meal.jpg"),
        P("Strogonoff de Carne","Marmitas",26.90m,"stroganoff.jpg"),
        P("Marmita Vegetariana","Marmitas",21.90m,"vegetarian.jpg"),
        P("Suco de Uva","Bebidas",8.90m,"grape.jpg"),
        P("Coca-Cola Lata","Bebidas",7.50m,"cola.jpg"),
        P("Água Mineral","Bebidas",4m,"water.jpg"),
        P("Purê de Batata","Adicionais",4.50m,"puree.jpg"),
        P("Ovo Frito","Adicionais",3m,"egg.jpg")
    ];
    private static Produto P(string nome,string categoria,decimal preco,string imagem) => new() {
        Nome=nome,Categoria=categoria,PrecoUnitario=preco,Imagem=imagem,EstoqueInsumo=100,ControlaEstoque=true
    };
    public static async Task<int> AdicionarAusentes(AppDbContext db) {
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var nomes=(await db.Produtos.AsNoTracking().Select(x=>x.Nome).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var novos=CriarProdutos().Where(p=>!nomes.Contains(p.Nome)).ToArray();
        db.Produtos.AddRange(novos);await db.SaveChangesAsync();await tx.CommitAsync();return novos.Length;
    }
}

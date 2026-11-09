using AreaGourmet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Data;
public static class Seed {
    public static async Task Run(IServiceProvider services,IConfiguration config){
        var db=services.GetRequiredService<AppDbContext>();var hasher=services.GetRequiredService<IPasswordHasher<Usuario>>();
        if(!await db.Usuarios.AnyAsync()){
            var senha=config["Seed:AdminPassword"];
            if(string.IsNullOrWhiteSpace(senha)||senha.Length<10)throw new InvalidOperationException("Defina Seed__AdminPassword com pelo menos 10 caracteres para o primeiro acesso.");
            var admin=new Usuario{Nome="Administrador",Email=(config["Seed:AdminEmail"]??"admin@areagourmet.local").Trim().ToLowerInvariant(),Perfil="Administrador"};
            admin.SenhaHash=hasher.HashPassword(admin,senha);db.Usuarios.Add(admin);await db.SaveChangesAsync();
        }
        if(!config.GetValue<bool>("Seed:DemoData")||await db.Produtos.AnyAsync())return;
        var produtos=DemoCatalog.CriarProdutos();
        var cliente=new Cliente{Nome="Maria Silva",Telefone="11987654321",Enderecos=[new Endereco{Logradouro="Rua das Flores",Numero="123",Bairro="Centro",Cidade="Assis",CEP="19800000"}]};
        db.Produtos.AddRange(produtos);db.Clientes.Add(cliente);await db.SaveChangesAsync();
        var adminId=await db.Usuarios.Where(x=>x.Perfil=="Administrador").Select(x=>x.Id).FirstAsync();
        for(int mes=0;mes<4;mes++)for(int j=0;j<3;j++){
            var p=produtos[(mes+j)%produtos.Length];var qtd=j+1;p.EstoqueInsumo-=qtd;p.Versao++;
            db.Pedidos.Add(new Pedido{ChaveOperacao=Guid.NewGuid(),DataHora=DateTime.UtcNow.AddMonths(-mes).AddMinutes(-j),UsuarioId=adminId,ClienteId=cliente.Id,TipoAtendimento="Retirada",FormatoPagamento=new[]{"Pix","Cartão","Dinheiro"}[j],ValorTotal=p.PrecoUnitario*qtd,Itens=[new ItemPedido{ProdutoId=p.Id,NomeProduto=p.Nome,CategoriaPraticada=p.Categoria,PrecoPraticado=p.PrecoUnitario,Quantidade=qtd}]});
        }
        await db.SaveChangesAsync();
    }
}

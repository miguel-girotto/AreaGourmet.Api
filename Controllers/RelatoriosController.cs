using AreaGourmet.Data;
using AreaGourmet.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AreaGourmet.Controllers;
[Authorize(Roles="Administrador")]
public class RelatoriosController(AppDbContext db):Controller {
    public async Task<IActionResult> Index(DateTime? inicio,DateTime? fim){
        var hoje=TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")).Date;
        var a=(inicio??new DateTime(hoje.Year,hoje.Month,1)).Date;var b=(fim??hoje).Date;
        if(b<a||b>a.AddYears(2)){ModelState.AddModelError("","Selecione um período válido de até dois anos.");return View(new RelatorioVM{Inicio=a,Fim=b});}
        var tz=TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        var start=TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(a,DateTimeKind.Unspecified),tz);
        var end=TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(b.AddDays(1),DateTimeKind.Unspecified),tz);
        var pedidos=await db.Pedidos.AsNoTracking().Where(x=>x.DataHora>=start&&x.DataHora<end)
            .Include(x=>x.Cliente).Include(x=>x.Usuario).Include(x=>x.Itens).OrderByDescending(x=>x.DataHora).ToListAsync();
        var vm=new RelatorioVM{Inicio=a,Fim=b};
        vm.Vendas=pedidos.Select(p=>new VendaLinha(p.Id,TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(p.DataHora,DateTimeKind.Utc),tz),p.Cliente?.Nome??"Balcão",p.Usuario.Nome,p.TipoAtendimento,p.FormatoPagamento,p.Itens.Sum(i=>i.Quantidade),p.Itens.Sum(i=>i.Quantidade*i.PrecoPraticado),p.TaxaEntrega,p.ValorTotal)).ToList();
        vm.Pagamentos=vm.Vendas.GroupBy(x=>x.Pagamento).Select(g=>new Grupo(g.Key,g.Sum(x=>x.Total))).ToList();
        for(var month=new DateTime(a.Year,a.Month,1);month<=b;month=month.AddMonths(1))vm.Meses.Add(month.ToString("yyyy-MM"));
        var items=pedidos.SelectMany(p=>p.Itens.Select(i=>new{Mes=TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(p.DataHora,DateTimeKind.Utc),tz).ToString("yyyy-MM"),Categoria=i.CategoriaPraticada,Valor=i.Quantidade*i.PrecoPraticado}));
        vm.Pivot=items.GroupBy(x=>x.Categoria).ToDictionary(g=>g.Key,g=>g.GroupBy(x=>x.Mes).ToDictionary(h=>h.Key,h=>h.Sum(x=>x.Valor)));
        return View(vm);
    }
}

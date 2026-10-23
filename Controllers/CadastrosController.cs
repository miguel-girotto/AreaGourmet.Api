using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using AreaGourmet.Api.Services;
using AreaGourmet.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
namespace AreaGourmet.Api.Controllers;
public record CadastroLinha(int Id, string Nome, string Detalhe, decimal? Valor, bool Ativo);
public class CadastrosController(AppDbContext db, Operacoes op) : BaseController
{
    bool Permitido(string tipo, bool escrita = false) => new[] { "Clientes", "Produtos", "Categorias" }.Contains(tipo) &&
        (Admin || tipo == "Clientes" && (!escrita || User.HasClaim("clientes", "sim")));
    async Task Preparar(string tipo)
    {
        ViewBag.Tipo = tipo;
        ViewBag.Categorias = await db.Categorias.Where(c => c.Ativo).OrderBy(c => c.Nome).ToListAsync();
    }
    [HttpGet] public async Task<IActionResult> Index(string tipo = "Clientes", string? q = null, string situacao = "ativos", int? categoriaId = null)
    {
        if (!Permitido(tipo)) return Forbid();
        await Preparar(tipo); ViewBag.Q = q; ViewBag.Situacao = situacao; ViewBag.CategoriaId = categoriaId;
        List<CadastroLinha> rows;
        if (tipo == "Clientes")
        {
            var query = db.Clientes.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q)) { var digits = new string(q.Where(char.IsDigit).ToArray()); query = query.Where(c => c.Nome.Contains(q) || (digits != "" && c.Telefone.Contains(digits))); }
            if (situacao != "todos") query = query.Where(c => c.Ativo == (situacao != "inativos"));
            rows = await query.OrderBy(c => c.Nome).Select(c => new CadastroLinha(c.Id, c.Nome, c.Telefone, null, c.Ativo)).ToListAsync();
        }
        else if (tipo == "Produtos")
        {
            var query = db.Produtos.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Nome.Contains(q));
            if (categoriaId.HasValue) query = query.Where(p => p.CategoriaId == categoriaId);
            if (situacao != "todos") query = query.Where(c => c.Ativo == (situacao != "inativos"));
            rows = await query.OrderBy(p => p.Nome).Select(p => new CadastroLinha(p.Id, p.Nome, p.Categoria.Nome, p.PrecoAtual, p.Ativo)).ToListAsync();
        }
        else
        {
            var query = db.Categorias.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c => c.Nome.Contains(q));
            if (situacao != "todos") query = query.Where(c => c.Ativo == (situacao != "inativos"));
            rows = await query.OrderBy(c => c.Nome).Select(c => new CadastroLinha(c.Id, c.Nome, "Categoria do cardápio", null, c.Ativo)).ToListAsync();
        }
        return View(rows);
    }
    [HttpGet] public async Task<IActionResult> Editar(string tipo, int id = 0)
    {
        if (!Permitido(tipo, true)) return Forbid();
        await Preparar(tipo);
        var form = await Buscar(tipo, id);
        return form is null ? NotFound() : View(form);
    }
    async Task<CadastroForm?> Buscar(string tipo, int id)
    {
        if (id == 0) return new CadastroForm();
        if (tipo == "Clientes") { var c = await db.Clientes.FindAsync(id); return c is null ? null : new CadastroForm { Id = c.Id, Nome = c.Nome, Telefone = c.Telefone, Ativo = c.Ativo }; }
        if (tipo == "Categorias") { var c = await db.Categorias.FindAsync(id); return c is null ? null : new CadastroForm { Id = c.Id, Nome = c.Nome, Ativo = c.Ativo }; }
        var p = await db.Produtos.FindAsync(id); return p is null ? null : new CadastroForm { Id = p.Id, Nome = p.Nome, Ativo = p.Ativo, CategoriaId = p.CategoriaId, PrecoAtual = p.PrecoAtual, DisponivelVenda = p.DisponivelVenda };
    }
    [HttpPost] public async Task<IActionResult> Editar(string tipo, CadastroForm form)
    {
        if (!Permitido(tipo, true)) return Forbid();
        if (tipo == "Clientes" && !Regex.IsMatch(new string((form.Telefone ?? "").Where(char.IsDigit).ToArray()), "^[0-9]{10,11}$")) ModelState.AddModelError("Telefone", "Informe telefone com DDD e 10 ou 11 dígitos.");
        if (tipo == "Produtos" && (!form.CategoriaId.HasValue || !form.PrecoAtual.HasValue)) ModelState.AddModelError("", "Informe categoria e preço.");
        return await Salvar(async () => await op.Transacao(async () =>
        {
            if (tipo == "Clientes")
            {
                var digits = new string(form.Telefone!.Where(char.IsDigit).ToArray());
                Operacoes.Exigir(!await db.Clientes.AnyAsync(c => c.Id != form.Id && c.Telefone == digits), "Já existe cliente com esse telefone.");
                var c = form.Id == 0 ? new Cliente() : await db.Clientes.FindAsync(form.Id);
                Operacoes.Exigir(c is not null, "Cliente não encontrado.");
                c!.Nome = form.Nome.Trim(); c.Telefone = digits; c.Ativo = form.Ativo;
                if (form.Id == 0) db.Clientes.Add(c);
            }
            else if (tipo == "Categorias")
            {
                Operacoes.Exigir(!await db.Categorias.AnyAsync(c => c.Id != form.Id && c.Nome == form.Nome.Trim()), "Já existe categoria com esse nome.");
                var c = form.Id == 0 ? new Categoria() : await db.Categorias.FindAsync(form.Id);
                Operacoes.Exigir(c is not null, "Categoria não encontrada.");
                c!.Nome = form.Nome.Trim(); c.Ativo = form.Ativo;
                if (form.Id == 0) db.Categorias.Add(c);
            }
            else
            {
                Operacoes.Exigir(await db.Categorias.AnyAsync(c => c.Id == form.CategoriaId && c.Ativo), "Escolha uma categoria ativa.");
                Operacoes.Exigir(decimal.Round(form.PrecoAtual!.Value, 2) == form.PrecoAtual.Value, "Use no máximo duas casas decimais no preço.");
                var p = form.Id == 0 ? new Produto() : await db.Produtos.FindAsync(form.Id);
                Operacoes.Exigir(p is not null, "Produto não encontrado.");
                p!.Nome = form.Nome.Trim(); p.CategoriaId = form.CategoriaId!.Value; p.PrecoAtual = form.PrecoAtual.Value; p.Ativo = form.Ativo; p.DisponivelVenda = form.DisponivelVenda;
                if (form.Id == 0) db.Produtos.Add(p);
            }
            op.Auditar(UsuarioId, $"{tipo}: cadastro atualizado ({form.Id})"); return 0;
        }), () => RedirectToAction(nameof(Index), new { tipo }), async () => { await Preparar(tipo); return View(form); });
    }
    [HttpGet] public async Task<IActionResult> Detalhes(string tipo, int id)
    {
        if (!Permitido(tipo)) return Forbid();
        await Preparar(tipo); var form = await Buscar(tipo, id);
        if (form is null || id == 0) return NotFound();
        if (tipo == "Clientes") ViewBag.Enderecos = await db.Enderecos.Where(e => e.ClienteId == id).ToListAsync();
        return View(form);
    }
    [HttpPost] public async Task<IActionResult> Desativar(string tipo, int id)
    {
        if (!Permitido(tipo, true)) return Forbid();
        await op.Transacao(async () =>
        {
            if (tipo == "Clientes") { var c = await db.Clientes.FindAsync(id); if (c is not null) c.Ativo = false; }
            if (tipo == "Categorias") { var c = await db.Categorias.FindAsync(id); if (c is not null) c.Ativo = false; }
            if (tipo == "Produtos") { var c = await db.Produtos.FindAsync(id); if (c is not null) c.Ativo = false; }
            op.Auditar(UsuarioId, $"{tipo}: {id} desativado"); return 0;
        });
        TempData["Sucesso"] = "Cadastro desativado. O histórico foi preservado.";
        return RedirectToAction(nameof(Index), new { tipo });
    }
    [HttpGet] public async Task<IActionResult> Endereco(int clienteId, int id = 0)
    {
        if (!Permitido("Clientes", true)) return Forbid();
        if (!await db.Clientes.AnyAsync(c => c.Id == clienteId && c.Ativo)) return NotFound();
        var e = id == 0 ? null : await db.Enderecos.SingleOrDefaultAsync(e => e.Id == id && e.ClienteId == clienteId);
        if (id != 0 && e is null) return NotFound();
        return View(e is null ? new EnderecoForm { ClienteId = clienteId } : new EnderecoForm { Id = e.Id, ClienteId = e.ClienteId, Logradouro = e.Logradouro, Numero = e.Numero, Bairro = e.Bairro, Complemento = e.Complemento, Rotulo = e.Rotulo });
    }
    [HttpPost] public async Task<IActionResult> Endereco(EnderecoForm form)
    {
        if (!Permitido("Clientes", true)) return Forbid();
        return await Salvar(async () => await op.Transacao(async () =>
        {
            Operacoes.Exigir(await db.Clientes.AnyAsync(c => c.Id == form.ClienteId && c.Ativo), "Cliente indisponível.");
            var e = form.Id == 0 ? new Endereco { ClienteId = form.ClienteId } : await db.Enderecos.SingleOrDefaultAsync(e => e.Id == form.Id && e.ClienteId == form.ClienteId);
            Operacoes.Exigir(e is not null, "Endereço não encontrado.");
            e!.Logradouro = form.Logradouro.Trim(); e.Numero = form.Numero.Trim(); e.Bairro = form.Bairro.Trim(); e.Complemento = form.Complemento?.Trim(); e.Rotulo = form.Rotulo?.Trim();
            if (form.Id == 0) db.Enderecos.Add(e);
            op.Auditar(UsuarioId, $"Endereço do cliente {form.ClienteId} atualizado"); return 0;
        }), () => RedirectToAction(nameof(Detalhes), new { tipo = "Clientes", id = form.ClienteId }), () => Task.FromResult<IActionResult>(View(form)));
    }
}

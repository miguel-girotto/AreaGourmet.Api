using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProdutosController(AppDbContext context)
    {
<<<<<<< Updated upstream
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Produto>>> GetAll()
        => await _context.Produtos
            .Include(p => p.Categoria)
            .Include(p => p.EstoqueBebida)
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Produto>> GetById(int id)
    {
        var produto = await _context.Produtos
            .Include(p => p.Categoria)
            .Include(p => p.EstoqueBebida)
            .FirstOrDefaultAsync(p => p.Id == id);

        return produto is null ? NotFound() : produto;
    }

    [HttpPost]
    public async Task<ActionResult<Produto>> Create(Produto produto)
    {
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = produto.Id }, produto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Produto produto)
    {
        if (id != produto.Id) return BadRequest();

        _context.Entry(produto).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto is null) return NotFound();

        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();
        return NoContent();
=======
        ViewBag.Busca = busca; 
        ViewBag.Categoria = categoria;

        var q = db.Produtos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca)) 
            q = q.Where(x => x.Nome.Contains(busca));

        if (!string.IsNullOrWhiteSpace(categoria)) 
            q = q.Where(x => x.Categoria == categoria);

        return View(await q.OrderBy(x => x.Nome).Take(200).ToListAsync());
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Editar(int? id)
    {
        if (id is null) 
            return View(new Produto()); 
            
        var p = await db.Produtos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id); 
        
        if (p is not null) 
            p.Imagem = ProductImages.Normalizar(p.Imagem); 
        
        return p is null ? NotFound() : View(p);
    }

    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Editar(int id, [Bind("Id,Nome,Categoria,PrecoUnitario,EstoqueInsumo,ControlaEstoque,Ativo,Versao,Imagem")] Produto form)
    {
        if (id != form.Id) 
            return BadRequest();

        if (!ProductImages.Valida(form.Imagem)) 
            ModelState.AddModelError("Imagem", "Imagem inválida.");

        if (!ModelState.IsValid) 
            return View(form);

        form.Imagem = ProductImages.Normalizar(form.Imagem);

        if (id == 0) { 
            form.Versao = 1; 
            db.Produtos.Add(form); 
            }

        else
        {
            var p = await db.Produtos.FindAsync(id); 
            
            if (p is null) 
                return NotFound();

            if (p.Versao != form.Versao) { 
                ModelState.AddModelError("", "Produto alterado por outra operação. Reabra a edição."); 
                return View(form); 
                }
            
            p.Nome = form.Nome; 
            p.Categoria = form.Categoria; 
            p.PrecoUnitario = form.PrecoUnitario; 
            p.EstoqueInsumo = form.EstoqueInsumo;
            p.ControlaEstoque = form.ControlaEstoque; 
            p.Ativo = form.Ativo; 
            p.Imagem = form.Imagem; 
            p.Versao++;
        }
        try { await db.SaveChangesAsync(); } 
        
        catch (DbUpdateConcurrencyException) { 
            ModelState.AddModelError("", "Alteração concorrente. Reabra a edição."); 
            return View(form); 
            }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ImportarDemonstracao()
    {
        try
        {
            var count = await DemoCatalog.AdicionarAusentes(db);
            TempData["Mensagem"] = count == 0 ? "Os exemplos já estão cadastrados. Nenhum produto foi alterado." : 
                $"{count} produtos demonstrativos adicionados. Preços, estoque e pedidos existentes foram preservados.";
        }
        catch (DbUpdateException) { TempData["Mensagem"] = "Não foi possível importar agora. Tente novamente."; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Excluir(int id)
    {
        var p = await db.Produtos.FindAsync(id); 
        
        if (p is null) 
            return NotFound(); 
            
        p.Ativo = false; 
        p.Versao++;

        try { await db.SaveChangesAsync(); } 
        
        catch (DbUpdateConcurrencyException) { 
            TempData["Mensagem"] = "Produto alterado. Tente novamente."; 
            return RedirectToAction(nameof(Index)); 
            }

        TempData["Mensagem"] = "Produto inativado; vendas anteriores preservadas."; 
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Estoque(string? busca)
    {
        ViewBag.Busca = busca; 
        return View(await db.Produtos
                            .AsNoTracking()
                            .Where(x => x.Ativo && x.ControlaEstoque && (busca == null || x.Nome.Contains(busca)))
                            .OrderBy(x => x.EstoqueInsumo)
                            .Take(200)
                            .ToListAsync());
>>>>>>> Stashed changes
    }
}

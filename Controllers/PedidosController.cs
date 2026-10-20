using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PedidosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetAll()
        => await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.Cliente)
            .Include(p => p.Endereco)
            .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> GetById(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.Cliente)
            .Include(p => p.Endereco)
            .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id);

        return pedido is null ? NotFound() : pedido;
    }

    [HttpPost]
    public async Task<ActionResult<Pedido>> Create(Pedido pedido)
    {
        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = pedido.Id }, pedido);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Pedido pedido)
    {
        if (id != pedido.Id) return BadRequest();

        _context.Entry(pedido).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido is null) return NotFound();

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

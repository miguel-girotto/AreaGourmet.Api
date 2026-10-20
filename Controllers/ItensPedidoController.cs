using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItensPedidoController : ControllerBase
{
    private readonly AppDbContext _context;

    public ItensPedidoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemPedido>>> GetAll()
        => await _context.ItensPedido
            .Include(i => i.Produto)
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemPedido>> GetById(int id)
    {
        var item = await _context.ItensPedido
            .Include(i => i.Produto)
            .FirstOrDefaultAsync(i => i.Id == id);

        return item is null ? NotFound() : item;
    }

    [HttpPost]
    public async Task<ActionResult<ItemPedido>> Create(ItemPedido itemPedido)
    {
        _context.ItensPedido.Add(itemPedido);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = itemPedido.Id }, itemPedido);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ItemPedido itemPedido)
    {
        if (id != itemPedido.Id) return BadRequest();

        _context.Entry(itemPedido).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.ItensPedido.FindAsync(id);
        if (item is null) return NotFound();

        _context.ItensPedido.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

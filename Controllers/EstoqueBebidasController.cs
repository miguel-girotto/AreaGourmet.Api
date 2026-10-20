using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstoqueBebidasController : ControllerBase
{
    private readonly AppDbContext _context;

    public EstoqueBebidasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EstoqueBebida>>> GetAll()
        => await _context.EstoqueBebidas
            .Include(e => e.Produto)
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<EstoqueBebida>> GetById(int id)
    {
        var estoque = await _context.EstoqueBebidas
            .Include(e => e.Produto)
            .FirstOrDefaultAsync(e => e.Id == id);

        return estoque is null ? NotFound() : estoque;
    }

    [HttpPost]
    public async Task<ActionResult<EstoqueBebida>> Create(EstoqueBebida estoqueBebida)
    {
        _context.EstoqueBebidas.Add(estoqueBebida);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = estoqueBebida.Id }, estoqueBebida);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, EstoqueBebida estoqueBebida)
    {
        if (id != estoqueBebida.Id) return BadRequest();

        _context.Entry(estoqueBebida).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var estoque = await _context.EstoqueBebidas.FindAsync(id);
        if (estoque is null) return NotFound();

        _context.EstoqueBebidas.Remove(estoque);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

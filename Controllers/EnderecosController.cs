using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnderecosController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnderecosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Endereco>>> GetAll()
        => await _context.Enderecos
            .Include(e => e.Cliente)
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Endereco>> GetById(int id)
    {
        var endereco = await _context.Enderecos
            .Include(e => e.Cliente)
            .FirstOrDefaultAsync(e => e.Id == id);

        return endereco is null ? NotFound() : endereco;
    }

    [HttpPost]
    public async Task<ActionResult<Endereco>> Create(Endereco endereco)
    {
        _context.Enderecos.Add(endereco);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = endereco.Id }, endereco);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Endereco endereco)
    {
        if (id != endereco.Id) return BadRequest();

        _context.Entry(endereco).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var endereco = await _context.Enderecos.FindAsync(id);
        if (endereco is null) return NotFound();

        _context.Enderecos.Remove(endereco);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

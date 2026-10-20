using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsuariosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuario>>> GetAll()
        => await _context.Usuarios.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Usuario>> GetById(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        return usuario is null ? NotFound() : usuario;
    }

    [HttpPost]
    public async Task<ActionResult<Usuario>> Create(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Usuario usuario)
    {
        if (id != usuario.Id) return BadRequest();

        _context.Entry(usuario).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

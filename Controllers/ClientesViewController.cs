using AreaGourmet.Api.Data;
using AreaGourmet.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AreaGourmet.Api.Controllers;

[Route("clientes")]
public class ClientesViewController : Controller
{
    private readonly AppDbContext _context;

    public ClientesViewController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var clientes = await _context.Clientes
            .Include(c => c.Enderecos)
            .OrderByDescending(c => c.Id)
            .ToListAsync();

        return View("~/Views/Clientes/Index.cshtml", clientes);
    }

    [HttpGet("detalhes/{id}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var cliente = await _context.Clientes
            .Include(c => c.Enderecos)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (cliente == null) return NotFound();

        return View("~/Views/Clientes/Details.cshtml", cliente);
    }

    [HttpGet("criar")]
    public IActionResult Create()
    {
        return View("~/Views/Clientes/Create.cshtml");
    }

    [HttpPost("criar")]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        if (ModelState.IsValid)
        {
            cliente.DataCadastro = DateTime.Now;
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Clientes/Create.cshtml", cliente);
    }

    [HttpGet("editar/{id}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        return View("~/Views/Clientes/Edit.cshtml", cliente);
    }

    [HttpPost("editar/{id}")]
    public async Task<IActionResult> Edit(int id, Cliente cliente)
    {
        if (id != cliente.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(cliente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Clientes.AnyAsync(e => e.Id == cliente.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Clientes/Edit.cshtml", cliente);
    }

    [HttpGet("excluir/{id}")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(m => m.Id == id);

        if (cliente == null) return NotFound();

        return View("~/Views/Clientes/Delete.cshtml", cliente);
    }

    [HttpPost("excluir/{id}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}

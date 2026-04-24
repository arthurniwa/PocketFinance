using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketFinance.Core;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace PocketFinance.Web.Controllers;

[Authorize]
public class CadastroContaController : Controller
{
    private readonly AppDbContext _db;

    public CadastroContaController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var usuarioID = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var contas = _db.Contas.Where( c => c.UsuarioId == usuarioID).ToList();

        return View(contas);
    }

    public IActionResult Criar() => View();

    [HttpPost]
    public IActionResult Criar(Conta conta)
    {
        conta.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        _db.Contas.Add(conta);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Editar(int id)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var conta = _db.Contas.FirstOrDefault(c => c.Id == id && c.UsuarioId == usuarioId);

        if(conta == null )
        {
            return NotFound();
        }

        return View(conta);

    }

    [HttpPost]
    public IActionResult Editar( int id, Conta conta)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var contaDb = _db.Contas.FirstOrDefault(c => c.Id == id && c.UsuarioId == usuarioId);

        if(contaDb == null)
        {
            return NotFound();
        }

        contaDb.Nome  = conta.Nome;
        contaDb.Tipo  = conta.Tipo;
        contaDb.Saldo = conta.Saldo;

        _db.SaveChanges();
        return RedirectToAction("Index"); 
    }
}
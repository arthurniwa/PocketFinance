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

    public IActionResult Deletar(int id)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var conta = _db.Contas.FirstOrDefault(c => c.Id == id && c.UsuarioId == usuarioId);

        if(conta != null)
        {
            _db.Contas.Remove(conta);
            _db.SaveChanges();
        }
            
        return RedirectToAction("Index");

    }
    public IActionResult Detalhes(int id)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var conta = _db.Contas.FirstOrDefault( c => c.Id == id && c.UsuarioId == usuarioId);

        if(conta == null) return NotFound();

        var transacoes = _db.Transacoes.Where( t => t.ContaId == id && t.UsuarioId == usuarioId)
        .OrderByDescending( t => t.Data)
        .ToList();

        var entradas = transacoes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor);
        var saidas   = transacoes.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => Math.Abs(t.Valor));

        var gastosPorCategoria = transacoes
        .Where(t => t.Tipo == TipoTransacao.Despesa)
        .GroupBy(t => t.Categoria)
        .Select(g => new { Categoria = g.Key, Valor = Math.Abs(g.Sum(t => t.Valor)) })
        .ToList();

        var evolucao = transacoes
        .OrderBy( t => t.Data)
        .GroupBy( t => t.Data.ToString("dd/MM/yyyy"))
        .Select( g => new { Data = g.Key, Valor = g.Sum( t => t.Valor)})
        .ToList();

        decimal saldoAcumulado = 0;
        var evolucaoLabels = new List<string>();
        var evolucaoValores = new List<decimal>();

        foreach(var dia in evolucao)
        {
            saldoAcumulado += dia.Valor;
            evolucaoLabels.Add(dia.Data);
            evolucaoValores.Add(saldoAcumulado);
        }

        ViewBag.Entradas = entradas;
        ViewBag.Saidas = saidas;
        ViewBag.GraficoLabels = gastosPorCategoria.Select(x => x.Categoria).ToArray();
        ViewBag.EvolucaoLabels = evolucaoLabels.ToArray();
        ViewBag.EvolucaoValores = evolucaoValores.ToArray();


        return View((conta, transacoes));
    }
}
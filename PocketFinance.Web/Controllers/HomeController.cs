using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PocketFinance.Web.Models;
using PocketFinance.Core;
using System.Security.Claims;
using System.Data.Common;

namespace PocketFinance.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _db;

    public HomeController(ILogger<HomeController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        if(User.Identity?.IsAuthenticated == true)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var agora = DateTime.Now;

            ViewBag.SaldoTotal = _db.Transacoes
            .Where(t => t.UsuarioId == meuId)
			.Sum(t => (decimal?) t.Valor) ?? 0;

			ViewBag.EntradasMes = _db.Transacoes
			.Where( t => t.UsuarioId == meuId
			&& t.Tipo == TipoTransacao.Receita
            && t.Data.Month == agora.Month
            && t.Data.Year  == agora.Year)
        	.Sum(t => (decimal?)t.Valor) ?? 0;

			ViewBag.SaidasMes = _db.Transacoes
            .Where(t => t.UsuarioId == meuId
            && t.Tipo == TipoTransacao.Despesa
            && t.Data.Month == agora.Month
            && t.Data.Year  == agora.Year)
            .Sum(t => (decimal?)t.Valor) ?? 0;

            ViewBag.UltimosLancamentos = _db.Transacoes
            .Where(t => t.UsuarioId == meuId)
            .OrderByDescending(t => t.Data)
            .Take(5)
            .ToList();
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

using Microsoft.AspNetCore.Mvc;
using PocketFinance.Core;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PocketFinance.Web.Controllers
{
    [Authorize]
    public class TransacaoController : Controller
    {
        private readonly AppDbContext _db;

        public TransacaoController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transacoes = _db.Transacoes
                .Where(t => t.UsuarioId == meuId)
                .OrderByDescending(t => t.Data)
                .ToList();

            var entradas = transacoes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor);
            var saidas = transacoes.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor);

            ViewBag.Entradas = entradas;
            ViewBag.Saidas = saidas;
            ViewBag.Saldo = entradas + saidas;

            var gastosPorCategoria = transacoes
                .Where(t => t.Tipo == TipoTransacao.Despesa)
                .GroupBy(t => t.Categoria)
                .Select(g => new { Categoria = g.Key, Valor = Math.Abs(g.Sum(t => t.Valor)) })
                .ToList();

            ViewBag.GraficoLabels = gastosPorCategoria.Select(x => x.Categoria).ToArray();
            ViewBag.GraficoValores = gastosPorCategoria.Select(x => x.Valor).ToArray();

            return View(transacoes);
        }

        public IActionResult Editar(int id)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var t = _db.Transacoes.FirstOrDefault(x => x.Id == id && x.UsuarioId == meuId);
            if (t == null) return NotFound();

            t.Valor = Math.Abs(t.Valor);
            return View(t);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Transacao transacao)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existente = _db.Transacoes.FirstOrDefault(t => t.Id == transacao.Id && t.UsuarioId == meuId);
            if (existente == null) return Forbid();

            transacao.Valor = ParseValorMonetario(Request.Form["Valor"].ToString());
            transacao.Valor = Math.Abs(transacao.Valor);
            if (transacao.Tipo == TipoTransacao.Despesa) transacao.Valor *= -1;

            ModelState.Remove("Valor");
            if (ModelState.IsValid)
            {
                existente.Descricao = transacao.Descricao;
                existente.Categoria = transacao.Categoria;
                existente.Valor = transacao.Valor;
                existente.Data = transacao.Data;
                existente.Tipo = transacao.Tipo;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(transacao);
        }

        public IActionResult Criar()
        {
            return View(new Transacao { Data = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Criar(Transacao transacao)
        {
            transacao.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            transacao.Valor = ParseValorMonetario(Request.Form["Valor"].ToString());
            transacao.Valor = Math.Abs(transacao.Valor);
            if (transacao.Tipo == TipoTransacao.Despesa) transacao.Valor *= -1;

            ModelState.Remove("Valor");
            if (ModelState.IsValid)
            {
                _db.Transacoes.Add(transacao);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(transacao);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deletar(int id)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var t = _db.Transacoes.FirstOrDefault(x => x.Id == id && x.UsuarioId == meuId);
            if (t != null)
            {
                _db.Transacoes.Remove(t);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        private static decimal ParseValorMonetario(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return 0;
            texto = texto.Replace("R$", "").Replace(".", "").Trim();
            return decimal.TryParse(texto, out decimal valor) ? valor : 0;
        }
    }
}

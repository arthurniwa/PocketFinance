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

 public IActionResult Index(int? contaId, int? mes, int? ano)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var mesFiltro = mes ?? DateTime.Now.Month;
            var anoFiltro = ano ?? DateTime.Now.Year;

            var todasTransacoes = _db.Transacoes
                .Where
                (
                    t => t.UsuarioId == meuId
                    && t.Data.Month  == mesFiltro
                    && t.Data.Year   == anoFiltro
                )
                .OrderByDescending(t => t.Data)
                .ToList();

            var transacoes = contaId.HasValue
                ? contaId.Value == -1
                ? todasTransacoes.Where(t => t.ContaId == null).ToList()
                : todasTransacoes.Where(t => t.ContaId == contaId.Value).ToList()
                : todasTransacoes;

            var entradas = transacoes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor);
            var saidas   = transacoes.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor);

            ViewBag.Entradas = entradas;
            ViewBag.Saidas   = saidas;

            var mesAntData = new DateTime(anoFiltro, mesFiltro, 1).AddMonths(-1);

            var transacoesAnt = _db.Transacoes
                .Where(t => t.UsuarioId == meuId
                         && t.Data.Month == mesAntData.Month
                         && t.Data.Year  == mesAntData.Year)
                .ToList();

            var transacoesAntFiltradas = contaId.HasValue
                ? contaId.Value == -1
                    ? transacoesAnt.Where(t => t.ContaId == null).ToList()
                    : transacoesAnt.Where(t => t.ContaId == contaId.Value).ToList()
                : transacoesAnt;

            ViewBag.EntradasAnt = transacoesAntFiltradas
            .Where(t => t.Tipo == TipoTransacao.Receita)
            .Sum(t => t.Valor);

            ViewBag.SaidasAnt = transacoesAntFiltradas
            .Where(t => t.Tipo == TipoTransacao.Despesa)
            .Sum(t => t.Valor);


            var fimDoMes = new DateTime(anoFiltro, mesFiltro, DateTime.DaysInMonth(anoFiltro, mesFiltro), 23, 59, 59);

            decimal saldoReal;
            if (contaId.HasValue && contaId.Value != -1)
            {
                saldoReal = _db.Transacoes
                    .Where(t => t.UsuarioId == meuId && t.ContaId == contaId.Value && t.Data <= fimDoMes)
                    .Sum(t => (decimal?)t.Valor) ?? 0;
            }
            else if (contaId.HasValue && contaId.Value == -1)
            {
                saldoReal = _db.Transacoes
                    .Where(t => t.UsuarioId == meuId && t.ContaId == null && t.Data <= fimDoMes)
                    .Sum(t => (decimal?)t.Valor) ?? 0;
            }
            else
            {
                saldoReal = _db.Transacoes
                    .Where(t => t.UsuarioId == meuId && t.Data <= fimDoMes)
                    .Sum(t => (decimal?)t.Valor) ?? 0;
            }

            ViewBag.Saldo = saldoReal;


            var gastosPorCategoria = transacoes
                .Where(t => t.Tipo == TipoTransacao.Despesa)
                .GroupBy(t => t.Categoria)
                .Select(g => new { Categoria = g.Key, Valor = Math.Abs(g.Sum(t => t.Valor)) })
                .ToList();

            ViewBag.GraficoLabels  = gastosPorCategoria.Select(x => x.Categoria).ToArray();
            ViewBag.GraficoValores = gastosPorCategoria.Select(x => x.Valor).ToArray();

            ViewBag.Contas    = _db.Contas.Where(c => c.UsuarioId == meuId).ToList();
            ViewBag.ContaId   = contaId;
            ViewBag.Mes       = mesFiltro;
            ViewBag.Ano       = anoFiltro;

            return View(transacoes);
        }

        public IActionResult Editar(int id)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var t = _db.Transacoes.FirstOrDefault(x => x.Id == id && x.UsuarioId == meuId);
            if (t == null) return NotFound();

            ViewBag.Contas = _db.Contas.Where(c => c.UsuarioId == meuId).ToList();
            CarregarCategorias(meuId);
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
                if (existente.ContaId.HasValue)
                {
                    var conta = _db.Contas.FirstOrDefault(c => c.Id == existente.ContaId.Value);
                    if (conta != null)
                    {
                        conta.Saldo = _db.Transacoes.Where(t => t.ContaId == conta.Id).Sum(t => t.Valor);
                        _db.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }

            return View(transacao);
        }

        public IActionResult Criar()
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            ViewBag.Contas = _db.Contas.Where(c => c.UsuarioId == meuId).ToList();
            CarregarCategorias(meuId);
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

                if (transacao.ContaId.HasValue)
                {
                    var conta = _db.Contas.FirstOrDefault(c => c.Id == transacao.ContaId.Value);

                    if(conta != null)
                    {
                        conta.Saldo = _db.Transacoes.Where(t => t.ContaId == conta.Id).Sum(t => t.Valor);
                        _db.SaveChanges();
                    }
                }

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
                var contaId = t.ContaId;
                _db.Transacoes.Remove(t);
                _db.SaveChanges();

                if (contaId.HasValue)
                {
                    var conta = _db.Contas.FirstOrDefault(c => c.Id == contaId.Value);
                    if (conta != null)
                    {
                        conta.Saldo = _db.Transacoes.Where(t2 => t2.ContaId == conta.Id).Sum(t2 => t2.Valor);
                        _db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("Index");
        }

        private void CarregarCategorias(string meuId)
        {
            if (!_db.Categorias.Any(c => c.UsuarioId == meuId))
            {
                var padroes = new[] { "Alimentação", "Transporte", "Moradia", "Lazer", "Contas", "Saúde", "Educação", "Salário", "Extra" };
                foreach (var nome in padroes)
                    _db.Categorias.Add(new Categoria { Nome = nome, UsuarioId = meuId });
                _db.SaveChanges();
            }
            ViewBag.Categorias = _db.Categorias.Where(c => c.UsuarioId == meuId).OrderBy(c => c.Nome).ToList();
        }
        private static decimal ParseValorMonetario(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return 0;
            texto = texto.Replace("R$", "").Replace(".", "").Trim();
            return decimal.TryParse(texto, out decimal valor) ? valor : 0;
        }
    }
}

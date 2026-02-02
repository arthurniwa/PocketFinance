using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using PocketFinance.Core;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace PocketFinance.Web.Controllers
{
    [Authorize]
    public class TransacaoController : Controller
    {
        public IActionResult Index(DateTime? mes, string busca)
        {
            using var db = new AppDbContext();
            
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var dataBase = mes ?? DateTime.Now;

            var inicioMes = new DateTime(dataBase.Year, dataBase.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            var query = db.Transacoes.AsQueryable();

            query = query.Where( t=> t.UsuarioId == usuarioId);

            query = query.Where(t => t.Data >= inicioMes && t.Data <= fimMes);

            if (!string.IsNullOrEmpty(busca))
            {
                
                query = query.Where(t => t.Descricao.Contains(busca) || 
                                        t.Categoria.Contains(busca));
                
                ViewBag.BuscaAtual = busca;
            }

            var lista = query.OrderByDescending(t => t.Data).ToList();

            var entradas = lista.Where(t => t.EhReceita).Sum(t => t.Valor);

            var saidas = lista.Where(t => !t.EhReceita).Sum(t => t.Valor);

            var saldo = entradas - saidas;


            ViewBag.Entradas = entradas;
            ViewBag.Saidas = saidas;
            ViewBag.Saldo = saldo;
            ViewBag.MesAtual = inicioMes;
            ViewBag.TotalEntradas = entradas;
            ViewBag.TotalSaidas = saidas;            

            return View(lista);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Transacao transacao)
        {
            if (ModelState.IsValid)
            {
                using var db = new AppDbContext();

                transacao.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (transacao.Data == DateTime.MinValue)
                {
                    transacao.Data = DateTime.Now;
                }

                db.Transacoes.Add(transacao);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(transacao);
        }

        public IActionResult Editar(int id)
        {
            using var db = new AppDbContext();

            var item = db.Transacoes.Find(id);

            if (item == null) return NotFound();

            return View("Criar", item);
        }

        [HttpPost]
        public IActionResult Editar(Transacao transacao)
        {
            if (ModelState.IsValid)
            {
                using var db = new AppDbContext();

                transacao.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                db.Transacoes.Update(transacao);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            
            return View("Criar", transacao);
        }

        public IActionResult Deletar(int id)
        {
            using var db = new AppDbContext();

            var item = db.Transacoes.Find(id);

            if (item != null)
            {
                db.Transacoes.Remove(item);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}

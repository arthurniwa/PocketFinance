using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using PocketFinance.Core;
using System.Linq;


namespace PocketFinance.Web.Controllers
{
    public class TransacaoController : Controller
    {
        public IActionResult Index(DateTime? mes)
        {
            using var db = new AppDbContext();
            
            var dataBase = mes ?? DateTime.Now;

            var inicioMes = new DateTime(dataBase.Year, dataBase.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            var lista = db.Transacoes
                  .Where(t => t.Data >= inicioMes && t.Data <= fimMes)
                  .OrderByDescending(t => t.Data)
                  .ToList();
            
            var entradas = lista.Where(t => t.EhReceita).Sum(t => t.Valor);

            var saidas = lista.Where(t => !t.EhReceita).Sum(t => t.Valor);

            var saldo = entradas - saidas;


            ViewBag.Entradas = lista.Where(t => t.EhReceita).Sum(t => t.Valor);
            ViewBag.Saidas = lista.Where(t => !t.EhReceita).Sum(t => t.Valor);
            ViewBag.Saldo = ViewBag.Entradas - ViewBag.Saidas;

            ViewBag.MesAtual = inicioMes;

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












































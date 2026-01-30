using Microsoft.AspNetCore.Mvc;
using PocketFinance.Core;
using System.Linq;


namespace PocketFinance.Web.Controllers
{
    public class TransacaoController : Controller
    {
        public IActionResult Index()
        {
            using var db = new AppDbContext();

            var lista = db.Transacoes.OrderByDescending(t => t.Data).ToList();

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












































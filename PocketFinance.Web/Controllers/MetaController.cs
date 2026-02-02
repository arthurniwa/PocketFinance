using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PocketFinance.Core;
using System.Security.Claims;
using System.Linq;

namespace PocketFinance.Web.Controllers
{
    [Authorize]
    public class MetaController : Controller
    {
        public IActionResult Index()
        {
            using var db = new AppDbContext();

            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var minhasMetas = db.Metas.Where( m => m.UsuarioId == meuId).ToList();

            return View(minhasMetas);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Meta meta)
        {
            using var db = new AppDbContext();

            meta.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            db.Metas.Add(meta);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Depositar(int id, decimal valor)
        {
            using var db = new AppDbContext();
            var meta = db.Metas.Find(id);

            if(meta != null && meta.UsuarioId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                db.Metas.Remove(meta);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
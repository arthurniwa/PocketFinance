using Microsoft.AspNetCore.Mvc;
using PocketFinance.Core;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PocketFinance.Web.Controllers
{
    [Authorize]
    public class MetaController : Controller
    {
        public IActionResult Index()
        {
            using var db = new AppDbContext();
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var metas = db.Metas
                .Where(m => m.UsuarioId == meuId)
                .ToList();

            return View(metas);
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

        public IActionResult Editar(int id)
        {
            using var db = new AppDbContext();
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var meta = db.Metas.FirstOrDefault(m => m.Id == id && m.UsuarioId == meuId);
            
            if (meta == null) return NotFound();

            return View(meta);
        }

        [HttpPost]
        public IActionResult Editar(Meta meta)
        {
            using var db = new AppDbContext();
            meta.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            db.Metas.Update(meta);
            db.SaveChanges();
            
            return RedirectToAction("Index");
        }

        public IActionResult Deletar(int id)
        {
            using var db = new AppDbContext();
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var meta = db.Metas.FirstOrDefault(m => m.Id == id && m.UsuarioId == meuId);
            if (meta != null)
            {
                db.Metas.Remove(meta);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Depositar(int id, string valor)
        {
            using var db = new AppDbContext();
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var meta = db.Metas.FirstOrDefault(m => m.Id == id && m.UsuarioId == meuId);

            if (meta != null && !string.IsNullOrEmpty(valor))
            {
                valor = valor.Replace(".", ""); 
                valor = valor.Replace("R$", "").Trim();

                if (decimal.TryParse(valor, out decimal valorDecimal))
                {
                    meta.ValorAtual += valorDecimal;
                    db.Metas.Update(meta);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using PocketFinance.Core;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PocketFinance.Web.Controllers
{
    [Authorize]
    public class MetaController : Controller
    {
        private readonly AppDbContext _db;

        public MetaController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var metas = _db.Metas
                .Where(m => m.UsuarioId == meuId)
                .OrderBy(m => m.DataAlvo)
                .ToList();

            return View(metas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Depositar(int id, decimal valorExtra)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var meta = _db.Metas.FirstOrDefault(m => m.Id == id && m.UsuarioId == meuId);

            if (meta != null && valorExtra > 0)
            {
                meta.ValorAtual += valorExtra;
                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Criar()
        {
            return View(new Meta { DataAlvo = DateTime.Now.AddMonths(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Criar(Meta meta)
        {
            meta.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                _db.Metas.Add(meta);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(meta);
        }

        public IActionResult Editar(int id)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var meta = _db.Metas.FirstOrDefault(m => m.Id == id && m.UsuarioId == meuId);
            if (meta == null) return NotFound();
            return View(meta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Meta meta)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existente = _db.Metas.FirstOrDefault(m => m.Id == meta.Id && m.UsuarioId == meuId);
            if (existente == null) return Forbid();

            if (ModelState.IsValid)
            {
                existente.Nome = meta.Nome;
                existente.ValorMeta = meta.ValorMeta;
                existente.DataAlvo = meta.DataAlvo;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(meta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deletar(int id)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var meta = _db.Metas.FirstOrDefault(m => m.Id == id && m.UsuarioId == meuId);

            if (meta != null)
            {
                _db.Metas.Remove(meta);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using PocketFinance.Core;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PocketFinance.Web.Controllers
{
    [Authorize]
    public class CategoriaController : Controller
    {
        private readonly AppDbContext _db;

        public CategoriaController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var categorias = _db.Categorias
                .Where(c => c.UsuarioId == meuId)
                .OrderBy(c => c.Nome)
                .ToList();
            return View(categorias);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Criar(string nome)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(nome))
            {
                _db.Categorias.Add(new Categoria { Nome = nome.Trim(), UsuarioId = meuId! });
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deletar(int id)
        {
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cat = _db.Categorias.FirstOrDefault(c => c.Id == id && c.UsuarioId == meuId);
            if (cat != null)
            {
                _db.Categorias.Remove(cat);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
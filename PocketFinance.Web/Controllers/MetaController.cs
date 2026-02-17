using Microsoft.AspNetCore.Mvc;
using PocketFinance.Core;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System;

namespace PocketFinance.Web.Controllers
{
    [Authorize]
    public class MetaController : Controller
    {
        public IActionResult Index()
        {
            using var db = new AppDbContext();
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Traz as metas ordenadas pela data alvo (as mais urgentes primeiro)
            var metas = db.Metas
                .Where(m => m.UsuarioId == meuId)
                .OrderBy(m => m.DataAlvo)
                .ToList();

            return View(metas);
        }

        // --- AÇÃO RÁPIDA: DEPOSITAR ---
        [HttpPost]
        public IActionResult Depositar(int id, decimal valorExtra)
        {
            using var db = new AppDbContext();
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var meta = db.Metas.FirstOrDefault(m => m.Id == id && m.UsuarioId == meuId);
            
            if (meta != null && valorExtra > 0)
            {
                meta.ValorAtual += valorExtra; // Soma o valor novo
                db.SaveChanges();
            }
            
            return RedirectToAction("Index");
        }

        public IActionResult Criar()
        {
            return View(new Meta { DataAlvo = DateTime.Now.AddMonths(1) });
        }

        [HttpPost]
        public IActionResult Criar(Meta meta)
        {
            using var db = new AppDbContext();
            meta.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (ModelState.IsValid)
            {
                db.Metas.Add(meta);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(meta);
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
            // Garante que não perde o dono da meta
            meta.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (ModelState.IsValid)
            {
                db.Metas.Update(meta);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(meta);
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
    }
}
using Microsoft.AspNetCore.Mvc;
using PocketFinance.Core;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System;

namespace PocketFinance.Web.Controllers
{
    [Authorize]
    public class TransacaoController : Controller
    {
        public IActionResult Index()
        {
            using var db = new AppDbContext();
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transacoes = db.Transacoes
                .Where(t => t.UsuarioId == meuId)
                .OrderByDescending(t => t.Data)
                .ToList();

            var entradas = transacoes.Where(t => t.Valor > 0).Sum(t => t.Valor);
            
            var saidas = transacoes.Where(t => t.Valor < 0).Sum(t => t.Valor);

            ViewBag.Entradas = entradas;
            ViewBag.Saidas = saidas; 
            ViewBag.Saldo = entradas + saidas; 
            return View(transacoes);
        }

        public IActionResult Criar()
        {
            
            return View(new Transacao { Data = DateTime.Now });
        }

        [HttpPost]
        public IActionResult Criar(Transacao transacao)
        {
            using var db = new AppDbContext();
            
            
            transacao.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            
            string valorTexto = Request.Form["Valor"].ToString(); 
            
            if (!string.IsNullOrEmpty(valorTexto))
            {
                
                valorTexto = valorTexto.Replace("R$", "").Replace(".", "").Trim();
                
                if (decimal.TryParse(valorTexto, out decimal valorFinal))
                {
                    transacao.Valor = valorFinal;
                }
            }

            
            transacao.Valor = Math.Abs(transacao.Valor); 
            
            
            if (transacao.Tipo == 0) 
            {
                transacao.Valor = transacao.Valor * -1;
            }

            
            ModelState.Remove("Valor"); 

            if (ModelState.IsValid)
            {
                db.Transacoes.Add(transacao);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(transacao);
        }

        public IActionResult Deletar(int id)
        {
            using var db = new AppDbContext();
            var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var t = db.Transacoes.FirstOrDefault(x => x.Id == id && x.UsuarioId == meuId);
            
            if (t != null)
            {
                db.Transacoes.Remove(t);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using PocketFinance.Core;


namespace PocketFinance.Web.Controllers
{
	[Authorize]
	public class IAController : Controller
	{
		private readonly AppDbContext _db;
		private readonly IConfiguration _config;
		private readonly IHttpClientFactory _httpFactory;

		public IAController(AppDbContext db, IConfiguration config, IHttpClientFactory httpFactory)
		{
			_db = db;
			_config = config;
			_httpFactory = httpFactory;
		}

		public IActionResult Index() => View();

		[HttpPost]
		public async Task<IActionResult> Chat([FromBody] ChatRequest request)
		{
			var meuId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var agora = DateTime.Now;
			var cultura = new System.Globalization.CultureInfo("pt-BR");

			 var saldoTotal = _db.Transacoes
                .Where(t => t.UsuarioId == meuId)
                .Sum(t => (decimal?)t.Valor) ?? 0;

            var transacoesMes = _db.Transacoes
                .Where(t => t.UsuarioId == meuId
                         && t.Data.Month == agora.Month
                         && t.Data.Year  == agora.Year)
                .OrderByDescending(t => t.Data)
                .ToList();

            var entradasMes = transacoesMes.Where(t => t.Tipo == TipoTransacao.Receita).Sum(t => t.Valor);
            var saidasMes   = transacoesMes.Where(t => t.Tipo == TipoTransacao.Despesa).Sum(t => t.Valor);

            var metas = _db.Metas.Where(m => m.UsuarioId == meuId).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Você é um assistente financeiro pessoal integrado ao PocketFinance.");
            sb.AppendLine("Responda sempre em português brasileiro, de forma clara e objetiva.");
            sb.AppendLine($"Data atual: {agora:dd/MM/yyyy}");
            sb.AppendLine($"Saldo total (todas as contas): {saldoTotal.ToString("C", cultura)}");
            sb.AppendLine($"Receitas em {agora.ToString("MMMM/yyyy", cultura)}: {entradasMes.ToString("C", cultura)}");
            sb.AppendLine($"Despesas em {agora.ToString("MMMM/yyyy", cultura)}: {Math.Abs(saidasMes).ToString("C", cultura)}");

            if (transacoesMes.Any())
            {
                sb.AppendLine($"\nTransações deste mês:");
                foreach (var t in transacoesMes)
                {
                    var tipo = t.Tipo == TipoTransacao.Receita ? "Receita" : "Despesa";
                    sb.AppendLine($"- {t.Data:dd/MM} | {tipo} | {t.Categoria} | {t.Descricao} | {Math.Abs(t.Valor).ToString("C", cultura)}");
                }
            }

            if (metas.Any())
            {
                sb.AppendLine("\nMetas:");
                foreach (var m in metas)
                    sb.AppendLine($"- {m.Nome}: {m.ValorAtual.ToString("C", cultura)} de {m.ValorMeta.ToString("C", cultura)} ({m.PorcentagemConcluida}%) — prazo: {m.DataAlvo:dd/MM/yyyy}");
            }

            var messages = new List<object> { new { role = "system", content = sb.ToString() } };
            foreach (var msg in request.History ?? new List<ChatMessage>())
                messages.Add(new { role = msg.Role, content = msg.Content });
            messages.Add(new { role = "user", content = request.Message });

            var http = _httpFactory.CreateClient();
            http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config["Groq:ApiKey"]}");

            var payload = new { model = "llama-3.3-70b-versatile", messages, max_tokens = 1024 };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await http.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
                return Json(new { error = "Erro ao chamar a IA." });

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var answer = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Json(new { answer });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = "";
        public List<ChatMessage>? History { get; set; }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    
	}
	
}


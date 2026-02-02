using System;
using System.ComponentModel.DataAnnotations;

namespace PocketFinance.Core
{
    public class Meta
    {
        public int Id {get; set; }

        [Required(ErrorMessage = "De um nome a sua meta")]

        public string Nome {get; set; } = string.Empty;

        [Range(0.01, 99999999, ErrorMessage = "Defina um valor alvo")]

        public decimal ValorAlvo {get; set; }

        public decimal ValorAtual {get; set;}

        public DateTime DataLimite {get; set; }

        public string UsuarioId {get; set; } = string.Empty;

        public int PorcentagemConcluida
        {
            get
            {
                if (ValorAlvo == 0) return 0;

                var pct = (int) ((ValorAtual / ValorAlvo) * 100);

                return pct > 100 ? 100 : pct;
            }
        }
    }
}
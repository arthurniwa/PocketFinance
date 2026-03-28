using System;
using System.ComponentModel.DataAnnotations;

namespace PocketFinance.Core
{
    public class Meta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "De um nome a sua meta")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quanto você quer juntar?")]
        public decimal ValorMeta { get; set; }

        public decimal ValorAtual { get; set; }

        [Required(ErrorMessage = "Quando você quer realizar?")]
        public DateTime DataAlvo { get; set; }

        public string UsuarioId { get; set; } = string.Empty;

        public int PorcentagemConcluida
        {
            get 
            {
                if (ValorMeta == 0) return 0;

                var pct = (int)((ValorAtual / ValorMeta) * 100);

                return pct > 100 ? 100 : pct;
            }
        }
    }
}
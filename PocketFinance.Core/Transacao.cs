using System;
using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;



namespace PocketFinance.Core
{
    public class Transacao
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        public string Descricao { get; set; }

        public string Categoria { get; set; } = string.Empty;


        [Required]
        public decimal Valor { get; set; }
        
        public DateTime Data { get; set; } = DateTime.Now;

        public TipoTransacao Tipo { get; set; }

        

        public string UsuarioId { get; set;} = string.Empty;
    }
}
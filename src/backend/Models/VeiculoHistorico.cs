using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking.Api.Models
{
    public class VeiculoHistorico
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid VeiculoId { get; set; }
        [ForeignKey(nameof(VeiculoId))]
        public Veiculo? Veiculo { get; set; }

        [Required]
        public Guid ClienteId { get; set; }
        [ForeignKey(nameof(ClienteId))]
        public Cliente? Cliente { get; set; }

        [Required]
        public DateTime Inicio { get; set; }

        public DateTime? Fim { get; set; }
    }
}

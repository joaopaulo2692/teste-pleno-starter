using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking.Api.Models
{
    public class VeiculoHistorico
    {
        public Guid Id { get; set; }
        public Guid VeiculoId { get; set; }
        public Guid ClienteId { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime? Fim { get; set; }

        public virtual Veiculo Veiculo { get; set; } = null!;
        public virtual Cliente Cliente { get; set; } = null!;
    }

}

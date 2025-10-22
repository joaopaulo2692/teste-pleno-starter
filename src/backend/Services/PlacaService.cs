
using System.Text.RegularExpressions;

namespace Parking.Api.Services
{
    public class PlacaService
    {
        // Intencionalmente simples (candidato deve robustecer)
        public string Sanitizar(string? placa)
        {
            var p = Regex.Replace(placa ?? "", "[^A-Za-z0-9]", "").ToUpperInvariant();
            return p;
        }

        // TODO: melhorar regras para Mercosul - aceitar AAA1A23 e similares
        //public bool EhValida(string placa)
        //{
        //    return Regex.IsMatch(placa, "^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$");
        //}
        public bool EhValida(string placa)
        {
            // Mercosul: ABC1D23
            var mercosul = @"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$";
            // Antigo: ABC1234
            var antigo = @"^[A-Z]{3}[0-9]{4}$";

            return Regex.IsMatch(placa, mercosul) || Regex.IsMatch(placa, antigo);
        }

    }
}

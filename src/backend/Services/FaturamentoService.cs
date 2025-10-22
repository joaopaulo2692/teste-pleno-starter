
using Microsoft.EntityFrameworkCore;
using Parking.Api.Data;
using Parking.Api.Models;

namespace Parking.Api.Services
{
    public class FaturamentoService
    {
        private readonly AppDbContext _db;
        public FaturamentoService(AppDbContext db) => _db = db;

        // BUG proposital: usa dono ATUAL do veículo em vez do dono NA DATA DE CORTE
        //public async Task<List<Fatura>> GerarAsync(string competencia, CancellationToken ct = default)
        //{
        //    // competencia formato yyyy-MM
        //    var part = competencia.Split('-');
        //    var ano = int.Parse(part[0]);
        //    var mes = int.Parse(part[1]);
        //    var ultimoDia = DateTime.DaysInMonth(ano, mes);
        //    var corte = new DateTime(ano, mes, ultimoDia, 23, 59, 59, DateTimeKind.Utc);

        //    var mensalistas = await _db.Clientes
        //        .Where(c => c.Mensalista)
        //        .AsNoTracking()
        //        .ToListAsync(ct);

        //    var criadas = new List<Fatura>();

        //    foreach (var cli in mensalistas)
        //    {
        //        var existente = await _db.Faturas
        //            .FirstOrDefaultAsync(f => f.ClienteId == cli.Id && f.Competencia == competencia, ct);
        //        if (existente != null) continue; // idempotência simples

        //        var veiculosAtuaisDoCliente = await _db.Veiculos
        //            .Where(v => v.ClienteId == cli.Id)
        //            .Select(v => v.Id)
        //            .ToListAsync(ct);

        //        var fat = new Fatura
        //        {
        //            Competencia = competencia,
        //            ClienteId = cli.Id,
        //            Valor = cli.ValorMensalidade ?? 0m,
        //            Observacao = "BUG: usando dono atual do veículo"
        //        };

        //        foreach (var id in veiculosAtuaisDoCliente)
        //            fat.Veiculos.Add(new FaturaVeiculo { FaturaId = fat.Id, VeiculoId = id });

        //        _db.Faturas.Add(fat);
        //        criadas.Add(fat);
        //    }

        //    await _db.SaveChangesAsync(ct);
        //    return criadas;
        //}
        public async Task<List<Fatura>> GerarAsync(string competencia, CancellationToken ct = default)
        {
            // competencia no formato yyyy-MM
            var part = competencia.Split('-');
            var ano = int.Parse(part[0]);
            var mes = int.Parse(part[1]);
            var ultimoDia = DateTime.DaysInMonth(ano, mes);
            var primeiroDiaMes = new DateTime(ano, mes, 1);
            var ultimoDiaMes = new DateTime(ano, mes, ultimoDia, 23, 59, 59, DateTimeKind.Utc);

            // pega todos os clientes mensalistas
            var mensalistas = await _db.Clientes
                .Where(c => c.Mensalista)
                .AsNoTracking()
                .ToListAsync(ct);

            var criadas = new List<Fatura>();

            foreach (var cli in mensalistas)
            {
                // idempotência: ignora se fatura já existe
                var existente = await _db.Faturas
                    .FirstOrDefaultAsync(f => f.ClienteId == cli.Id && f.Competencia == competencia, ct);
                if (existente != null) continue;

                // pega histórico de veículos do cliente no mês
                var veiculosHist = await _db.VeiculosHistorico
                    .Where(h => h.ClienteId == cli.Id &&
                                h.Inicio <= ultimoDiaMes &&
                                (h.Fim == null || h.Fim >= primeiroDiaMes))
                    .Include(h => h.Veiculo)
                    .ToListAsync(ct);

                if (!veiculosHist.Any()) continue;

                var fat = new Fatura
                {
                    Competencia = competencia,
                    ClienteId = cli.Id,
                    Valor = 0m,
                    Observacao = "Fatura proporcional"
                };

                foreach (var vh in veiculosHist)
                {
                    // calcula vigência dentro do mês
                    var inicio = vh.Inicio > primeiroDiaMes ? vh.Inicio : primeiroDiaMes;
                    var fim = (vh.Fim.HasValue && vh.Fim < ultimoDiaMes) ? vh.Fim.Value : ultimoDiaMes;

                    var dias = (fim - inicio).Days + 1;
                    var diasNoMes = DateTime.DaysInMonth(ano, mes);

                    // soma valor proporcional
                    fat.Valor += (cli.ValorMensalidade ?? 0m) * dias / diasNoMes;

                    // adiciona veículo à fatura
                    fat.Veiculos.Add(new FaturaVeiculo { FaturaId = fat.Id, VeiculoId = vh.VeiculoId });
                }

                _db.Faturas.Add(fat);
                criadas.Add(fat);
            }

            await _db.SaveChangesAsync(ct);
            return criadas;
        }
    }
}

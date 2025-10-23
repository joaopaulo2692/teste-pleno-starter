using Microsoft.EntityFrameworkCore;
using Parking.Api.Data;
using Parking.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parking.Api.Services
{
    public class FaturamentoService
    {
        private readonly AppDbContext _db;

        public FaturamentoService(AppDbContext db) => _db = db;

        public async Task<List<Fatura>> GerarAsync(string competencia, CancellationToken ct = default)
        {
            // 1. Definição da Janela de Corte
            var part = competencia.Split('-');
            if (part.Length != 2 || !int.TryParse(part[0], out var ano) || !int.TryParse(part[1], out var mes))
            {
                throw new ArgumentException("A competência deve estar no formato 'yyyy-MM'.");
            }

            var diasNoMes = DateTime.DaysInMonth(ano, mes);

            // CORRIGIDO: Explicitamente definido como DateTimeKind.Utc para compatibilidade com PostgreSQL/Npgsql
            var primeiroDiaMes = new DateTime(ano, mes, 1, 0, 0, 0, DateTimeKind.Utc);
            var ultimoDiaMes = new DateTime(ano, mes, diasNoMes, 23, 59, 59, DateTimeKind.Utc);

            // 2. Busca de Clientes Mensalistas
            var mensalistas = await _db.Clientes
                .Where(c => c.Mensalista)
                .AsNoTracking()
                .ToListAsync(ct);

            var criadas = new List<Fatura>();

            // 3. Processamento e Faturamento por Cliente
            foreach (var cli in mensalistas)
            {
                // Idempotência: ignora se fatura já existe
                var existente = await _db.Faturas
                    .FirstOrDefaultAsync(f => f.ClienteId == cli.Id && f.Competencia == competencia, ct);
                if (existente != null) continue;

                try
                {
                    // Consulta o histórico de veículos
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

                    // SOLUÇÃO PARA O ERRO DE RASTREAMENTO: Usar HashSet para garantir VeiculoId único.
                    var veiculosFaturados = new HashSet<Guid>();

                    // 4. Cálculo da Proporcionalidade
                    foreach (var vh in veiculosHist)
                    {
                        // 4.1. Define início e fim da vigência dentro do mês
                        var inicioVigencia = vh.Inicio.Date > primeiroDiaMes.Date
                            ? vh.Inicio.Date
                            : primeiroDiaMes.Date;

                        var fimVigencia = ultimoDiaMes.Date;
                        if (vh.Fim.HasValue && vh.Fim.Value.Date < ultimoDiaMes.Date)
                        {
                            fimVigencia = vh.Fim.Value.Date;
                        }

                        // 4.2. Cálculo dos dias
                        var dias = (int)(fimVigencia - inicioVigencia).TotalDays + 1;

                        if (dias <= 0) continue;

                        // 4.3. Cálculo e Acúmulo do Valor Proporcional
                        decimal valorMensalidade = cli.ValorMensalidade ?? 0m;
                        decimal valorProporcional = valorMensalidade * ((decimal)dias / diasNoMes);

                        fat.Valor += valorProporcional;

                        // 4.4. Adiciona o VeiculoId ao set de rastreamento de IDs únicos.
                        veiculosFaturados.Add(vh.VeiculoId);
                    }

                    // 5. Adiciona os Veiculos à fatura APENAS UMA VEZ.
                    // Isso impede que o EF Core rastreie duas entidades FaturaVeiculo com a mesma chave composta.
                    foreach (var veiculoId in veiculosFaturados)
                    {
                        fat.Veiculos.Add(new FaturaVeiculo { FaturaId = fat.Id, VeiculoId = veiculoId });
                    }

                    // Adiciona a fatura
                    _db.Faturas.Add(fat);
                    criadas.Add(fat);
                }
                catch (Exception ex)
                {
                    // Tratamento de Erro Isolado
                    Console.WriteLine($"ERRO ao faturar cliente {cli.Nome} ({cli.Id}) na competência {competencia}: {ex.Message}");
                    continue;
                }
            }

            // 6. Persistência (Salva todas as faturas criadas)
            await _db.SaveChangesAsync(ct);

            return criadas;
        }
    }
}
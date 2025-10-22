
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.Api.Data;
using Parking.Api.Dtos;
using Parking.Api.Models;
using Parking.Api.Services;

namespace Parking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VeiculosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly PlacaService _placa;
        public VeiculosController(AppDbContext db, PlacaService placa) { _db = db; _placa = placa; }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] Guid? clienteId = null)
        {
            var q = _db.Veiculos.AsQueryable();
            if (clienteId.HasValue) q = q.Where(v => v.ClienteId == clienteId.Value);
            var list = await q.OrderBy(v => v.Placa).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VeiculoCreateDto dto)
        {
            var placa = _placa.Sanitizar(dto.Placa);
            if (!_placa.EhValida(placa)) return BadRequest("Placa inválida.");
            if (await _db.Veiculos.AnyAsync(v => v.Placa == placa)) return Conflict("Placa já existe.");

            var v = new Veiculo { Placa = placa, Modelo = dto.Modelo, Ano = dto.Ano, ClienteId = dto.ClienteId };
            _db.Veiculos.Add(v);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = v.Id }, v);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var v = await _db.Veiculos.FindAsync(id);
            return v == null ? NotFound() : Ok(v);
        }

        // BUG propositado: não invalida/atualiza nada no front; candidato deve ajustar no front (React Query) ou aqui (retornar entidade e orientar)
        //[HttpPut("{id:guid}")]
        //public async Task<IActionResult> Update(Guid id, [FromBody] VeiculoUpdateDto dto)
        //{
        //    var v = await _db.Veiculos.FindAsync(id);
        //    if (v == null) return NotFound();
        //    var placa = _placa.Sanitizar(dto.Placa);
        //    if (!_placa.EhValida(placa)) return BadRequest("Placa inválida.");
        //    if (await _db.Veiculos.AnyAsync(x => x.Placa == placa && x.Id != id)) return Conflict("Placa já existe.");

        //    v.Placa = placa;
        //    v.Modelo = dto.Modelo;
        //    v.Ano = dto.Ano;
        //    v.ClienteId = dto.ClienteId; // troca de cliente permitida
        //    await _db.SaveChangesAsync();
        //    return Ok(v);
        //}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] VeiculoUpdateDto dto)
        {
            // Busca o veículo
            var veiculo = await _db.Veiculos.FindAsync(id);
            if (veiculo == null)
                return NotFound(new { message = "Veículo não encontrado." });

            // Sanitiza e valida placa
            var placa = _placa.Sanitizar(dto.Placa);
            if (!_placa.EhValida(placa))
                return BadRequest(new { message = "Placa inválida." });

            var cliente = await _db.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                return NotFound(new { message = "Cliente não encontrado." });

            // Verifica duplicidade de placa
            if (await _db.Veiculos.AnyAsync(v => v.Placa == placa && v.Id != id))
                return Conflict(new { message = "Já existe outro veículo com esta placa." });

            // Verifica se o cliente existe
            if (!await _db.Clientes.AnyAsync(c => c.Id == dto.ClienteId))
                return BadRequest(new { message = "Cliente informado não existe." });

            // Atualiza campos parciais
            veiculo.Cliente = cliente;
            veiculo.ClienteId = cliente.Id;
            veiculo.Placa = placa;
            if (!string.IsNullOrEmpty(dto.Modelo))
                veiculo.Modelo = dto.Modelo;
            if (dto.Ano.HasValue)
                veiculo.Ano = dto.Ano.Value;

            // Atualiza histórico se o cliente mudou
            if (veiculo.ClienteId != dto.ClienteId)
            {
                // Fecha histórico anterior, se existir
                var ultimoHist = await _db.VeiculosHistorico
                    .Where(h => h.VeiculoId == veiculo.Id && h.Fim == null)
                    .FirstOrDefaultAsync();

                if (ultimoHist != null)
                {
                    ultimoHist.Fim = DateTime.UtcNow;
                }
                else
                {
                    // Nenhum histórico anterior: cria histórico inicial do cliente antigo
                    _db.VeiculosHistorico.Add(new VeiculoHistorico
                    {
                        VeiculoId = veiculo.Id,
                        ClienteId = veiculo.ClienteId,
                        Inicio = veiculo.DataInclusao,
                        Fim = DateTime.UtcNow
                    });
                }

                // Cria novo histórico para o novo cliente
                _db.VeiculosHistorico.Add(new VeiculoHistorico
                {
                    VeiculoId = veiculo.Id,
                    ClienteId = dto.ClienteId,
                    Inicio = DateTime.UtcNow
                });

                veiculo.ClienteId = dto.ClienteId;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Veículo atualizado com sucesso",
                veiculo
            });
        }



        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var v = await _db.Veiculos.FindAsync(id);
            if (v == null) return NotFound();
            _db.Veiculos.Remove(v);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

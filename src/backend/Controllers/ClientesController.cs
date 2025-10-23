
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.Api.Data;
using Parking.Api.Dtos;
using Parking.Api.Models;

namespace Parking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ClientesController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10, [FromQuery] string? filtro = null, [FromQuery] string mensalista = "all")
        {
            var q = _db.Clientes.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filtro))
                q = q.Where(c => c.Nome.Contains(filtro));
            if (mensalista == "true") q = q.Where(c => c.Mensalista);
            if (mensalista == "false") q = q.Where(c => !c.Mensalista);

            var total = await q.CountAsync();
            var itens = await q
                .OrderBy(c => c.Nome)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .ToListAsync();
            return Ok(new { total, itens });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClienteCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                // 1. Encontra o primeiro erro de validação
                var primeiroErro = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .FirstOrDefault()?
                    .ErrorMessage;

                // 2. Retorna 400 Bad Request contendo a string pura
                if (primeiroErro != null)
                {
                    // Retorna o status 400 com a string como corpo da resposta.
                    // O Content-Type será text/plain ou application/json com uma string, dependendo
                    // do middleware, mas o resultado é uma string simples.
                    return BadRequest($"{primeiroErro}");
                }
            }
        
            var existe = await _db.Clientes.AnyAsync(c => c.Nome == dto.Nome && c.Telefone == dto.Telefone);
            if (existe) return Conflict("Cliente já existe.");

            var c = new Cliente
            {
                Nome = dto.Nome,
                Telefone = dto.Telefone,
                Endereco = dto.Endereco,
                Mensalista = dto.Mensalista,
                ValorMensalidade = dto.ValorMensalidade,
            };
            _db.Clientes.Add(c);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = c.Id }, c);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var c = await _db.Clientes.Include(x => x.Veiculos).FirstOrDefaultAsync(x => x.Id == id);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ClienteUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                // 1. Encontra o primeiro erro de validação
                var primeiroErro = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .FirstOrDefault()?
                    .ErrorMessage;

                // 2. Retorna 400 Bad Request contendo a string pura
                if (primeiroErro != null)
                {
                    // Retorna o status 400 com a string como corpo da resposta.
                    // O Content-Type será text/plain ou application/json com uma string, dependendo
                    // do middleware, mas o resultado é uma string simples.
                    return BadRequest($"{primeiroErro}");
                }
            }
            var c = await _db.Clientes.FindAsync(id);


            //var temNomeETelefone = await _db.Clientes.AnyAsync(x => x.Nome == dto.Nome && x.Telefone == dto.Telefone);

            //if (temNomeETelefone) return Conflict("Já existe um cliente com esse nome e telefone.");

            var temNomeETelefoneEmOutroCliente = await _db.Clientes
                .AnyAsync(x => x.Id != id && x.Nome == dto.Nome && x.Telefone == dto.Telefone);

            if (temNomeETelefoneEmOutroCliente)
            {
                return Conflict("Já existe outro cliente com esse nome e telefone.");
            }

            if (c == null) return NotFound();
            c.Nome = dto.Nome;
            c.Telefone = dto.Telefone;
            c.Endereco = dto.Endereco;
            c.Mensalista = dto.Mensalista;
            c.ValorMensalidade = dto.ValorMensalidade;
            await _db.SaveChangesAsync();
            return Ok(c);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var c = await _db.Clientes.FindAsync(id);
            if (c == null) return NotFound();
            var temVeiculos = await _db.Veiculos.AnyAsync(v => v.ClienteId == id);
            if (temVeiculos) return BadRequest("Cliente possui veículos associados. Transfira ou remova antes.");
            _db.Clientes.Remove(c);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

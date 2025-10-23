
using System.ComponentModel.DataAnnotations;

namespace Parking.Api.Dtos
{
    //public record ClienteCreateDto(string Nome, string? Telefone, string? Endereco, bool Mensalista, decimal? ValorMensalidade);
    public record ClienteCreateDto(
    [Required(ErrorMessage = "O Nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O Nome deve ter no máximo 100 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O Telefone é obrigatório.")]
    [StringLength(20, ErrorMessage = "O Telefone deve ter no máximo 20 caracteres.")]
    string? Telefone,

    [Required(ErrorMessage = "O Endereço é obrigatório.")]
    [StringLength(200, ErrorMessage = "O Endereço deve ter no máximo 200 caracteres.")]
    string? Endereco,

    bool Mensalista,

    // O valor deve ser positivo (e será obrigatório se Mensalista for true - veremos na validação avançada)
    [Range(0.0, double.MaxValue, ErrorMessage = "O Valor da Mensalidade deve ser positivo ou zero.")]
    decimal? ValorMensalidade
);
    //public record ClienteUpdateDto(string Nome, string? Telefone, string? Endereco, bool Mensalista, decimal? ValorMensalidade);

    public record ClienteUpdateDto(
    [Required(ErrorMessage = "O Nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O Nome deve ter no máximo 100 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O Telefone é obrigatório.")]
    [StringLength(20, ErrorMessage = "O Telefone deve ter no máximo 20 caracteres.")]
    string? Telefone,

    [Required(ErrorMessage = "O Endereço é obrigatório.")]
    [StringLength(200, ErrorMessage = "O Endereço deve ter no máximo 200 caracteres.")]
    string? Endereco,

    bool Mensalista,

    // O valor deve ser positivo (e será obrigatório se Mensalista for true - veremos na validação avançada)
    [Range(0.0, double.MaxValue, ErrorMessage = "O Valor da Mensalidade deve ser positivo ou zero.")]
    decimal? ValorMensalidade
);
}

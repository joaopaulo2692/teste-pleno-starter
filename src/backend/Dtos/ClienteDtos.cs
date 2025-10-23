
using System.ComponentModel.DataAnnotations;

namespace Parking.Api.Dtos
{
    //public record ClienteCreateDto(string Nome, string? Telefone, string? Endereco, bool Mensalista, decimal? ValorMensalidade);
    public record ClienteCreateDto(
    [Required(ErrorMessage = "O Nome � obrigat�rio.")]
    [StringLength(100, ErrorMessage = "O Nome deve ter no m�ximo 100 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O Telefone � obrigat�rio.")]
    [StringLength(20, ErrorMessage = "O Telefone deve ter no m�ximo 20 caracteres.")]
    string? Telefone,

    [Required(ErrorMessage = "O Endere�o � obrigat�rio.")]
    [StringLength(200, ErrorMessage = "O Endere�o deve ter no m�ximo 200 caracteres.")]
    string? Endereco,

    bool Mensalista,

    // O valor deve ser positivo (e ser� obrigat�rio se Mensalista for true - veremos na valida��o avan�ada)
    [Range(0.0, double.MaxValue, ErrorMessage = "O Valor da Mensalidade deve ser positivo ou zero.")]
    decimal? ValorMensalidade
);
    //public record ClienteUpdateDto(string Nome, string? Telefone, string? Endereco, bool Mensalista, decimal? ValorMensalidade);

    public record ClienteUpdateDto(
    [Required(ErrorMessage = "O Nome � obrigat�rio.")]
    [StringLength(100, ErrorMessage = "O Nome deve ter no m�ximo 100 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O Telefone � obrigat�rio.")]
    [StringLength(20, ErrorMessage = "O Telefone deve ter no m�ximo 20 caracteres.")]
    string? Telefone,

    [Required(ErrorMessage = "O Endere�o � obrigat�rio.")]
    [StringLength(200, ErrorMessage = "O Endere�o deve ter no m�ximo 200 caracteres.")]
    string? Endereco,

    bool Mensalista,

    // O valor deve ser positivo (e ser� obrigat�rio se Mensalista for true - veremos na valida��o avan�ada)
    [Range(0.0, double.MaxValue, ErrorMessage = "O Valor da Mensalidade deve ser positivo ou zero.")]
    decimal? ValorMensalidade
);
}

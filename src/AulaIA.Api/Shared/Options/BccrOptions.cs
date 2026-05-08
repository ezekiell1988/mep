using System.ComponentModel.DataAnnotations;

namespace AulaIA.Api.Shared.Options;

/// <summary>
/// Credenciales y parámetros para el web service SOAP del BCCR.
/// Referencia: https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx
/// Registro gratuito: https://gee.bccr.fi.cr/Indicadores/Suscripciones/
/// TODOS los campos son obligatorios por el servicio; si falta alguno retorna Nothing.
/// </summary>
public sealed class BccrOptions
{
    public const string Section = "Bccr";

    /// <summary>Token de suscripción obtenido al registrarse en el portal del BCCR.</summary>
    [Required]
    public required string Token { get; init; }

    /// <summary>Correo electrónico del suscriptor registrado en el portal del BCCR.</summary>
    [Required]
    public required string CorreoElectronico { get; init; }

    /// <summary>Nombre de la persona o institución suscrita (usado como identificador en el servicio).</summary>
    [Required]
    public required string Nombre { get; init; }

    /// <summary>
    /// Indicar si se desean subniveles del indicador: "S" = Sí, "N" = No.
    /// Para el tipo de cambio (318) siempre usar "N".
    /// </summary>
    public string SubNiveles { get; init; } = "N";

    /// <summary>
    /// Indicador económico del tipo de cambio de venta USD/CRC.
    /// 318 = Tipo de cambio de venta, dólares de los Estados Unidos.
    /// </summary>
    public int IndicadorDolar { get; init; } = 318;
}

using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using AulaIA.Api.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace AulaIA.Api.Features.Suscripciones.Jobs;

/// <summary>
/// Consulta el tipo de cambio de venta USD/CRC del BCCR (indicador 318) y lo guarda en exchange_rates.
/// Se ejecuta diariamente a las 6 AM hora CR (11 AM UTC en verano / 12 PM UTC en invierno).
/// Referencia: https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx
/// </summary>
public sealed class UpdateExchangeRateJob(
    AulaIADbContext db,
    IHttpClientFactory http,
    ILlmAuditService audit,
    ILogger<UpdateExchangeRateJob> log)
{
    // Indicador 318 = Tipo de cambio de venta, dólares de los Estados Unidos
    private const string IndicadorVenta = "318";
    private const string ServiceUrl = "https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx";

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        audit.LogEvent("UpdateExchangeRateJob", "Iniciando", $"date={today}");

        try
        {
            var tc = await FetchTipoCambioAsync(today, ct);

            var exists = await db.ExchangeRates.AnyAsync(r => r.Date == today, ct);
            if (!exists)
            {
                db.ExchangeRates.Add(new ExchangeRate
                {
                    Date = today,
                    UsdToCrc = tc,
                    Source = "BCCR"
                });
                await db.SaveChangesAsync(ct);
            }

            audit.LogEvent("UpdateExchangeRateJob", "Completado", $"✅ TC={tc:N2} CRC/USD para {today}");
            log.LogInformation("Tipo de cambio actualizado: {Tc} CRC/USD para {Date}", tc, today);
        }
        catch (Exception ex)
        {
            audit.LogError("UpdateExchangeRateJob", $"❌ Falló para {today}", ex);
            log.LogError(ex, "Error actualizando tipo de cambio BCCR");
            throw;
        }
    }

    private async Task<decimal> FetchTipoCambioAsync(DateOnly date, CancellationToken ct)
    {
        var fechaStr = date.ToString("dd/MM/yyyy");
        var soapBody = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                           xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                           xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ObtenerIndicadoresEconomicos xmlns="http://ws.sdde.bccr.fi.cr">
                  <Indicador>{IndicadorVenta}</Indicador>
                  <FechaInicio>{fechaStr}</FechaInicio>
                  <FechaFinal>{fechaStr}</FechaFinal>
                  <Nombre>AulaIA</Nombre>
                  <SubNiveles>N</SubNiveles>
                </ObtenerIndicadoresEconomicos>
              </soap:Body>
            </soap:Envelope>
            """;

        using var client = http.CreateClient("bccr");
        using var request = new HttpRequestMessage(HttpMethod.Post, ServiceUrl);
        request.Content = new StringContent(soapBody, System.Text.Encoding.UTF8, "text/xml");
        request.Headers.Add("SOAPAction", "http://ws.sdde.bccr.fi.cr/ObtenerIndicadoresEconomicos");

        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(ct);
        return ParseTipoCambio(xml);
    }

    private static decimal ParseTipoCambio(string soapXml)
    {
        var doc = XDocument.Parse(soapXml);
        // El BCCR devuelve el TC en un nodo NUM_VALOR dentro del XML embebido en el resultado
        var result = doc.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "ObtenerIndicadoresEconomicosResult")
            ?.Value;

        if (string.IsNullOrEmpty(result))
            throw new InvalidOperationException("BCCR no devolvió resultado.");

        // El resultado es un XML embebido: <BCCRWS>...<NUM_VALOR>630.40</NUM_VALOR>...
        var inner = XDocument.Parse(result);
        var valor = inner.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "NUM_VALOR")
            ?.Value;

        if (string.IsNullOrEmpty(valor))
            throw new InvalidOperationException("No se encontró NUM_VALOR en la respuesta del BCCR.");

        return decimal.Parse(valor.Replace(",", "."),
            System.Globalization.CultureInfo.InvariantCulture);
    }
}

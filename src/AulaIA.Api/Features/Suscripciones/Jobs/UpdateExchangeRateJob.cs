using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using AulaIA.Api.Shared.Services;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    IOptions<BccrOptions> bccrOpts,
    ILlmAuditService audit,
    ILogger<UpdateExchangeRateJob> log)
{
    private const string ServiceUrl = "https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx";

    public async Task ExecuteAsync(PerformContext? ctx, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        ctx?.WriteLine($"[1/4] Iniciando para {today}");
        log.LogInformation("[1/4] UpdateExchangeRateJob iniciando para {Date}", today);
        audit.LogEvent("UpdateExchangeRateJob", "Iniciando", $"date={today}");

        try
        {
            ctx?.WriteLine($"[2/4] Consultando BCCR indicador {bccrOpts.Value.IndicadorDolar}...");
            log.LogInformation("[2/4] Consultando BCCR (indicador {Indicador})...", bccrOpts.Value.IndicadorDolar);
            var tc = await FetchTipoCambioAsync(today, ctx, ct);

            if (tc is null)
            {
                const string msg = "BCCR no devolvió tipo de cambio. Verificar credenciales o disponibilidad del servicio.";
                ctx?.WriteLine($"⚠️ {msg}");
                log.LogError(msg);
                audit.LogEvent("UpdateExchangeRateJob", "Error", $"❌ {msg}");
                throw new InvalidOperationException(msg);
            }

            ctx?.WriteLine($"[3/4] TC={tc.Value:N2} CRC/USD obtenido. Verificando duplicado en BD...");
            log.LogInformation("[3/4] TC={Tc:N2} CRC/USD obtenido. Verificando duplicado en BD...", tc.Value);
            var exists = await db.ExchangeRates.AnyAsync(r => r.Date == today, ct);
            if (exists)
            {
                ctx?.WriteLine($"[3/4] Registro para {today} ya existe. Se omite inserción.");
                log.LogInformation("[3/4] Registro para {Date} ya existe en exchange_rates. Se omite inserción.", today);
            }
            else
            {
                ctx?.WriteLine($"[4/4] Guardando TC={tc.Value:N2} en exchange_rates...");
                log.LogInformation("[4/4] Guardando TC={Tc:N2} para {Date} en exchange_rates...", tc.Value, today);
                db.ExchangeRates.Add(new ExchangeRate
                {
                    Date = today,
                    UsdToCrc = tc.Value,
                    Source = "BCCR"
                });
                await db.SaveChangesAsync(ct);
                ctx?.WriteLine($"[4/4] ✅ TC={tc.Value:N2} CRC/USD guardado para {today}");
                log.LogInformation("[4/4] ✅ Tipo de cambio guardado: {Tc:N2} CRC/USD para {Date}", tc.Value, today);
            }

            audit.LogEvent("UpdateExchangeRateJob", "Completado", $"✅ TC={tc.Value:N2} CRC/USD para {today}");
        }
        catch (Exception ex)
        {
            ctx?.WriteLine($"❌ Falló: {ex.Message}");
            audit.LogError("UpdateExchangeRateJob", $"❌ Falló para {today}", ex);
            log.LogError(ex, "Error actualizando tipo de cambio BCCR para {Date}", today);
            throw;
        }
    }

    private async Task<decimal?> FetchTipoCambioAsync(DateOnly date, PerformContext? ctx, CancellationToken ct)
    {
        var opts = bccrOpts.Value;
        var fechaStr = date.ToString("dd/MM/yyyy");
        var soapBody = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                           xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                           xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ObtenerIndicadoresEconomicos xmlns="http://ws.sdde.bccr.fi.cr">
                  <Indicador>{opts.IndicadorDolar}</Indicador>
                  <FechaInicio>{fechaStr}</FechaInicio>
                  <FechaFinal>{fechaStr}</FechaFinal>
                  <Nombre>{opts.Nombre}</Nombre>
                  <SubNiveles>{opts.SubNiveles}</SubNiveles>
                  <CorreoElectronico>{opts.CorreoElectronico}</CorreoElectronico>
                  <Token>{opts.Token}</Token>
                </ObtenerIndicadoresEconomicos>
              </soap:Body>
            </soap:Envelope>
            """;

        using var client = http.CreateClient("bccr");
        using var request = new HttpRequestMessage(HttpMethod.Post, ServiceUrl);
        request.Content = new StringContent(soapBody, System.Text.Encoding.UTF8, "text/xml");
        request.Headers.Add("SOAPAction", "http://ws.sdde.bccr.fi.cr/ObtenerIndicadoresEconomicos");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await client.SendAsync(request, ct);
        sw.Stop();
        ctx?.WriteLine($"BCCR respondió HTTP {(int)response.StatusCode} en {sw.ElapsedMilliseconds}ms");
        log.LogInformation("BCCR respondió {StatusCode} en {ElapsedMs}ms para {Date}",
            (int)response.StatusCode, sw.ElapsedMilliseconds, date);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            ctx?.WriteLine($"BCCR HTTP {(int)response.StatusCode}. Body: {body[..Math.Min(body.Length, 200)]}");
            log.LogWarning("BCCR HTTP {StatusCode} para {Date}. Body: {Body}",
                (int)response.StatusCode, date, body[..Math.Min(body.Length, 400)]);
            return null;
        }

        var xml = await response.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(xml))
        {
            ctx?.WriteLine("BCCR devolvió respuesta vacía.");
            log.LogWarning("BCCR devolvió respuesta vacía para {Date}", date);
            return null;
        }

        ctx?.WriteLine($"BCCR respuesta ({xml.Length} chars): {xml[..Math.Min(xml.Length, 500)]}");
        log.LogInformation("BCCR XML recibido ({Length} chars): {Xml}", xml.Length, xml[..Math.Min(xml.Length, 1000)]);

        try
        {
            var valor = ParseTipoCambio(xml);
            ctx?.WriteLine($"Parse OK: {valor:N2} CRC/USD");
            log.LogInformation("BCCR XML parseado exitosamente: {Valor:N2} CRC/USD", valor);
            return valor;
        }
        catch (Exception ex) when (ex is System.Xml.XmlException or InvalidOperationException)
        {
            ctx?.WriteLine($"Contenido no analizable: {xml[..Math.Min(xml.Length, 200)]}");
            log.LogWarning("BCCR devolvió contenido no analizable para {Date}. Inicio: {Content}",
                date, xml[..Math.Min(xml.Length, 200)]);
            return null;
        }
    }

    private static decimal ParseTipoCambio(string soapXml)
    {
        var doc = XDocument.Parse(soapXml);

        // El BCCR devuelve el TC como descendiente directo del SOAP en un nodo NUM_VALOR
        // dentro de un diffgram. No hace falta un segundo parse — buscamos en el árbol ya cargado.
        var valor = doc.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "NUM_VALOR")
            ?.Value;

        if (string.IsNullOrEmpty(valor))
            throw new InvalidOperationException("BCCR no devolvió NUM_VALOR en la respuesta.");

        return decimal.Parse(valor.Replace(",", "."),
            System.Globalization.CultureInfo.InvariantCulture);
    }
}

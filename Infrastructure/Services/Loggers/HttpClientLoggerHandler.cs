using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Infrastructure.Services.Loggers
{

    public class HttpClientLoggerHandler : DelegatingHandler
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger = Log.ForContext<HttpClientLoggerHandler>();
        public HttpClientLoggerHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            dynamic log = new System.Dynamic.ExpandoObject();
            string? uri = request.RequestUri?.AbsoluteUri;
            try
            {
                string? body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

                DateTime timeStamp = DateTime.UtcNow;

                request.Headers.Add("accept", "application/json");

                // Nota: ServicePointManager está obsoleto. Si necesitas deshabilitar la validación de certificados SSL,
                // configura HttpClientHandler.ServerCertificateCustomValidationCallback al crear el HttpClient.
                // ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                Stopwatch timer = new();
                timer.Start();
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
                timer.Stop();
                string? responseContent = response.Content == null ? null : await response.Content.ReadAsStringAsync(cancellationToken);

                log.Url = uri;
                log.Headers = JsonSerializer.Serialize(request.Headers.ToDictionary(h => h.Key, h => h.Value));
                log.Body = body;
                log.Method = request.Method.Method;
                log.StatusCode = response.StatusCode;
                log.ReasonPhrase = response.ReasonPhrase;
                log.Response = responseContent;
                log.RequestDate = DateTime.UtcNow;
                log.TimeElapsed = $"{timer.ElapsedMilliseconds}ms";

                _logger.Information($"log ===> {JsonSerializer.Serialize(log)}");

                _ = response.EnsureSuccessStatusCode();

                return response;

            }
            catch (HttpRequestException ex)
            {
                _logger.Error($"Error consumiendo servicio externo: request ===> {JsonSerializer.Serialize(log)} ===> EXCEPTION: {ex}");

                throw ex.StatusCode switch
                {
                    HttpStatusCode.NotFound => new NotFoundException("No se encontro el recurso.", ex),
                    HttpStatusCode.BadRequest => new BadRequestException("Error en la petición.", ex),
                    HttpStatusCode.Unauthorized => new UnAuthorizedException("Acceso denegado", ex),
                    HttpStatusCode.InternalServerError => new BadRequestException($"Error en servidor servicio externo", ex),
                    _ => new BadRequestException("Error desconocido", ex),
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Error logging request: request ===> {JsonSerializer.Serialize(log)} ===> EXCEPTION: {ex}");
                throw new BadRequestException(ex.Message, ex);
            }
        }
    }
}

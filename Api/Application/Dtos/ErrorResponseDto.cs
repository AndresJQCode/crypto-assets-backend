namespace Api.Application.Dtos
{
    /// <summary>
    /// DTO para respuestas de error en formato JSON
    /// </summary>
    internal sealed class ErrorResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }

        public ErrorResponseDto(string message, int statusCode, string? details = null, string? traceId = null)
        {
            Message = message;
            StatusCode = statusCode;
            Details = details;
            TraceId = traceId;
        }
    }
}

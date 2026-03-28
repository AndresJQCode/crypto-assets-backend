namespace Infrastructure.Idempotency;

public class ClientRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Time { get; set; }
}

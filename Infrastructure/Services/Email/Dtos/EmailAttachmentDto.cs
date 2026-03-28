namespace Infrastructure.Services.Email.Dtos;

public class EmailAttachmentDto
{
    public required Stream Content { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
}

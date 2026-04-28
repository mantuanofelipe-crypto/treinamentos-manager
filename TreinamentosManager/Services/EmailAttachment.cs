namespace TreinamentosManager.Services
{
    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string? ContentId { get; set; }
        public bool Inline => !string.IsNullOrWhiteSpace(ContentId);
    }
}

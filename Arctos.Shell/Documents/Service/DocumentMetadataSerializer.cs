    using System.Text;
    using System.Text.Json;

    namespace Arctos.Shell.Documents
{
    public static class DocumentMetadataSerializer
    {
        public static string Encode(DocumentMetadata metadata)
        {
            var json = JsonSerializer.Serialize(metadata);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public static DocumentMetadata? Decode(string base64)
        {
            try
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                return JsonSerializer.Deserialize<DocumentMetadata>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}

namespace Application.Clients.Imports.Models
{
    public class ClientImportOptions
    {
        public string RootDirectory { get; set; } = "ClientImports";
        public string PendingDirectoryName { get; set; } = "Pending";
        public string ProcessingDirectoryName { get; set; } = "Processing";
        public string ProcessedDirectoryName { get; set; } = "Processed";
        public string FailedDirectoryName { get; set; } = "Failed";
        public int QueueCapacity { get; set; } = 100;
        public long MaxFileSizeInBytes { get; set; } = 5 * 1024 * 1024;
    }
}

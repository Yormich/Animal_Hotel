namespace Animal_Hotel.Services
{
    public interface IIFileProvider
    {
        public List<string> DefaultFileNames { get; }
        public Task UploadFileToServer(IFormFile file, string uniqueFileName);

        public Task RemoveFileFromServer(string fileName);

        public bool IsFileExtensionSupported(string fileName);
    }
}

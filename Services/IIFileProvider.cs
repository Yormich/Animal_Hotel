namespace Animal_Hotel.Services
{
    public interface IIFileProvider
    {

        public Task UploadFileToServer(IFormFile file, string uniqueFileName);
        public bool IsFileExtensiionSupported(string fileName);
    }
}

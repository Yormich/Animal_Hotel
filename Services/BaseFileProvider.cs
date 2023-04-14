using Animal_Hotel.Services;
using Microsoft.Extensions.Primitives;

namespace Animal_Hotel.Services
{
    public class BaseFileProvider : IIFileProvider
    {
        private readonly IConfiguration _config;
        public BaseFileProvider(IConfiguration config)
        {
            _config = config;
        }

        public bool IsFileExtensiionSupported(string fileName)
        {
            return _config["ImageExtensions"]!.Contains(Path.GetExtension(fileName));
        }

        public async Task UploadFileToServer(IFormFile file, string uniqueFileName)
        {
            string uniquePath = @$"{Directory.GetCurrentDirectory()}\wwwroot\img\user_images\{uniqueFileName}";
            Console.WriteLine(Directory.GetCurrentDirectory());
            using (var fs = new FileStream(uniquePath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fs);
            }
        }
    }
}

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

        public List<string> DefaultFileNames => new List<string>()
        {
            "UnsetClient.png",
            "UnsetEmployee.png",
            "DefaultCat.png",
            "DefaultDog.png",
            "DefaultParrot.png",
            "DefaultRodent.png",
        };

        public bool IsFileExtensionSupported(string fileName)
        {
            return _config["ImageExtensions"]!.Contains(Path.GetExtension(fileName));
        }

        public Task RemoveFileFromServer(string fileName)
        {
            string name = Path.GetFileName(fileName);
            string path = @$"{Directory.GetCurrentDirectory()}\wwwroot\img\user_images\{name}";
            return Task.Run(() => 
            {
                if (!DefaultFileNames.Contains(name) && File.Exists(path))
                {
                    File.Delete(path);
                }   
            });
        }

        public async Task UploadFileToServer(IFormFile file, string uniqueFileName)
        {
            string uniquePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\img\user_images\", uniqueFileName);
            using (var fs = new FileStream(uniquePath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fs);
            }
        }
    }
}

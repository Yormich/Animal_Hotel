using Animal_Hotel.Models.DatabaseModels;

namespace Animal_Hotel.Services
{
    public class ImagePathProvider : IImagePathProvider
    {
        public string BuildAnimalFileName(string userLogin, long animalId, string fileName)
        {
            return $"{userLogin}_animal{animalId}_{fileName}";
        }

        public string BuildRoomFileName(short roomId, string fileName)
        {
            return $"room_{roomId}_{fileName}";
        }

        public string BuildUserFileName(string userLogin, string fileName)
        {
            return $"{userLogin}_{fileName}";
        }
    }
}

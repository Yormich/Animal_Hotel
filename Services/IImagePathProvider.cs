namespace Animal_Hotel.Services
{
    public interface IImagePathProvider
    {
        public string BuildUserFileName(string userLogin, string fileName);

        public string BuildAnimalFileName(string userLogin, long animalId, string fileName);

        public string BuildRoomFileName(short roomId, string fileName);
    }
}

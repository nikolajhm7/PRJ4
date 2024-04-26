using Server.API.Services.Interfaces;

namespace Server.API.Services
{
    public class RandomGenerator : IIdGenerator, IRandomPicker
    {
        public string GenerateRandomLobbyId()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 999999);
            return randomNumber.ToString("D6"); // to make the correct format
        }

        public T PickRandomItem<T>(List<T> list)
        {
            Random random = new Random();
            return list[random.Next(list.Count)];
        }
    }
}

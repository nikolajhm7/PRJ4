namespace Server.API.Services
{
    public class IDGenerator
    {
        public static string GenerateRandomLobbyID()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 999999);
            return randomNumber.ToString("D6"); // to make the correct format
        }
    }
}

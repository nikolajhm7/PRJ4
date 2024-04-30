namespace Server.API.Services.Interfaces
{
    public interface IRandomPicker
    {
        T PickRandomItem<T>(List<T> list);
    }
}

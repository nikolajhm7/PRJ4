namespace Client.Libary.Interfaces;

public interface IPreferenceManager
{
    string Get(string key, string defaultValue);
    void Set(string key, string value);
    bool ContainsKey(string key);
}
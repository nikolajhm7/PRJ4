namespace Client.Library.Interfaces;

public interface IPreferenceManager
{
    string Get(string key, string defaultValue);
    void Set(string key, string value);
    bool ContainsKey(string key);
    void Remove(string key);
    void Clear(string sharedName);
}
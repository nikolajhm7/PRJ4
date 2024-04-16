using Client.Libary.Interfaces;

namespace Client.UI.Managers;

public class PreferenceManager : IPreferenceManager
{
    public string Get(string key, string defaultValue) => Preferences.Get(key, defaultValue);

    public void Set(string key, string value) => Preferences.Set(key, value);

    public bool ContainsKey(string key) => Preferences.ContainsKey(key);
}
namespace Server.API.Games
{
    public static class Words
    {
        public static Dictionary<string, List<string>> CategoryMap { get; } = new()
        {
            { "Animals", new List<string> { "dog", "cat", "elephant", "tiger", "lion" } },
            { "Fruits", new List<string> { "apple", "banana", "orange", "grape", "strawberry" } },
            { "Sports", new List<string> { "football", "basketball", "tennis", "volleyball", "swimming" } },
            { "Countries", new List<string> { "usa", "canada", "france", "germany", "japan" } },
            { "Programming", new List<string> { "csharp", "java", "python", "javascript", "typescript" } },
            {"SverreLingo", new List<string> {"lootbanana","soaking"}}
        };
    }
}
 
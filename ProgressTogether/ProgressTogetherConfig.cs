using Newtonsoft.Json;
using TShockAPI;

namespace ProgressTogether;

public class ProgressTogetherConfig
{
    [JsonProperty("enabled")] public bool enabled { get; set; }
    [JsonProperty("entries")] public List<ProgressTogetherEntry> entries { get; set; } = new();

    public ProgressTogetherConfig()
    {
        enabled = true;
    }
    
    public void Add(ProgressTogetherEntry entry)
    {
        entries.Add(entry);
    }

    public int RemoveAllMatches(string name)
    {
        return entries.RemoveAll(val => val.name == name);
    }

    public string StringifyEntries()
    {
        return String.Join("\n", this.entries.Select(entry => entry.ToString()));
    }
}

public class ProgressTogetherEntry
{
    [JsonProperty("name")] public string? name { get; set; }
    [JsonProperty("uuid")] public string? uuid { get; set; }

    public ProgressTogetherEntry()
    {
    }
    public ProgressTogetherEntry(string name, string uuid)
    {
        this.name = name;
        this.uuid = uuid;
    }

    public static ProgressTogetherEntry? FromActivePlayerName(string name)
    {
        ProgressTogetherEntry entry = null;
        foreach (var player in TShock.Players)
        {
            if (player.Name == name)
            {
                entry = new ProgressTogetherEntry()
                {
                    name = player.Name,
                    uuid = player.UUID,
                };
                break;
            }
        }

        return entry;
    }

    public bool Matches(TSPlayer player)
    {
        return this.name == player.Name && this.uuid == player.UUID;
    }

    public bool Matches(ProgressTogetherEntry entry)
    {
        return this.name == entry.name && this.uuid == entry.uuid;
    }
    
    public override string ToString()
    {
        return $"ProgressTogetherEntry(name={name}, uuid={uuid})";
    }
}
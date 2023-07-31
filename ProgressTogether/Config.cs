using Newtonsoft.Json;
using TShockAPI;

namespace ProgressTogether;

public class Config
{
    [JsonIgnore]
    public static string ConfigPath
    {
        get { return Path.Combine(TShock.SavePath, "progress-together.json"); }
    }

    [JsonProperty("addOnLogin")] private bool _addOnLogin;
    [JsonProperty("logBossSpawns")] private bool _logBossSpawns;
    [JsonProperty("enabled")] private bool _enabled;
    [JsonProperty("entries")] private List<ProgressTogetherEntry> _entries = new();

    [JsonIgnore] private static readonly JsonSerializerSettings serializeOpts = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Include,
        Formatting = Formatting.Indented,
    };

    public Config()
    {
        _enabled = true;
    }

    public bool Enabled()
    {
        return _enabled;
    }
    public void Enabled(bool enable)
    {
        _enabled = enable;
        Write();
    }

    public bool AddOnLogin()
    {
        return _addOnLogin;
    }
    public bool LogBossSpawns()
    {
        return _logBossSpawns;
    }

    public List<ProgressTogetherEntry> Entries()
    {
        return _entries;
    }

    public bool Matches(TSPlayer player)
    {
        foreach (var entry in _entries)
        {
            if (entry.Matches(player))
            {
                return true;
            }
        }

        return false;
    }
    
    public void Add(ProgressTogetherEntry entry)
    {
        _entries.Add(entry);
        Write();
    }

    public int RemoveAllMatches(string name)
    {
        var matches = _entries.RemoveAll(val => val.name == name);
        if (matches > 0)
        {
            Write();
        }
        return matches;
    }

    public string StringifyEntries()
    {
        return String.Join("\n", this._entries.Select(entry => entry.ToString()));
    }

    public static Config? Load()
    {
        if (!File.Exists(ConfigPath))
        {
            return new Config();
        }

        string fileContent = File.ReadAllText(ConfigPath);
        Config? configData;
        try
        {
            configData = JsonConvert.DeserializeObject<Config>(fileContent, serializeOpts);
        }
        catch (Exception)
        {
            return null;
        }

        return configData;
    }

    public void Write()
    {
        var jsonConfig = JsonConvert.SerializeObject(this, serializeOpts);
        File.WriteAllText(ConfigPath, jsonConfig);
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
        ProgressTogetherEntry? entry = null;
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
        var matches = this.name == player.Name;
        if (this.uuid != null)
        {
            matches = matches && this.uuid == player.UUID;
        }

        return matches;
    }

    public bool Matches(ProgressTogetherEntry entry)
    {
        var matches = this.name == entry.name;
        if (this.uuid != null)
        {
            matches = matches && this.uuid == entry.uuid;
        }

        return matches;
    }

    public override string ToString()
    {
        return $"ProgressTogetherEntry(name={name}, uuid={uuid})";
    }
}
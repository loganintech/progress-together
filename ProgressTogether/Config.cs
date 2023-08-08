using Newtonsoft.Json;
using TShockAPI;

namespace ProgressTogether;

public class Config
{
    [JsonIgnore] private static string ConfigPath => Path.Combine(TShock.SavePath, "progress-together.json");

    [JsonProperty("addOnLogin")] public bool AddOnLogin { get; }
    [JsonProperty("logBossSpawns")] public bool LogBossSpawns { get; }
    [JsonProperty("enabled")] private bool _enabled = true;
    [JsonProperty("entries")] private List<ProgressTogetherEntry> _entries = new();
    [JsonProperty("sendMissedBossesOnJoin")] public bool SendMissedBossesOnJoin { get; }
    [JsonProperty("missedBosses")] private Dictionary<ProgressTogetherEntry, List<BossEntry>> _missedBosses = new();

    [JsonIgnore] private static readonly JsonSerializerSettings SerializeOpts = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Include,
        Formatting = Formatting.Indented,
    };

    public bool Enabled()
    {
        return _enabled;
    }

    public void Enabled(bool enable)
    {
        _enabled = enable;
        Write();
    }

    public List<ProgressTogetherEntry> Entries()
    {
        return _entries;
    }

    public bool PlayerInRequiredList(TSPlayer player)
    {
        foreach (var entry in _entries)
        {
            if (entry == player)
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

    public void AddMissed(ProgressTogetherEntry entry, BossEntry boss)
    {
        // If the player hasn't missed anything yet, don't include anything
        if (!_missedBosses.ContainsKey(entry))
        {
            _missedBosses.Add(entry, new List<BossEntry> { boss });
            Write();
            return;
        }

        // If the player already has that boss marked as missed, don't add it again
        // This shouldn't happen if we only add a boss on first spawn but let's cover ourselves anyways
        if (_missedBosses[entry].Contains(boss))
        {
            return;
        }
        
        _missedBosses[entry].Add(boss);
        Write();
    }

    public List<BossEntry>? GetMissedForEntry(ProgressTogetherEntry entry)
    {
        _missedBosses.TryGetValue(entry, out List<BossEntry>? boss);
        return boss;
    }

    public void ClearMissedForEntry(ProgressTogetherEntry entry)
    {
        _missedBosses.Remove(entry);
        Write();
    }

    public int RemoveAllMatches(string name)
    {
        var matches = _entries.RemoveAll(val => val.Name == name);
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
            configData = JsonConvert.DeserializeObject<Config>(fileContent, SerializeOpts);
        }
        catch (Exception)
        {
            return null;
        }

        return configData;
    }

    public void Write()
    {
        var jsonConfig = JsonConvert.SerializeObject(this, SerializeOpts);
        File.WriteAllText(ConfigPath, jsonConfig);
    }
}

public class BossEntry
{
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("netId")] public int NetId { get; set; }

    public BossEntry()
    {
        Name = "";
        NetId = 0;
    }

    public BossEntry(string name, int netId)
    {
        this.Name = name;
        this.NetId = netId;
    }

    public static bool operator ==(BossEntry a, BossEntry b)
    {
        return a.NetId == b.NetId;
    }

    public static bool operator !=(BossEntry a, BossEntry b)
    {
        return a.NetId != b.NetId;
    }

    public override string ToString()
    {
        return $"ProgressTogetherEntry(name={Name}, netId={NetId})";
    }
}

public class ProgressTogetherEntry
{
    [JsonProperty("name")] public string? Name { get; set; }
    [JsonProperty("uuid")] public string? Uuid { get; set; }

    public ProgressTogetherEntry()
    {
    }

    public ProgressTogetherEntry(string name, string uuid)
    {
        this.Name = name;
        this.Uuid = uuid;
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
                    Name = player.Name,
                    Uuid = player.UUID,
                };
                break;
            }
        }

        return entry;
    }

    public static bool operator ==(ProgressTogetherEntry a, ProgressTogetherEntry b)
    {
        var matches = a.Name == b.Name;
        if (a.Uuid != null)
        {
            matches = matches && a.Uuid == b.Uuid;
        }

        return matches;
    }

    public static bool operator !=(ProgressTogetherEntry a, ProgressTogetherEntry b)
    {
        return !(a == b);
    }

    public static bool operator ==(ProgressTogetherEntry a, TSPlayer b)
    {
        var matches = a.Name == b.Name;
        if (a.Uuid != null)
        {
            matches = matches && a.Uuid == b.UUID;
        }

        return matches;
    }

    public static bool operator !=(ProgressTogetherEntry a, TSPlayer b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return $"ProgressTogetherEntry(name={Name}, uuid={Uuid})";
    }
}
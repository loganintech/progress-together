using Newtonsoft.Json;
using TShockAPI;
using TShockAPI.Configuration;

namespace ProgressTogether;

public class ProgressTogetherConfig : ConfigFile<Config>
{
    public ProgressTogetherConfig ReadFile()
    {
        if (!File.Exists(Config.ConfigPath))
        {
            Write(Config.ConfigPath);
        }

        Read(Config.ConfigPath, out bool incompleteSettings);
        if (incompleteSettings)
        {
            // File was changed out of the server, this seems ok
        }

        return this;
    }

    public bool Enabled()
    {
        return this.Settings.Enabled;
    }

    public void Enabled(bool enable)
    {
        this.Settings.Enabled = enable;
        Write(Config.ConfigPath);
    }


    public List<ProgressTogetherEntry> Entries()
    {
        return this.Settings.Entries;
    }

    public bool PlayerInRequiredList(TSPlayer player)
    {
        foreach (var entry in this.Settings.Entries)
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
        this.Settings.Entries.Add(entry);
        Write(Config.ConfigPath);
    }

    public void AddMissed(ProgressTogetherEntry entry, BossEntry boss)
    {
        // If the player hasn't missed anything yet, don't include anything
        if (!this.Settings.MissedBosses.ContainsKey(entry))
        {
            this.Settings.MissedBosses.Add(entry, new List<BossEntry> { boss });
            Write(Config.ConfigPath);
            return;
        }

        // If the player already has that boss marked as missed, don't add it again
        // This shouldn't happen if we only add a boss on first spawn but let's cover ourselves anyways
        if (this.Settings.MissedBosses[entry].Contains(boss))
        {
            return;
        }

        this.Settings.MissedBosses[entry].Add(boss);
        Write(Config.ConfigPath);
    }

    public List<BossEntry>? GetMissedForEntry(ProgressTogetherEntry entry)
    {
        this.Settings.MissedBosses.TryGetValue(entry, out List<BossEntry>? boss);
        return boss;
    }

    public void ClearMissedForEntry(ProgressTogetherEntry entry)
    {
        this.Settings.MissedBosses.Remove(entry);
        Write(Config.ConfigPath);
    }

    public int RemoveAllMatches(string name)
    {
        var matches = this.Settings.Entries.RemoveAll(val => val.Name == name);
        if (matches > 0)
        {
            Write(Config.ConfigPath);
        }

        return matches;
    }

    public string StringifyEntries()
    {
        return String.Join("\n", this.Settings.Entries.Select(entry => entry.ToString()));
    }
}

public class Config
{
    public static string ConfigPath => Path.Combine(TShock.SavePath, "progress-together.json");

    [JsonProperty("addOnLogin")] public bool AddOnLogin;
    [JsonProperty("logBossSpawns")] public bool LogBossSpawns;
    [JsonProperty("enabled")] public bool Enabled;

    [JsonProperty("sendMissedBossesOnJoin")]
    public bool SendMissedBossesOnJoin;

    [JsonProperty("missedBosses")] public Dictionary<ProgressTogetherEntry, List<BossEntry>> MissedBosses;
    [JsonProperty("entries")] public List<ProgressTogetherEntry> Entries = new();
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
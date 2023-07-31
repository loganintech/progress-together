﻿using Newtonsoft.Json;
using TShockAPI;

namespace ProgressTogether;

public class ProgressTogetherConfig
{
    [JsonIgnore]
    private static string TShockConfigPath
    {
        get { return Path.Combine(TShock.SavePath, "progress-together.json"); }
    }

    [JsonProperty("enabled")] private bool _enabled;
    [JsonProperty("entries")] private List<ProgressTogetherEntry> _entries = new();

    [JsonIgnore] private static readonly JsonSerializerSettings serializeOpts = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Include,
        Formatting = Formatting.Indented,
    };

    public ProgressTogetherConfig()
    {
        _enabled = true;
    }

    public bool Enabled()
    {
        return _enabled;
    }
    public void Enable()
    {
        _enabled = true;
        Write();
    }
    public void Disable()
    {
        _enabled = false;
        Write();
    }

    public List<ProgressTogetherEntry> Entries()
    {
        return _entries;
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

    public static ProgressTogetherConfig? Load()
    {
        if (!File.Exists(TShockConfigPath))
        {
            return new ProgressTogetherConfig();
        }

        string fileContent = File.ReadAllText(TShockConfigPath);
        ProgressTogetherConfig? configData;
        try
        {
            configData = JsonConvert.DeserializeObject<ProgressTogetherConfig>(fileContent, serializeOpts);
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
        File.WriteAllText(TShockConfigPath, jsonConfig);
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
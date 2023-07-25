using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using TerrariaApi.Server;
using Terraria;
using Terraria.ID;
using TShockAPI;

namespace ProgressTogether;

[ApiVersion(2, 1)]
public class ProgressTogether : TerrariaPlugin
{
    public override string Author => "loganintech";

    public override string Description =>
        "Blocks bosses that haven't been spawned yet until enough of your friends are online!";

    public override string Name => "Progress Together";
    public override Version Version => new Version(0, 0, 0, 1);

    private ProgressTogetherConfig _config;

    private void Log(string message)
    {
        Console.WriteLine($"[{Name}]: {message}");
    }

    private static JsonSerializerSettings serializeOpts = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Include,
        Formatting = Formatting.Indented,
    };


    private static string TShockConfigPath
    {
        get { return Path.Combine(TShock.SavePath, "progress-together.json"); }
    }


    public override void Initialize()
    {
        ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
        Commands.ChatCommands.Add(new Command(Permissions.spawnboss, CommandHandler, "progress"));
        if (!Load())
        {
            Write();
        }
    }

    public bool Load()
    {
        if (!File.Exists(TShockConfigPath))
        {
            Log($"Load: File does not exist {TShockConfigPath}");
            _config = new ProgressTogetherConfig();
            return false;
        }

        string fileContent = File.ReadAllText(TShockConfigPath);
        ProgressTogetherConfig? configData;
        try
        {
            configData = JsonConvert.DeserializeObject<ProgressTogetherConfig>(fileContent, serializeOpts);
        }
        catch (Exception e)
        {
            Log($"Load: Exception happened reading config {e.Message}");
            return true;
        }

        if (configData == null)
        {
            Log($"Load: Config was loaded but not set properly.");
            return true;
        }

        Log("Load: Config loaded successfully.");
        _config = configData;
        return true;
    }


    public void Write()
    {
        try
        {
            var jsonConfig = JsonConvert.SerializeObject(_config, serializeOpts);
            File.WriteAllText(TShockConfigPath, jsonConfig);
        }
        catch (Exception e)
        {
            Log($"Write: Exception happened writing config {e.Message}");
        }
    }

    private void CommandHandler(CommandArgs args)
    {
        if (args.Parameters.Count == 0 || args.Parameters.Count > 2)
        {
            args.Player.SendErrorMessage("Usage: /progress <add|remove|status|enable|disable|reload> [player]");
            return;
        }

        if (args.Parameters.Count == 1)
        {
            HandleListEnableDisable(args);
            return;
        }

        if (args.Parameters.Count == 2)
        {
            HandleAddRemove(args);
            return;
        }

        args.Player.SendErrorMessage("Unknown command. You broke Tshock's param parsing.");
    }

    private void HandleListEnableDisable(CommandArgs args)
    {
        switch (args.Parameters[0])
        {
            case "status":
                args.Player.SendInfoMessage(
                    $"Progress Together is currently {(_config.enabled ? "enabled" : "disabled")}");
                var playersNotOnline = PlayersNotOnline();
                if (playersNotOnline.Count == 0 || !_config.enabled)
                {
                    args.Player.SendSuccessMessage($"Bosses that haven't spawned before will spawn freely.");
                    return;
                }

                args.Player.SendErrorMessage($"Bosses that haven't spawned before will be blocked.");
                args.Player.SendInfoMessage(
                    $"The following players are required for progression: {String.Join(", ", playersNotOnline)}");
                return;
            case "enable":
                _config.enabled = true;
                Write();
                args.Player.SendSuccessMessage("Progress Together is now enabled.");
                return;
            case "disable":
                _config.enabled = false;
                Write();
                args.Player.SendSuccessMessage("Progress Together is now disabled.");
                args.Player.SendSuccessMessage("Bosses will spawn without restriction.");
                return;
            case "list":
                args.Player.SendSuccessMessage($"The following players are required for progression: {_config.StringifyEntries()}");
                return;
            case "reload":
                if (!Load())
                {
                    args.Player.SendErrorMessage("Failed to reload config.");
                    return;
                }

                args.Player.SendSuccessMessage("Config reloaded.");
                return;
        }
    }

    private void HandleAddRemove(CommandArgs args)
    {
        switch (args.Parameters[0])
        {
            case "add":
                var entry = ProgressTogetherEntry.FromActivePlayerName(args.Parameters[1]);
                if (entry == null)
                {
                    args.Player.SendErrorMessage("Player not found.");
                    return;
                }

                _config.Add(entry);
                Write();
                return;
            case "remove":
                var removed = _config.RemoveAllMatches(args.Parameters[1]);
                if (removed == 0)
                {
                    args.Player.SendErrorMessage("Player not found.");
                    return;
                }
                args.Player.SendErrorMessage("Player was removed from progression requirements.");
                Write();
                return;
        }
    }


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);
        }

        base.Dispose(disposing);
    }

    public ProgressTogether(Main game) : base(game)
    {
    }

    private static bool BossAlreadyKilledByNetId(int id)
    {
        switch (id)
        {
            #region Pre-hardmode

            case NPCID.KingSlime:
                return NPC.downedSlimeKing;
            case NPCID.Deerclops:
                return NPC.downedDeerclops;
            case NPCID.EyeofCthulhu:
                return NPC.downedBoss1;
            case NPCID.BrainofCthulhu:
            case NPCID.EaterofWorldsBody:
            case NPCID.EaterofWorldsHead:
            case NPCID.EaterofWorldsTail:
            case NPCID.VileSpitEaterOfWorlds:
                return NPC.downedBoss2;
            case NPCID.QueenBee:
                return NPC.downedQueenBee;
            case NPCID.SkeletronHand:
            case NPCID.SkeletronHead:
                return NPC.downedBoss3;
            case NPCID.WallofFlesh:
            case NPCID.WallofFleshEye:
                return Main.hardMode;

            #endregion

            # region Hardmode

            case NPCID.QueenSlimeBoss:
                return NPC.downedQueenSlime;

            // The twins
            case NPCID.Retinazer:
            case NPCID.Spazmatism:
                return NPC.downedMechBoss1;

            case NPCID.TheDestroyer:
                return NPC.downedMechBoss2;

            // Also Mechdusa
            case NPCID.SkeletronPrime:
                return NPC.downedMechBoss3;

            // Moon Lord
            case NPCID.MoonLordCore:
            case NPCID.MoonLordHand:
            case NPCID.MoonLordHead:
            case NPCID.MoonLordFreeEye:
            case NPCID.MoonLordLeechBlob:
                return NPC.downedMoonlord;

            // Plantera
            case NPCID.Plantera:
            case NPCID.PlanterasTentacle:
            case NPCID.PlanterasHook:
                return NPC.downedPlantBoss;

            // Golem
            case NPCID.Golem:
            case NPCID.GolemHead:
            case NPCID.GolemFistRight:
            case NPCID.GolemFistLeft:
            case NPCID.GolemHeadFree:
                return NPC.downedGolemBoss;

            case NPCID.DukeFishron:
                return NPC.downedFishron;
            case NPCID.CultistBoss:
                return NPC.downedAncientCultist;
            case NPCID.EmpressButterfly:
                return NPC.downedEmpressOfLight;

            #endregion
        }

        return false;
    }

    private List<string> PlayersNotOnline()
    {
        var onlinePlayersSet = new List<ProgressTogetherEntry>();
        for (int i = 0; i < TShock.Players.Length; i++)
        {
            var player = TShock.Players[i];
            if (player == null)
            {
                continue;
            }

            onlinePlayersSet.Add(new ProgressTogetherEntry(player.Name, player.UUID));
        }

        var playersNotOnlineNames = new List<string>();
        foreach (var entry in _config.entries)
        {
            var foundMatch = false;
            foreach (var onlinePlayer in onlinePlayersSet)
            {
                Log($"Comparing {entry} to {onlinePlayer}");
                if (entry.Matches(onlinePlayer))
                {
                    foundMatch = true;
                    break;
                }
            }

            if (!foundMatch)
            {
                playersNotOnlineNames.Add(entry.name ?? "Unknown");
            }
        }

        return playersNotOnlineNames;
    }

    private void OnNpcSpawn(NpcSpawnEventArgs args)
    {
        var npc = Main.npc[args.NpcId];
        if (!npc.boss || BossAlreadyKilledByNetId(npc.netID) || !_config.enabled)
        {
            return;
        }

        var playersNotOnline = PlayersNotOnline();
        if (playersNotOnline.Count == 0)
        {
            return;
        }

        var playersNotOnlineString = String.Join(", ", playersNotOnline);
        var adjective = playersNotOnline.Count > 1 ? "are" : "is";
        TShock.Utils.Broadcast(
            $"Spawning {npc.FullName} is blocked because {playersNotOnlineString} {adjective} not online", Color.Red);
        args.Handled = true;
        npc.active = false;
    }
}
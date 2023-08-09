using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace ProgressTogether;

[ApiVersion(2, 1)]
public class ProgressTogether : TerrariaPlugin
{
    private Config _config;

    public ProgressTogether(Main game) : base(game)
    {
        _config = new Config();
    }

    public override string Author => "loganintech";

    public override string Description =>
        "Blocks bosses that haven't been spawned yet until enough of your friends are online!";

    public override string Name => Log.Name;
    public override Version Version => new(0, 0, 3, 0);

    public override void Initialize()
    {
        ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
        ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
        Commands.ChatCommands.Add(new Command(Permissions.spawnboss, CommandHandler, "progress"));
        var config = Config.Load();
        if (config is null)
        {
            config = new Config();
            config.Write();
        }

        _config = config;
    }

    private void OnServerJoin(JoinEventArgs args)
    {
        AddOnJoin(args);
        SendMissedBossesOnJoin(args);
    }

    private void AddOnJoin(JoinEventArgs args)
    {
        if (!_config.AddOnLogin)
        {
            return;
        }

        var player = TShock.Players[args.Who];
        if (player is null)
        {
            return;
        }

        // If that player is already in the matches list, don't add them again.
        if (_config.PlayerInRequiredList(player))
        {
            return;
        }

        // Add only based on name, otherwise the same player connecting on two different devices would prevent progression
        _config.Add(new ProgressTogetherEntry(player.Name, ""));
    }

    private void SendMissedBossesOnJoin(JoinEventArgs args)
    {
        if (!_config.SendMissedBossesOnJoin)
        {
            return;
        }

        var player = TShock.Players[args.Who];
        var entry = new ProgressTogetherEntry(player.Name, "");
        var missed = _config.GetMissedForEntry(entry);
        if (missed is null)
        {
            return;
        }

        if (missed.Count == 0)
        {
            _config.ClearMissedForEntry(entry);
            return;
        }

        player.SendInfoMessage($"While you were away, {CombineBossNames(missed.ConvertAll(x => x.Name))}");
        _config.ClearMissedForEntry(entry);
    }

    static string CombineBossNames(List<string> strings)
    {
        var count = strings.Count;
        switch (count)
        {
            case 0:
                return "nothing spawned. This message is probably a bug to send to loganintech";
            case 1:
                return strings[0] + " spawned.";
            case 2:
                return $"{strings[0]} and {strings[1]} spawned.";
            default:
            {
                string combined = string.Join(", ", strings.GetRange(0, count - 1));
                return $"{combined}, and {strings[count - 1]} all spawned.";
            }
        }
    }

    private void CommandHandler(CommandArgs args)
    {
        switch (args.Parameters.Count)
        {
            case 0 or > 2:
                args.Player.SendErrorMessage(
                    "Usage: /progress <add|remove|status|list|enable|disable|reload> [player]");
                return;
            case 1:
                HandleListEnableDisable(args);
                return;
            case 2:
                HandleAddRemove(args);
                return;
            default:
                args.Player.SendErrorMessage("Unknown command. You broke Tshock's param parsing.");
                break;
        }
    }

    private void HandleListEnableDisable(CommandArgs args)
    {
        switch (args.Parameters[0])
        {
            case "status":
                args.Player.SendInfoMessage(
                    $"Progress Together is currently {(_config.Enabled() ? "enabled" : "disabled")}");
                var playersNotOnline = PlayersNotOnline();
                if (playersNotOnline.Count == 0 || !_config.Enabled())
                {
                    args.Player.SendSuccessMessage($"All required players are online.");
                    args.Player.SendSuccessMessage($"Bosses that haven't spawned before will spawn freely.");
                    return;
                }

                args.Player.SendErrorMessage($"Bosses that haven't spawned before will be blocked.");
                args.Player.SendInfoMessage(
                    $"The following players are required for progression: {String.Join(", ", playersNotOnline)}");
                return;
            case "enable":
                _config.Enabled(true);
                args.Player.SendSuccessMessage("Progress Together is now enabled.");
                return;
            case "disable":
                _config.Enabled(false);
                args.Player.SendSuccessMessage("Progress Together is now disabled.");
                args.Player.SendSuccessMessage("Bosses will spawn without restriction.");
                return;
            case "list":
                if (_config.Entries().Count == 0)
                {
                    args.Player.SendSuccessMessage($"No players are required for progress.");
                    return;
                }

                args.Player.SendSuccessMessage(
                    $"The following players are required for progression: {_config.StringifyEntries()}");
                return;
            case "reload":
                var config = Config.Load();
                if (config is null)
                {
                    args.Player.SendErrorMessage("Failed to reload config.");
                    return;
                }

                _config = config;
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
                if (entry is null)
                {
                    args.Player.SendErrorMessage("Player not found.");
                    return;
                }

                args.Player.SendSuccessMessage("Player was added to the progression requirements.");
                _config.Add(entry);

                return;
            case "remove":
                var removed = _config.RemoveAllMatches(args.Parameters[1]);
                if (removed == 0)
                {
                    args.Player.SendErrorMessage("Player not found.");
                    return;
                }

                args.Player.SendSuccessMessage("Player was removed from progression requirements.");

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

    private List<ProgressTogetherEntry> PlayersNotOnline()
    {
        var onlinePlayersSet = new List<ProgressTogetherEntry>();
        for (int i = 0; i < TShock.Players.Length; i++)
        {
            var player = TShock.Players[i];
            if (player is null)
            {
                continue;
            }

            onlinePlayersSet.Add(new ProgressTogetherEntry(player.Name, player.UUID));
        }

        var playersNotOnlineNames = new List<ProgressTogetherEntry>();
        foreach (var entry in _config.Entries())
        {
            var foundMatch = false;
            foreach (var onlinePlayer in onlinePlayersSet)
            {
                if (entry == onlinePlayer)
                {
                    foundMatch = true;
                    break;
                }
            }

            if (!foundMatch)
            {
                playersNotOnlineNames.Add(entry);
            }
        }

        return playersNotOnlineNames;
    }

    private void OnNpcSpawn(NpcSpawnEventArgs args)
    {
        var npc = Main.npc[args.NpcId];
        // If it's not a boss don't block
        if (!npc.boss)
        {
            return;
        }

        // If it's not the first time they've spawned, don't block
        bool isFirstSpawn = !BossAlreadyKilledByNetId(npc.netID);
        if (!isFirstSpawn)
        {
            return;
        }

        var playersNotOnline = PlayersNotOnline();
        // Only block spawns if there are any players not online and the plugin is enabled
        bool shouldBlockSpawns = playersNotOnline.Any() && _config.Enabled();

        // If we're not going to block this spawn, log that the boss is spawning for the first time, then don't block
        if (!shouldBlockSpawns)
        {
            if (_config.LogBossSpawns)
            {
                Log.LogToFile($"{npc.FullName} spawned for the first time!");
            }

            if (_config.SendMissedBossesOnJoin)
            {
                foreach (var entry in playersNotOnline)
                {
                    _config.AddMissed(entry, new BossEntry(npc.FullName, npc.netID));
                }
            }

            return;
        }

        // We want to block spawning, so we set the npc to inactive and broadcast a message
        var playersNotOnlineString = String.Join(", ", playersNotOnline.ConvertAll(p => p.Name));
        var adjective = playersNotOnline.Count > 1 ? "are" : "is";
        TShock.Utils.Broadcast(
            $"Spawning {npc.FullName} is blocked because {playersNotOnlineString} {adjective} not online", Color.Red);
        args.Handled = true;
        npc.active = false;

        if (_config.LogBossSpawns)
        {
            Log.LogToFile($"Spawning {npc.FullName} was blocked.");
        }
    }
}
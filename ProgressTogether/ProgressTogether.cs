using Microsoft.Xna.Framework;
using TerrariaApi.Server;
using Terraria;
using TShockAPI;

namespace ProgressTogether;

[ApiVersion(2, 1)]
public class ProgressTogether : TerrariaPlugin
{
    public override string Author => "loganintech";
    public override string Description => "Hold progression together with your friends!";
    public override string Name => "Progress Together";
    public override Version Version => new Version(0, 0, 0, 1);

    public override void Initialize()
    {
        ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
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


    private void OnNpcSpawn(NpcSpawnEventArgs args)
    {
        var npc = Main.npc[args.NpcId];
        if (!npc.boss)
        {
            return;
        }

        TShock.Utils.Broadcast($"Spawning {npc.FullName} is blocked because Logan is not online", Color.Red);
        args.Handled = true;
        Main.npc[args.NpcId].active = false;
        // Set to bunny just cus
        args.NpcId = 46;
    }
}
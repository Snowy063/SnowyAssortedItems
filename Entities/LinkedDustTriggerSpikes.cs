using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.SnowyAssortedItems.Entities;

public class LinkedDustTriggerSpikes : TriggerSpikes
{
    public static void Load()
    {
        On.Celeste.TriggerSpikes.GetPlayerCollideIndex += onTriggerSpikesGetPlayerCollideIndex;
    }

    public static void Unload()
    {
        On.Celeste.TriggerSpikes.GetPlayerCollideIndex -= onTriggerSpikesGetPlayerCollideIndex;
    }

    private bool triggered = false;
    public string flag;
    public bool persistent; // Whether the flag should be reset in Added().
    public bool leader; // Whether this is the linked spikes the player makes contact with.

    public LinkedDustTriggerSpikes(EntityData data, Vector2 offset, Directions dir) : base(data, offset, dir)
    {
        leader = false;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (!persistent)
        {
            SceneAs<Level>().Session.SetFlag(flag, false);
        }
    }

    public override void Update()
    {
        base.Update();
        if (!triggered && SceneAs<Level>().Session.GetFlag(flag))
        {
            triggered = true;
        }
        else if (triggered && !SceneAs<Level>().Session.GetFlag(flag) && !leader)
        {
            triggered = false;
        }
    }

    private static void onTriggerSpikesGetPlayerCollideIndex(On.Celeste.TriggerSpikes.orig_GetPlayerCollideIndex orig,
        TriggerSpikes self, Player player, out int minIndex, out int maxIndex)
    {
        orig(self, player, out minIndex, out maxIndex);

        if (self is LinkedDustTriggerSpikes groupedSpikes)
        {
            int spikeCount = new DynData<TriggerSpikes>(self).Get<int>("size") / 4;

            if (maxIndex >= 0 && minIndex < spikeCount)
            {
                // let's pretend the player is pressing every trigger spike at once.
                minIndex = 0;
                maxIndex = spikeCount - 1;

                // the spikes have been triggered.
                groupedSpikes.triggered = true;
            }
        }
    }
}

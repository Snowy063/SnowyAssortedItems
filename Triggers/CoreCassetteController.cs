using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using SnowyAssortedEntities.Entities;
using System.Reflection;

namespace Celeste.Mod.SnowyAssortedItems.Triggers;

[CustomEntity("SnowyAssortedItems/CoreCassetteController")]
public class CoreCassetteController : Trigger
{
    private int beatsRequired;
    private int currentBeats;

    public CoreCassetteController(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        beatsRequired = data.Int("beatsRequired");
        Add(new CassetteListener
        {
            OnBeat = OnBeat,
            OnFinish = OnFinish
        });
    }

    private void OnBeat(int idx)
    {
        currentBeats++;
        if(currentBeats == beatsRequired)
        {
            Level level = SceneAs<Level>();
            level.CoreMode = level.CoreMode != Session.CoreModes.Cold ? Session.CoreModes.Cold : Session.CoreModes.Hot;
            currentBeats = 0;
        }
    }

    private void OnFinish()
    {
        Level level = SceneAs<Level>();
        level.CoreMode = Session.CoreModes.None;
    }
}


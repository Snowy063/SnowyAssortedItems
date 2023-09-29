using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.SnowyAssortedItems.Entities;

[CustomEntity("SnowyAssortedItems/JuckterTheoCrystal")]
[TrackedAs(typeof(TheoCrystal))]
[Tracked(false)]
public class JuckterTheoCrystal : TheoCrystal
{
    private bool inField;
    private DynamicData theoCrystalData;

    public bool InField { get => inField; set => inField = value; }

    public JuckterTheoCrystal(Vector2 position, bool inField) : base(position)
    {
        this.inField = inField;
        this.Hold.OnPickup = new Action(this.OnPickup);
        theoCrystalData = DynamicData.For(this);
    }

    public JuckterTheoCrystal(EntityData e, Vector2 offset)
      : this(e.Position + offset, false)
    {
    }


    private void OnPickup()
    {
        this.Speed = Vector2.Zero;
        this.AddTag((int) Tags.Persistent);

        foreach (JuckterTheoCrystal entity in this.Scene.Tracker.GetEntities<JuckterTheoCrystal>())
        {
            if (entity != this)
            {
                entity.theoCrystalData.Set("dead", true);
                entity.theoCrystalData.Get<Sprite>("sprite").Visible = false;
                Audio.Play("event:/char/madeline/death", Position);
                Add(new DeathEffect(Color.ForestGreen, base.Center - Position));
                entity.Depth = -1000000;
                entity.AllowPushing = false;
                entity.RemoveSelf();
            }
        }
    }

    public override void Update()
    {
        base.Update();

        bool isColliding;
        bool hasCollided = false;
        foreach (JuckterField entity in this.Scene.Tracker.GetEntities<JuckterField>())
        {
            entity.Collidable = true;
            isColliding = CollideCheck(entity);
            entity.Collidable = false;

            if (isColliding)
            {
                hasCollided = true;
                if (!Hold.IsHeld && !inField && !theoCrystalData.Get<bool>("dead"))
                {
                    theoCrystalData.Set("dead", true);
                    theoCrystalData.Get<Sprite>("sprite").Visible = false;
                    Audio.Play("event:/char/madeline/death", Position);
                    Add(new DeathEffect(Color.ForestGreen, base.Center - Position));
                    Depth = -1000000;
                    AllowPushing = false;
                    RemoveSelf();
                    foreach (JuckterField field in this.Scene.Tracker.GetEntities<JuckterField>())
                    {
                        this.Scene.Add(new JuckterTheoCrystal(field.Position + new Vector2(field.Width / 2, field.Height / 2 + 10), true));
                        field.OnDuplicate();
                    }
                }
            }
        }

        if (!hasCollided)
        {
            inField = false;
        }

    }
}

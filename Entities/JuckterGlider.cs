using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
namespace Celeste.Mod.SnowyAssortedItems.Entities;

[CustomEntity("SnowyAssortedItems/JuckterGlider")]
[Tracked(false)]
public class JuckterGlider : Glider
{
    private bool inField;
    private DynamicData gliderData;

    public JuckterGlider(Vector2 position, bool bubble, bool inField) : base(position, bubble, false)
    {
        this.inField = inField;
        this.Hold.OnPickup = new Action(this.OnPickup);
        gliderData = DynamicData.For(this);
        gliderData.Set("bubble", bubble);
    }

    public JuckterGlider(EntityData e, Vector2 offset)
      : this(e.Position + offset, e.Bool("bubble"), false)
    {
    }

    private void OnPickup()
    {
        if (!gliderData.Get<bool>("bubble"))
        {
            for (int num = 0; num < 24; ++num)
                gliderData.Get<Level>("level").Particles.Emit(P_Platform, Position + PlatformAdd(num), PlatformColor(num));
        }
        this.AllowPushing = false;
        this.Speed = Vector2.Zero;
        this.AddTag((int) Tags.Persistent);
        gliderData.Set("highFrictionTimer", 0.5f);
        gliderData.Set("bubble", false);
        gliderData.Get<Wiggler>("wiggler").Start();

        foreach (JuckterGlider entity in this.Scene.Tracker.GetEntities<JuckterGlider>())
        {
            if (entity != this)
            {
                entity.gliderData.Set("destroyed", true);
                entity.Collidable = false;
                entity.Add((Component) new Coroutine(entity.DestroyAnimationRoutine()));
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
                if (!Hold.IsHeld && !inField && !gliderData.Get<bool>("destroyed"))
                {
                    gliderData.Set("destroyed", true);
                    this.Collidable = false;
                    Add(new Coroutine(DestroyAnimationRoutine()));
                    foreach (JuckterField field in this.Scene.Tracker.GetEntities<JuckterField>())
                    {
                        this.Scene.Add(new JuckterGlider(field.Position + new Vector2(field.Width / 2, field.Height / 2 + 10), field.Bubble, true));
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

    private IEnumerator DestroyAnimationRoutine()
    {
        Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", Position);
        gliderData.Get<Sprite>("sprite").Play("death");
        yield return 1f;
        RemoveSelf();
    }

    private Vector2 PlatformAdd(int num)
    {
        return new Vector2(-12 + num, -5 + (int) Math.Round(Math.Sin(base.Scene.TimeActive + (float) num * 0.2f) * 1.7999999523162842));
    }

    private Color PlatformColor(int num)
    {
        if (num <= 1 || num >= 22)
        {
            return Color.White * 0.4f;
        }

        return Color.White * 0.8f;
    }
}

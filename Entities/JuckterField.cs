using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.SnowyAssortedItems.Entities;

[CustomEntity("SnowyAssortedItems/JuckterField")]
[Tracked(false)]
public class JuckterField : Solid
{
    private bool bubble;
    public float Flash;
    public float Solidify;
    public bool Flashing;
    private float solidifyDelay;
    internal bool hasRenderer = false;
    private List<Vector2> particles = new List<Vector2>();
    private float[] speeds = new float[3] { 12f, 20f, 40f };


    public global::System.Boolean Bubble { get => bubble; set => bubble = value; }

    public JuckterField(Vector2 position, float width, float height, bool bubble)
      : base(position, width, height, false)
    {
        this.bubble = bubble;
        this.Collidable = false;
        for (int index = 0; (double) index < (double) this.Width * (double) this.Height / 16.0; ++index)
            this.particles.Add(new Vector2(Calc.Random.NextFloat(this.Width - 1f), Calc.Random.NextFloat(this.Height - 1f)));
    }

    public JuckterField(EntityData data, Vector2 offset)
      : this(data.Position + offset, data.Width,data.Height, data.Bool("bubble"))
    {
    }

    public override void Awake(Scene scene)
    {
        if (!hasRenderer)
        {
            JuckterFieldRenderer next = new JuckterFieldRenderer();
            Scene.Add(next);
            foreach (JuckterField f in SceneAs<Level>().Tracker.GetEntities<JuckterField>())
            {
                next.Track(f, this.SceneAs<Level>());
                f.hasRenderer = true;
            }
        }
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        scene.Tracker.GetEntity<JuckterFieldRenderer>().Untrack(this);
    }

    public override void Update()
    {
        if (Flashing)
        {
            Flash = Calc.Approach(Flash, 0.0f, Engine.DeltaTime * 4f);
            if (Flash <= 0.0)
                Flashing = false;
        }
        else if (solidifyDelay > 0.0)
            solidifyDelay -= Engine.DeltaTime;
        else if (Solidify > 0.0)
            Solidify = Calc.Approach(Solidify, 0.0f, Engine.DeltaTime);
        int length = speeds.Length;
        float height = Height;
        int index = 0;
        for (int count = particles.Count; index < count; ++index)
        {
            Vector2 vector2 = particles[index] + Vector2.UnitY * speeds[index % length] * Engine.DeltaTime;
            vector2.Y %= height - 1f;
            particles[index] = vector2;
        }
        base.Update();
    }

    public void OnDuplicate()
    {
        this.Flash = 1f;
        this.Solidify = 1f;
        this.solidifyDelay = 1f;
        this.Flashing = true;
    }

    public override void Render()
    {
        if (!this.IsVisible())
            return;
        Color color = Color.Cyan * 0.5f;
        foreach (Vector2 particle in this.particles)
            Draw.Pixel.Draw(this.Position + particle, Vector2.Zero, color);
        if (!this.Flashing)
            return;
        Draw.Rect(this.Collider, Color.Cyan * this.Flash * 0.5f);
    }

    private bool IsVisible() => CullHelper.IsRectangleVisible(this.Position.X, this.Position.Y, this.Width, this.Height);
}

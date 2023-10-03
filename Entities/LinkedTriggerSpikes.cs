﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.SnowyAssortedItems.Entities;

[CustomEntity(
        "SnowyAssortedItems/LinkedTriggerSpikesUp = LoadUp",
        "SnowyAssortedItems/LinkedTriggerSpikesDown = LoadDown",
        "SnowyAssortedItems/LinkedTriggerSpikesLeft = LoadLeft",
        "SnowyAssortedItems/LinkedTriggerSpikesRight = LoadRight"
    )]
public class LinkedTriggerSpikes : Entity
{
    public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {

        if (entityData.Attr("type", "default") == "dust")
        {
            //return new LinkedDustTriggerSpikes(entityData, offset, TriggerSpikes.Directions.Up);
        }
        return new LinkedTriggerSpikes(entityData, offset, Directions.Up);
    }

    public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        if (entityData.Attr("type", "default") == "dust")
        {
            //return new LinkedDustTriggerSpikes(entityData, offset, TriggerSpikes.Directions.Down);
        }
        return new LinkedTriggerSpikes(entityData, offset, Directions.Down);
    }

    public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        if (entityData.Attr("type", "default") == "dust")
        {
            //return new LinkedDustTriggerSpikes(entityData, offset, TriggerSpikes.Directions.Left);
        }
        return new LinkedTriggerSpikes(entityData, offset, Directions.Left);
    }

    public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
    {
        if (entityData.Attr("type", "default") == "dust")
        {
            //return new LinkedDustTriggerSpikes(entityData, offset, TriggerSpikes.Directions.Right);
        }
        return new LinkedTriggerSpikes(entityData, offset, Directions.Right);
    }

    public enum Directions
    {
        Up,
        Down,
        Left,
        Right
    }

    public string flag;
    public bool persistent; // Whether the flag should be reset in Added().
    public bool leader; // Whether this is the linked spikes the player makes contact with.
    private const float DelayTime = 0.4f;
    public bool Triggered = false;
    public float DelayTimer;
    public float Lerp;
    private int size;
    private Directions direction;
    private string overrideType;
    private Vector2 outwards;
    private Vector2 shakeOffset;
    private string spikeType;
    private Vector2[] spikePositions;
    private List<MTexture> spikeTextures;

    // "attached to cassette block" stuff
    private Color enabledColor = Color.White;
    private Color disabledColor = Color.White;
    private bool visibleWhenDisabled = false;

    private bool blockingLedge = false;


    public LinkedTriggerSpikes(EntityData data, Vector2 offset, Directions dir)
            : this(data.Position + offset, GetSize(data, dir), dir, data.Attr("type", "default"), data.Attr("flag"), data.Bool("persistent"))
    {
    }

    public LinkedTriggerSpikes(Vector2 position, int size, Directions direction, string overrideType, string flag, bool persistent)
        : base(position)
    {
        leader = false;
        this.persistent = persistent;
        this.flag = flag;
        this.size = size;
        this.direction = direction;
        this.overrideType = overrideType;
        Depth = -50;

        switch (direction)
        {
            case Directions.Up:
                outwards = new Vector2(0f, -1f);
                Collider = new Hitbox(size, 3f, 0f, -3f);
                Add(new SafeGroundBlocker());
                Add(new LedgeBlocker(UpSafeBlockCheck));
                break;

            case Directions.Down:
                outwards = new Vector2(0f, 1f);
                Collider = new Hitbox(size, 3f, 0f, 0f);
                break;

            case Directions.Left:
                outwards = new Vector2(-1f, 0f);
                Collider = new Hitbox(3f, size, -3f, 0f);

                Add(new SafeGroundBlocker());
                Add(new LedgeBlocker(SideSafeBlockCheck));
                break;

            case Directions.Right:
                outwards = new Vector2(1f, 0f);
                Collider = new Hitbox(3f, size, 0f, 0f);

                Add(new SafeGroundBlocker());
                Add(new LedgeBlocker(SideSafeBlockCheck));
                break;
        }

        Add(new PlayerCollider(OnCollide));

        Add(new StaticMover
        {
            OnShake = OnShake,
            SolidChecker = CheckAttachToSolid,
            JumpThruChecker = IsRiding,
            OnEnable = OnEnable,
            OnDisable = OnDisable
        });
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        if (!persistent)
        {
            SceneAs<Level>().Session.SetFlag(flag, false);
        }

        AreaData areaData = AreaData.Get(scene);
        spikeType = areaData.Spike;
        if (!string.IsNullOrEmpty(overrideType) && overrideType != "default")
        {
            spikeType = overrideType;
        }

        string dir = direction.ToString().ToLower();

        if (spikeType == "tentacles")
        {
            throw new NotSupportedException("Trigger tentacles currently not supported");
        }

        spikePositions = new Vector2[size / 8];
        spikeTextures = GFX.Game.GetAtlasSubtextures("danger/spikes/" + spikeType + "_" + dir);
        for (int i = 0; i < spikePositions.Length; i++)
        {
            switch (direction)
            {
                case Directions.Up:
                    spikePositions[i] = Vector2.UnitX * (i + 0.5f) * 8f + Vector2.UnitY;
                    break;

                case Directions.Down:
                    spikePositions[i] = Vector2.UnitX * (i + 0.5f) * 8f - Vector2.UnitY;
                    break;

                case Directions.Left:
                    spikePositions[i] = Vector2.UnitY * (i + 0.5f) * 8f + Vector2.UnitX;
                    break;

                case Directions.Right:
                    spikePositions[i] = Vector2.UnitY * (i + 0.5f) * 8f - Vector2.UnitX;
                    break;
            }
        }
    }

    private bool CheckAttachToSolid(Solid solid)
    {
        bool collides = IsRiding(solid);

        // if we happen to be attached to a cassette block, take its color.
        if (collides && solid is CassetteBlock cassetteBlock)
        {
            enabledColor = new DynData<CassetteBlock>(cassetteBlock).Get<Color>("color");

            // same formula as cassette blocks do.
            Color color = Calc.HexToColor("667da5");
            disabledColor = new Color(
                color.R / 255f * (enabledColor.R / 255f),
                color.G / 255f * (enabledColor.G / 255f),
                color.B / 255f * (enabledColor.B / 255f),
                1f);

            visibleWhenDisabled = true;
        }

        return collides;
    }

    private void OnEnable()
    {
        Collidable = Visible = true;
    }

    private void OnDisable()
    {
        Collidable = false;
        Visible = visibleWhenDisabled;
    }

    private void OnShake(Vector2 amount)
    {
        shakeOffset += amount;
    }

    private bool UpSafeBlockCheck(Player player)
    {
        int dir = 8 * (int)player.Facing;
        int left = (int)((player.Left + dir - Left) / 4f);
        int right = (int)((player.Right + dir - Left) / 4f);

        if (right < 0 || left >= spikePositions.Length)
        {
            return false;
        }

        return Lerp >= 1f;
    }

    private bool SideSafeBlockCheck(Player player)
    {
        int top = (int)((player.Top - Top) / 4f);
        int bottom = (int)((player.Bottom - Top) / 4f);

        if (bottom < 0 || top >= spikePositions.Length)
            return false;

        return Lerp >= 1f;
    }

    private void OnCollide(Player player)
    {
        GetPlayerCollideIndex(player, out int minIndex, out int maxIndex);
        if (minIndex < 0 || minIndex >= spikePositions.Length)
        {
            return;
        }

        if (!Triggered)
        {
            Audio.Play("event:/game/03_resort/fluff_tendril_touch", Position + spikePositions[minIndex]);
            Triggered = true;
            leader = true;
            DelayTimer = DelayTime;
        }
        else if (Lerp >= 1f)
        {
            player.Die(outwards);
        }
    }

    private void GetPlayerCollideIndex(Player player, out int minIndex, out int maxIndex)
    {
        minIndex = maxIndex = -1;

        switch (direction)
        {
            case Directions.Up:
                if (player.Speed.Y >= 0f)
                {
                    minIndex = (int)((player.Left - Left) / 8f);
                    maxIndex = (int)((player.Right - Left) / 8f);
                }
                break;

            case Directions.Down:
                if (player.Speed.Y <= 0f)
                {
                    minIndex = (int)((player.Left - Left) / 8f);
                    maxIndex = (int)((player.Right - Left) / 8f);
                }
                break;

            case Directions.Left:
                if (player.Speed.X >= 0f)
                {
                    minIndex = (int)((player.Top - Top) / 8f);
                    maxIndex = (int)((player.Bottom - Top) / 8f);
                }
                break;

            case Directions.Right:
                if (player.Speed.X <= 0f)
                {
                    minIndex = (int)((player.Top - Top) / 8f);
                    maxIndex = (int)((player.Bottom - Top) / 8f);
                }
                break;
        }
    }

    private static int GetSize(EntityData data, Directions dir)
    {
        return
            dir > Directions.Down ?
            data.Height :
            data.Width;
    }

    public override void Update()
    {
        base.Update();
        if (!Triggered && SceneAs<Level>().Session.GetFlag(flag))
        {
            Triggered = true;
        }
        else if (Triggered && !SceneAs<Level>().Session.GetFlag(flag) && !leader)
        {
            Triggered = false;
        }

        if (Triggered)
        {
            if (DelayTimer > 0f)
            {
                DelayTimer -= Engine.DeltaTime;
                if (DelayTimer <= 0f)
                {
                    if (CollideCheck<Player>())
                    {
                        DelayTimer = 0.05f;
                    }
                    else
                    {
                        Audio.Play("event:/game/03_resort/fluff_tendril_emerge", Position + spikePositions[spikePositions.Length / 2]);
                        SceneAs<Level>().Session.SetFlag(flag, true);
                        leader = false;
                    }
                }
            }
            else
            {
                Lerp = Calc.Approach(Lerp, 1f, 8f * Engine.DeltaTime);
            }
        }
        else
        {
            Lerp = Calc.Approach(Lerp, 0f, 4f * Engine.DeltaTime);
            if (Lerp <= 0f)
            {
                Triggered = false;
            }
        }

        // "climb hopping" should be blocked if trigger spikes are going to kill Madeline (Lerp >= 1).
        // this is done by adding a LedgeBlocker component.
        if (blockingLedge != (Lerp >= 1f))
        {
            blockingLedge = !blockingLedge;

            // add or remove the ledge blocker depending on the need.
            if (blockingLedge)
            {
                Add(new LedgeBlocker());
            }
            else
            {
                foreach (Component component in this)
                {
                    if (component is LedgeBlocker)
                    {
                        Remove(component);
                        break;
                    }
                }
            }
        }
    }

    public override void Render()
    {
        base.Render();

        Vector2 justify = Vector2.One * 0.5f;
        switch (direction)
        {
            case Directions.Up:
                justify = new Vector2(0.5f, 1f);
                break;
            case Directions.Down:
                justify = new Vector2(0.5f, 0f);
                break;
            case Directions.Left:
                justify = new Vector2(1f, 0.5f);
                break;
            case Directions.Right:
                justify = new Vector2(0f, 0.5f);
                break;
        }

        for (int i = 0; i < spikePositions.Length; i++)
        {
            MTexture tex = spikeTextures[0];
            Vector2 pos = Position + shakeOffset + spikePositions[i] + outwards * (-4f + Lerp * 4f);
            tex.DrawJustified(pos, justify, Collidable ? enabledColor : disabledColor);
        }
    }

    private bool IsRiding(Solid solid)
    {
        switch (direction)
        {
            case Directions.Up:
                return CollideCheckOutside(solid, Position + Vector2.UnitY);
            case Directions.Down:
                return CollideCheckOutside(solid, Position - Vector2.UnitY);
            case Directions.Left:
                return CollideCheckOutside(solid, Position + Vector2.UnitX);
            case Directions.Right:
                return CollideCheckOutside(solid, Position - Vector2.UnitX);
            default:
                return false;
        }
    }

    private bool IsRiding(JumpThru jumpThru)
    {
        return direction == Directions.Up && CollideCheck(jumpThru, Position + Vector2.UnitY);
    }
}

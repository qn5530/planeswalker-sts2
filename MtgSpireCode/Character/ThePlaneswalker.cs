using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Planeswalker.MtgSpireCode.Cards;
using Planeswalker.MtgSpireCode.Extensions;
using Planeswalker.MtgSpireCode.Relics;

namespace Planeswalker.MtgSpireCode.Character;

/// <summary>
/// The Planeswalker: a mage who draws on all five colors of mana and summons creatures into
/// three battlefield slots (the game's orb slots). Combat visuals are the mod's own sprite with
/// code-built idle/attack/hit/cast/death animations; rest-site and merchant art still fall back to
/// the Ironclad via PlaceholderCharacterModel.
/// </summary>
public class ThePlaneswalker : PlaceholderCharacterModel
{
    public const string CharacterId = "Planeswalker";

    /// <summary>Spark gold, used for the name color, deck entries and lab outlines.</summary>
    public static readonly Color Color = new("E0B84A");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 72;

    /// <summary>Battlefield: creatures are summoned into these slots.</summary>
    public override int BaseOrbSlotCount => 3;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<LightningJavelin>(),
        ModelDb.Card<LightningJavelin>(),
        ModelDb.Card<LightningJavelin>(),
        ModelDb.Card<LightningJavelin>(),
        ModelDb.Card<EnshroudingMist>(),
        ModelDb.Card<EnshroudingMist>(),
        ModelDb.Card<EnshroudingMist>(),
        ModelDb.Card<EnshroudingMist>(),
        ModelDb.Card<FieryImpulse>(),
        ModelDb.Card<SavannahLions>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<SolRing>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<PlaneswalkerCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<PlaneswalkerRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<PlaneswalkerPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon_planeswalker.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_planeswalker.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_planeswalker_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_planeswalker.png".CharacterUiPath();

    public override float DeathAnimTime => 1.4f;

    // ---------------------------------------------------------------- combat visuals

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        var path = "planeswalker_combat.png".CharacterUiPath();
        if (!ResourceLoader.Exists(path))
        {
            MainFile.Logger.Warn($"Combat sprite missing at {path}; falling back to placeholder visuals.");
            return null;
        }

        var visuals = NodeFactory<NCreatureVisuals>.CreateFromResource(path);
        var sprite = visuals.GetNodeOrNull<Sprite2D>("Visuals");
        if (sprite == null) return visuals;

        var basePos = sprite.Position;
        var player = new AnimationPlayer { Name = "AnimationPlayer" };
        visuals.AddChild(player);

        var lib = new AnimationLibrary();
        lib.AddAnimation("idle", Idle(basePos));
        lib.AddAnimation("attack", Attack(basePos));
        lib.AddAnimation("hurt", Hurt(basePos));
        lib.AddAnimation("cast", Cast(basePos));
        lib.AddAnimation("powerup", Cast(basePos));
        lib.AddAnimation("die", Die(basePos));
        player.AddAnimationLibrary("", lib);
        player.AnimationFinished += name =>
        {
            if (name != "die") player.Play("idle");
        };
        player.Play("idle");
        return visuals;
    }

    private const string PosTrack = "Visuals:position";
    private const string ScaleTrack = "Visuals:scale";
    private const string ModTrack = "Visuals:modulate";
    private const string RotTrack = "Visuals:rotation";

    private static Animation NewAnim(float length, bool loop)
    {
        return new Animation { Length = length, LoopMode = loop ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None };
    }

    private static int Track(Animation anim, string path, params (float time, Variant value)[] keys)
    {
        int t = anim.AddTrack(Animation.TrackType.Value);
        anim.TrackSetPath(t, new NodePath(path));
        anim.TrackSetInterpolationType(t, Animation.InterpolationType.Cubic);
        foreach (var (time, value) in keys) anim.TrackInsertKey(t, time, value);
        return t;
    }

    /// <summary>Slow breathing bob.</summary>
    private static Animation Idle(Vector2 p)
    {
        var a = NewAnim(2.6f, true);
        Track(a, PosTrack, (0f, p), (1.3f, p + new Vector2(0, -7)), (2.6f, p));
        Track(a, ScaleTrack, (0f, Vector2.One), (1.3f, new Vector2(1.01f, 0.99f)), (2.6f, Vector2.One));
        return a;
    }

    /// <summary>Quick lunge toward the enemies and recoil back.</summary>
    private static Animation Attack(Vector2 p)
    {
        var a = NewAnim(0.45f, false);
        Track(a, PosTrack, (0f, p), (0.08f, p + new Vector2(-18, 0)), (0.2f, p + new Vector2(70, -6)), (0.45f, p));
        Track(a, ScaleTrack, (0f, Vector2.One), (0.2f, new Vector2(1.08f, 0.95f)), (0.45f, Vector2.One));
        return a;
    }

    /// <summary>Knockback shake and a red flash.</summary>
    private static Animation Hurt(Vector2 p)
    {
        var a = NewAnim(0.4f, false);
        Track(a, PosTrack, (0f, p), (0.06f, p + new Vector2(-22, 0)), (0.16f, p + new Vector2(10, 0)), (0.26f, p + new Vector2(-6, 0)), (0.4f, p));
        Track(a, ModTrack, (0f, new Color(1, 0.45f, 0.45f)), (0.4f, Colors.White));
        return a;
    }

    /// <summary>Spell cast: rise and glow.</summary>
    private static Animation Cast(Vector2 p)
    {
        var a = NewAnim(0.5f, false);
        Track(a, PosTrack, (0f, p), (0.25f, p + new Vector2(0, -24)), (0.5f, p));
        Track(a, ScaleTrack, (0f, Vector2.One), (0.25f, new Vector2(1.06f, 1.06f)), (0.5f, Vector2.One));
        Track(a, ModTrack, (0f, Colors.White), (0.25f, new Color(1.3f, 1.25f, 0.9f)), (0.5f, Colors.White));
        return a;
    }

    /// <summary>Collapse and fade.</summary>
    private static Animation Die(Vector2 p)
    {
        var a = NewAnim(1.4f, false);
        Track(a, RotTrack, (0f, 0f), (1.0f, -1.3f), (1.4f, -1.3f));
        Track(a, PosTrack, (0f, p), (1.0f, p + new Vector2(-30, 40)), (1.4f, p + new Vector2(-30, 40)));
        Track(a, ModTrack, (0f, Colors.White), (0.6f, new Color(0.6f, 0.6f, 0.6f, 1f)), (1.4f, new Color(0.3f, 0.3f, 0.3f, 0f)));
        return a;
    }
}

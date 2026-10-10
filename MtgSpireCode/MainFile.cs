using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace Planeswalker.MtgSpireCode;

/// <summary>Mod entry point. The game calls Initialize() once all mod assemblies are loaded.</summary>
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "Planeswalker"; // must match Planeswalker.json "id" and the res:// asset folder
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();
        Harmony harmony = new(ModId);
        harmony.PatchAll(assembly);
        Logger.Info("The Planeswalker has arrived on this plane.");
    }
}

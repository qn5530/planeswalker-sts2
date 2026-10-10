using Godot;

namespace Planeswalker.MtgSpireCode.Extensions;

/// <summary>Helpers that resolve asset paths inside the mod's pck, falling back to placeholders.</summary>
public static class StringExtensions
{
    public static string ImagePath(this string path) => string.Join("/", [MainFile.ResPath, "images", path]);

    private static string WithFallback(string path, string fallback, string what)
    {
        if (ResourceLoader.Exists(path)) return path;
        MainFile.Logger.Info($"Could not find {what} image path: {path}");
        return fallback;
    }

    public static string CardImagePath(this string path) =>
        WithFallback(string.Join("/", [MainFile.ResPath, "images", "card_portraits", path]),
            string.Join("/", [MainFile.ResPath, "images", "card_portraits", "card.png"]), "card");

    public static string BigCardImagePath(this string path) =>
        WithFallback(string.Join("/", [MainFile.ResPath, "images", "card_portraits", "big", path]),
            string.Join("/", [MainFile.ResPath, "images", "card_portraits", "big", "card.png"]), "big card");

    public static string PowerImagePath(this string path) =>
        WithFallback(string.Join("/", [MainFile.ResPath, "images", "powers", path]),
            string.Join("/", [MainFile.ResPath, "images", "powers", "power.png"]), "power");

    public static string BigPowerImagePath(this string path) =>
        WithFallback(string.Join("/", [MainFile.ResPath, "images", "powers", "big", path]),
            string.Join("/", [MainFile.ResPath, "images", "powers", "big", "power.png"]), "big power");

    public static string RelicImagePath(this string path) =>
        WithFallback(string.Join("/", [MainFile.ResPath, "images", "relics", path]),
            string.Join("/", [MainFile.ResPath, "images", "relics", "relic.png"]), "relic");

    public static string BigRelicImagePath(this string path) =>
        WithFallback(string.Join("/", [MainFile.ResPath, "images", "relics", "big", path]),
            string.Join("/", [MainFile.ResPath, "images", "relics", "big", "relic.png"]), "big relic");

    public static string CharacterUiPath(this string path) => string.Join("/", [MainFile.ResPath, "images", "charui", path]);
}

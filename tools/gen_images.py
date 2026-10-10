"""Builds every image the mod needs from the Scryfall art crops.
Run: python -I -X utf8 gen_images.py <project_root> <art_dir> <card_art_map.json>
"""
import json, math, os, re, sys
from PIL import Image, ImageDraw, ImageOps, ImageFilter

ROOT, ART, MAP = sys.argv[1], sys.argv[2], sys.argv[3]
IMG = os.path.join(ROOT, "Planeswalker", "images")
cardmap = json.load(open(MAP))

def slug(name): return "".join(ch if ch.isalnum() else "_" for ch in name.lower()).strip("_")
def snake(cls): return re.sub(r"(?<!^)(?=[A-Z])", "_", cls).lower()
def art(name): return Image.open(os.path.join(ART, slug(name) + ".jpg")).convert("RGB")
def fit(im, w, h):
    """Center-crop to the target aspect, then resize (high quality)."""
    return ImageOps.fit(im, (w, h), Image.LANCZOS, centering=(0.5, 0.4))
def circle(im, size, border=(224, 184, 74, 255), bw=3):
    im = fit(im, size, size).convert("RGBA")
    mask = Image.new("L", (size * 4, size * 4), 0)
    ImageDraw.Draw(mask).ellipse((0, 0, size * 4 - 1, size * 4 - 1), fill=255)
    mask = mask.resize((size, size), Image.LANCZOS)
    out = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    out.paste(im, (0, 0), mask)
    d = ImageDraw.Draw(out)
    d.ellipse((bw // 2, bw // 2, size - 1 - bw // 2, size - 1 - bw // 2), outline=border, width=bw)
    return out
def save(im, *parts):
    p = os.path.join(IMG, *parts); os.makedirs(os.path.dirname(p), exist_ok=True); im.save(p)

# cards: 1000x760 big + 250x190 small
for cls, mtg in cardmap.items():
    a = art(mtg)
    save(fit(a, 1000, 760), "card_portraits", "big", snake(cls) + ".png")
    save(fit(a, 250, 190), "card_portraits", snake(cls) + ".png")

# powers: 64x64 small + 256x256 big
for power, mtg in {"chandra_power": "Chandra, Pyromaster", "jace_power": "Jace, Vryn's Prodigy",
                   "llanowar_power": "Llanowar Elves", "kiln_fiend_power": "Kiln Fiend", "counterspell_power": "Counterspell",
                   "comeuppance_power": "Comeuppance", "dark_confidant_power": "Dark Confidant",
                   "ajanis_pridemate_power": "Ajani's Pridemate"}.items():
    a = art(mtg)
    save(circle(a, 64, bw=2), "powers", power + ".png")
    save(circle(a, 256, bw=6), "powers", "big", power + ".png")

# mana powers: colored disc + letter, 64 small / 256 big
from PIL import ImageFont
def mana_icon(size, fill, letter, text_fill):
    s4 = size * 4
    im = Image.new("RGBA", (s4, s4), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    rim = max(2, s4 // 16)
    d.ellipse((0, 0, s4 - 1, s4 - 1), fill=(224, 184, 74, 255))
    d.ellipse((rim, rim, s4 - 1 - rim, s4 - 1 - rim), fill=fill + (255,))
    try:
        f = ImageFont.truetype("C:/Windows/Fonts/arialbd.ttf", int(s4 * 0.62))
    except Exception:
        f = ImageFont.load_default()
    bb = d.textbbox((0, 0), letter, font=f)
    d.text(((s4 - (bb[2] - bb[0])) / 2 - bb[0], (s4 - (bb[3] - bb[1])) / 2 - bb[1]), letter, font=f, fill=text_fill + (255,))
    return im.resize((size, size), Image.LANCZOS)
for name, fill, letter, tf in [("white_mana_power", (249, 250, 244), "W", (120, 100, 40)), ("blue_mana_power", (14, 104, 171), "U", (240, 248, 255)),
                               ("black_mana_power", (40, 30, 48), "B", (220, 200, 230)), ("red_mana_power", (211, 32, 42), "R", (255, 240, 200)),
                               ("green_mana_power", (0, 115, 62), "G", (230, 255, 230))]:
    save(mana_icon(64, fill, letter, tf), "powers", name + ".png")
    save(mana_icon(256, fill, letter, tf), "powers", "big", name + ".png")

# creature orbs: 64px slot icon + 96px battlefield sprite
for orb, mtg in {"savannah_lions_orb": "Savannah Lions", "grizzly_bears_orb": "Grizzly Bears",
                 "shivan_dragon_orb": "Shivan Dragon", "vampire_nighthawk_orb": "Vampire Nighthawk",
                 "serra_angel_orb": "Serra Angel", "birds_of_paradise_orb": "Birds of Paradise"}.items():
    a = art(mtg)
    save(circle(a, 64, bw=2), "orbs", orb + ".png")
    save(circle(a, 96, bw=4), "orbs", "sprite", orb + ".png")

# relics: 94x94 + outline + 256 big
for relic, mtg in {"sol_ring": "Sol Ring", "chromatic_lantern": "Chromatic Lantern",
                   "senseis_diving_top": "Sensei's Divining Top"}.items():
    a = art(mtg)
    small = circle(a, 94, bw=3)
    save(small, "relics", relic + ".png")
    alpha = small.split()[3]
    outline = Image.new("RGBA", small.size, (255, 255, 255, 0))
    outline.putalpha(alpha.filter(ImageFilter.MaxFilter(5)))
    save(outline, "relics", relic + "_outline.png")
    save(circle(a, 256, bw=8), "relics", "big", relic + ".png")

# character UI art is hand-made (charui/); only the energy icon is regenerated here.
# energy icon: five-color mana wheel (W U B R G) with a gold rim
def mana_wheel(size):
    s = size * 4
    im = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    cols = [(249, 250, 244), (14, 104, 171), (21, 11, 0), (211, 32, 42), (0, 115, 62)]
    for i, c in enumerate(cols):
        d.pieslice((0, 0, s - 1, s - 1), start=-90 + i * 72, end=-90 + (i + 1) * 72, fill=c + (255,))
    rim = max(2, s // 14)
    d.ellipse((rim // 2, rim // 2, s - 1 - rim // 2, s - 1 - rim // 2), outline=(224, 184, 74, 255), width=rim)
    inner = s // 3
    d.ellipse((s // 2 - inner // 2, s // 2 - inner // 2, s // 2 + inner // 2, s // 2 + inner // 2), fill=(224, 184, 74, 255))
    return im.resize((size, size), Image.LANCZOS)
save(mana_wheel(74), "charui", "big_energy.png")
save(mana_wheel(24), "charui", "text_energy.png")
print("images done")

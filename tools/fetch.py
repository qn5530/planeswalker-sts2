import json, time, urllib.request, urllib.parse, sys, os
names = ["Lightning Javelin","Enshrouding Mist","Fiery Impulse","Lightning Bolt","Shock","Flame Slash","Lava Axe","Grizzly Bears",
"Divination","Ponder","Wall of Omens","Giant Growth","Disfigure","Revitalize","Mighty Leap",
"Lightning Helix","Char","Fling","Exquisite Firecraft","Fireball","Dark Ritual","Brainstorm","Harmonize","Pacifism","Regrowth","Claustrophobia",
"Glorious Anthem","Thornweald Archer","Serra Angel","Llanowar Elves",
"Wrath of God","Black Lotus","Ancestral Recall","Time Warp",
"Chandra, Pyromaster","Jace, Vryn's Prodigy","Liliana, Heretical Healer","Gideon, Battle-Forged","Nissa, Vastwood Seer",
"Sol Ring","Chromatic Lantern","Sensei's Divining Top",
"Savannah Lions","Vampire Nighthawk","Shivan Dragon","Wall of Stone","Royal Assassin","Birds of Paradise","Rampant Growth","Altar's Reap","Overrun",
"Fog","Counterspell","Comeuppance","Condemn","Ball Lightning","Rancor","Demonic Tutor","Reanimate","Fireblast","Doom Blade","Clone","Grapeshot","Dark Confidant","Unsummon","Ajani's Pridemate",
"Rite of Flame","Kiln Fiend","Blue Sun's Zenith","Craterhoof Behemoth","Gray Merchant of Asphodel"]
out = json.load(open("scryfall_cards.json",encoding="utf-8")) if os.path.exists("scryfall_cards.json") else {}
for n in names:
    if n in out: continue
    url = "https://api.scryfall.com/cards/named?exact=" + urllib.parse.quote(n)
    req = urllib.request.Request(url, headers={"User-Agent":"MTGSpireMod/0.1","Accept":"application/json"})
    try:
        d = json.load(urllib.request.urlopen(req))
    except Exception as e:
        print("FAIL", n, e); continue
    face = d.get("card_faces",[None])[0] if "image_uris" not in d else None
    img = (face or d).get("image_uris",{})
    out[n] = {"name": d["name"], "set": d["set"], "set_name": d.get("set_name"), "type_line": d["type_line"], "mana_cost": d.get("mana_cost"),
              "oracle": (face or d).get("oracle_text"), "artist": d.get("artist"), "art_crop": img.get("art_crop"), "scryfall": d["scryfall_uri"]}
    print("ok", n, "|", d["set"], "|", d.get("artist"))
    time.sleep(1.2)
json.dump(out, open("scryfall_cards.json","w",encoding="utf-8"), indent=1, ensure_ascii=False)

# download art crops
os.makedirs("art", exist_ok=True)
for n, d in out.items():
    if not d["art_crop"]: print("no art", n); continue
    fn = "art/" + "".join(ch if ch.isalnum() else "_" for ch in n.lower()).strip("_") + ".jpg"
    if os.path.exists(fn): continue
    req = urllib.request.Request(d["art_crop"], headers={"User-Agent":"MTGSpireMod/0.1"})
    open(fn,"wb").write(urllib.request.urlopen(req).read()); time.sleep(1.2)
print("art done", len(os.listdir("art")))

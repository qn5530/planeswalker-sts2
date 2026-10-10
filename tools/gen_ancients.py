"""Writes ancients.json for the Planeswalker by mirroring the Ironclad's dialogue key structure."""
import json, sys
vanilla = json.load(open(sys.argv[1], encoding="utf-8"))
out_path = sys.argv[2]
CH = "PLANESWALKER-THE_PLANESWALKER"
text = {
 "DARV.0-0.ancient": "A walker of planes! Now there's a customer I don't see every day.\nI've got just the thing for ya!",
 "DARV.1-0r.ancient": "Back again? Good to see ya still slingin' spells!",
 "DARV.2-0.ancient": "Word is you can step right off this world whenever you like... so why stay?",
 "DARV.2-0.next": "Continue",
 "DARV.2-1.char": "[i][font_size=22]The Planeswalker studies the Spire above.[/font_size][/i]",
 "DARV.2-1.next": "Continue",
 "DARV.2-2.ancient": "Maybe the Architect knows somethin'.",
 "NEOW.0-0.ancient": "[sine]..a spark.. from beyond... this plane...\n..climb... and..be..tested...[/sine]",
 "NEOW.1-0r.ancient": "[sine]..the spark..still burns... good...[/sine]",
 "NEOW.2-0.ancient": "[sine]..remember.. the.. first..time\n...you ..walked...before me..[/sine]",
 "NEOW.2-0.next": "Continue",
 "NEOW.2-1.ancient": "[sine]..so..many..worlds...[/sine]",
 "NEOW.2-1.next": "Continue",
 "NEOW.2-2.ancient": "[sine]...and..yet..you..return...[/sine]",
 "NONUPEIPE.0-0.ancient": "Ah, a mage of many colors arrives before me.\nCome forth and I shall endow you with my power.",
 "NONUPEIPE.1-0r.ancient": "Planeswalker, planeswalker... was my previous gift not enough?",
 "NONUPEIPE.2-0.ancient": "You return again my champion? How many times?\nWere five colors of mana not enough?",
 "OROBAS.0-0.ancient": "A Planeswalker!? Here?? Strange. Very strange!!",
 "OROBAS.1-0r.ancient": "World-hopper? Why stay!? Bad choice? Yes. Mistake!",
 "OROBAS.2-0.ancient": "Wishing you home!! Wishing hard!\nVakuu bad. Very bad! Architect free you?",
 "PAEL.0-0.ancient": "[thinky_dots]So many worlds behind your eyes. Take from me and rest a while...[/thinky_dots]",
 "PAEL.1-0r.ancient": "[thinky_dots]Do you dream of other planes?\nIt is good to sleep...[/thinky_dots]",
 "PAEL.2-0.ancient": "[thinky_dots]You must quiet your spark.\nFather can free you...[/thinky_dots]",
 "TANX.0-0.ancient": "IMPRESSIVE MAGIC, MAGE! YOU CAN USE MORE WEAPONS!",
 "TANX.1-0r.ancient": "WEAPONS FOR THE MAGE! GRAB A WEAPON! LET'S FIGHT!",
 "TANX.2-0.ancient": "FIGHT ME, PLANESWALKER!!",
 "TANX.2-0.next": "Fight",
 "TANX.2-1.char": "[i][font_size=22]The Planeswalker fights Tanx.[/font_size][/i]",
 "TANX.2-1.next": "Continue",
 "TANX.2-2.ancient": "YEEES!! MIGHTY FIGHT, TAKE WEAPON!",
 "TEZCATARA.0-0.ancient": "You [jitter][b]shimmer[/b][/jitter] so pretty! Come in, come in!",
 "TEZCATARA.1-0r.ancient": "Such a clever one you are! Come and eat something.\nCan't [jitter][b]cast[/b][/jitter] on an empty stomach!",
 "TEZCATARA.2-0.ancient": "Ohh, Architect still alive? Kill him [jitter][b]quickly[/b][/jitter] would you?\nI'll be waiting right here!",
 "THE_ARCHITECT.0-0r.ancient": "So a visitor from another plane has arrived?",
 "THE_ARCHITECT.0-0r.next": "Threaten",
 "THE_ARCHITECT.0-1r.char": "I have unmade greater things than you.",
 "THE_ARCHITECT.1-0r.ancient": "Bound to this Spire like the rest of them, aren't you?",
 "THE_ARCHITECT.1-0r.next": "Threaten",
 "THE_ARCHITECT.1-1r.char": "Architect... I will walk right through you.",
 "THE_ARCHITECT.1-1r.next": "Continue",
 "THE_ARCHITECT.1-2r.ancient": "Then why haven't you left?",
 "THE_ARCHITECT.2-0r.ancient": "I pity you.",
 "THE_ARCHITECT.2-0r.next": "Threaten",
 "THE_ARCHITECT.2-1r.char": "Save it.",
 "THE_ARCHITECT.2-1r.next": "Continue",
 "THE_ARCHITECT.2-2r.ancient": "Return to the bottom, Planeswalker.",
 "VAKUU.0-0.ancient": "Imagine seeing [sine]you[/sine] again after so long.",
 "VAKUU.0-0.next": "Awaken",
 "VAKUU.0-1.char": "[jitter][b]YOU![/b][/jitter]\nWhat have you done to my spark!?",
 "VAKUU.0-1.next": "Continue",
 "VAKUU.0-2.ancient": "Exactly what you bargained for.",
 "VAKUU.1-0r.ancient": "Do the lost worlds weigh on you...?\n[i][font_size=22]Vakuu howls with laughter.[/font_size][/i]",
 "VAKUU.2-0.ancient": "My wandering puppet... why haven't you killed the Architect yet?",
 "VAKUU.2-0.next": "Awaken",
 "VAKUU.2-1.char": "After I kill the Architect, I will kill [jitter][b]YOU![/b][/jitter]",
 "VAKUU.2-1.next": "Continue",
 "VAKUU.2-2.ancient": "[i][font_size=22][sine]Yawn[/sine][/font_size][/i]\nI look forward to watching you fail.",
}
out = {}
missing = []
for k in vanilla:
    if ".talk.IRONCLAD." not in k: continue
    ancient, rest = k.split(".talk.IRONCLAD.")
    short = f"{ancient}.{rest}"
    if short not in text: missing.append(short); continue
    out[f"{ancient}.talk.{CH}.{rest}"] = text[short]
assert not missing, missing
json.dump(out, open(out_path, "w", encoding="utf-8"), indent=2, ensure_ascii=False)
print("ancients keys:", len(out))

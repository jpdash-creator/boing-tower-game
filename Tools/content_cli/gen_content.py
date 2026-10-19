"""Generates the baseline Content/ JSON from doc 08 (rules addendum).

Run from the repo root:  python Tools/content_cli/gen_content.py
The output is committed; re-run only when doc 08 changes. Hand edits to generated files
will be overwritten, so put long-lived overrides in the level 'overrides' block via this script.
"""
import json
import os
import re

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
CONTENT = os.path.join(ROOT, "Content")

# (region name, palette, signature module name, module description)
REGIONS = [
    ("Meadow Springs", ["green", "pink", "sky blue"], "Bounce Flowers", "Tutorial region; bouncy flower platforms, no hazards"),
    ("Mushroom Grove", ["purple", "teal"], "Bounce Caps", "Bounce-cap platforms with a high launch"),
    ("Sunny Beach", ["gold", "aqua"], "Drifting Platforms", "Platforms sway horizontally"),
    ("Candy Canyon", ["pink", "mint"], "Sticky Gum", "Sticky platforms reduce jump height"),
    ("Bamboo Temple", ["jade", "red"], "Swinging Gongs", "Swinging gongs knock the player back"),
    ("Frosty Peaks", ["ice blue", "white"], "Slippery Platforms", "Low-friction platforms"),
    ("Autumn Orchard", ["orange", "brown"], "Wind Gusts", "Gusts push the player left or right"),
    ("Desert Oasis", ["sand", "turquoise"], "Crumbling Sand", "Sand platforms crumble after landing"),
    ("Pirate Cove", ["navy", "gold"], "Cannonballs", "Cannonballs fly in from the sides"),
    ("Toy Factory", ["red", "blue", "yellow"], "Conveyor Platforms", "Conveyor platforms move the player"),
    ("Haunted Manor", ["violet", "green glow"], "Ghost Platforms", "Vanishing / ghost platforms"),
    ("Crystal Caverns", ["cyan", "magenta"], "Crystal Bounce", "Reflective crystals give an angled rebound"),
    ("Volcano Forge", ["red", "charcoal"], "Rising Lava", "Rising lava plus timed fire jets"),
    ("Jungle Ruins", ["emerald", "amber"], "Vine Swing", "Grab and release vines"),
    ("Neon City", ["electric blue", "pink"], "Laser Gates", "Timed laser gates"),
    ("Cloud Kingdom", ["white", "lavender"], "Dissolving Clouds", "Dissolving clouds plus lightning flashes"),
    ("Steampunk Skyport", ["brass", "teal"], "Steam Vents", "Steam vents give upward lift"),
    ("Deep Sea Reef", ["deep blue", "coral"], "Water Currents", "Low-gravity water currents"),
    ("Space Station", ["black", "cyan"], "Zero-G Zones", "Zero-g zones with floaty jumps"),
    ("Moon Carnival", ["rainbow", "neon"], "Carnival Mix", "Trampolines plus rotating wheels"),
    ("Dragon Peak", ["crimson", "gold"], "Fire-Breath Sweeps", "Fire-breath sweeps across the tower"),
    ("Pyramid Sands", ["gold", "lapis"], "Sliding Stone Doors", "Sliding stone doors open and close"),
    ("Viking Fjord", ["steel blue", "frost"], "Ice Floes", "Drifting ice floes plus gusts (reuses Wind Gusts)"),
    ("Wild West Mesa", ["terracotta", "sunset"], "Tumbleweeds And Barrels", "Tumbleweeds and rolling barrels"),
    ("Dino Valley", ["lime", "orange"], "Stomp Quakes", "Stomp quakes shake the platforms"),
    ("Storybook Castle", ["pastel", "royal"], "Drawbridge Platforms", "Tilting drawbridge platforms"),
    ("Arcade Realm", ["pixel", "neon"], "Pixel Snap", "Pixel-grid snap platforms that blink"),
    ("Samurai Dojo", ["ink", "cherry"], "Falling Blades", "Falling blades with warning marks"),
    ("Alien Jungle", ["glow green", "violet"], "Gravity Pods", "Gravity pods with a flip pull"),
    ("Rift Of Time", ["chrono violet", "white"], "Rewind Zones", "Rewind zones leave a position echo"),
]

# Speedrun time goal (level index 9, every region), seconds. Owner decision 2026-10-19: relax the time, keep the normal
# vertical gaps. Value = slowest bot time over regions 1-5 times 1.2 (margin), rounded up to 5 s; see
# Tests/Simulation/SOLVABILITY_REPORT.md and Assets/_Game/Simulation/NOTES.md.
SPEEDRUN_TIME_SEC = 75

# index -> (class, d, goal)
LEVELS = {
    1: ("simple", 0.05, {"type": "reachFloor", "value": 30}),
    2: ("simple", 0.10, {"type": "reachFloor", "value": 45}),
    3: ("simple", 0.15, {"type": "reachFloor", "value": 60}),
    4: ("simple", 0.22, {"type": "collectCoins", "count": 15}),
    5: ("complex", 0.40, {"type": "comboChain", "value": 3, "count": 2}),
    6: ("medium", 0.35, {"type": "reachFloor", "value": 80}),
    7: ("medium", 0.42, {"type": "noWallBounce", "value": 60}),
    8: ("medium", 0.50, {"type": "findHiddenGem"}),
    9: ("medium", 0.58, {"type": "speedrun", "value": 90, "timeSec": SPEEDRUN_TIME_SEC}),
    10: ("complex", 0.70, {"type": "surviveWave"}),
    11: ("hard", 0.62, {"type": "comboChain", "value": 6, "count": 1}),
    12: ("hard", 0.70, {"type": "reachFloor", "value": 100}),
    13: ("hard", 0.78, {"type": "reachFloor", "value": 120}),
    14: ("hard", 0.86, {"type": "reachFloor", "value": 140}),
    15: ("highly_complex", 1.00, {"type": "bossChase", "value": 150}),
}
NORMAL_LEVELS = [1, 2, 3, 4, 6, 7, 8, 9, 11, 12, 13, 14]


def slug(text):
    return re.sub(r"[^a-z0-9]+", "_", text.lower()).strip("_")


def write(rel, data):
    path = os.path.join(CONTENT, rel)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, indent=2)
        f.write("\n")


def coin_payout(index):
    if index in (5, 10):
        return 300
    if index == 15:
        return 900
    k = NORMAL_LEVELS.index(index)
    return int(round((60 + 80 * k / 11) / 5) * 5)


def gem_rewards(index):
    if index == 15:
        return {"chance": 1.0, "min": 5, "max": 5, "firstClearGuaranteed": 5,
                "starBonusMax": 5, "replayChance": 0.5, "replayAmount": 2}
    if index == 10:
        return {"chance": 0.25, "min": 1, "max": 3, "firstClearGuaranteed": 1}
    if index == 5:
        return {"chance": 0.25, "min": 1, "max": 3}
    return {"chance": 0.03, "min": 1, "max": 1}


def load_coin_star_targets():
    """Star-2 targets of regions 1-5 from Content/config/coin_star_targets.json (written by the Simulation test
    CoinStarTargetTests from the bot's proven coin bound). Returns {level id: target}; empty if the file is absent."""
    path = os.path.join(CONTENT, "config", "coin_star_targets.json")
    if not os.path.exists(path):
        return {}
    with open(path, encoding="utf-8") as f:
        table = json.load(f)
    return {lid: e["target"] for lid, e in table["levels"].items()}


COIN_STAR_TARGETS = load_coin_star_targets()


def coin_star_target(level_id, i):
    """Raised target for the five playable regions (from the bot table); the old formula for regions 6-30."""
    return COIN_STAR_TARGETS.get(level_id, 20 + 5 * i)


def make_level(r, region_id, i):
    cls, d, goal = LEVELS[i]
    level_id = "lvl_%03d_%02d" % (r, i)
    level = {
        "id": level_id,
        "region": region_id,
        "index": i,
        "seed": (r * 7919 + i * 104729 + 12345) % 2147483647,
        "class": cls,
        "difficulty": d,
        "goal": goal,
        "stars": [{"type": "clear"}, {"type": "coins", "value": coin_star_target(level_id, i)}, {"type": "noRevive"}],
        "rewards": {"coinPayout": coin_payout(i), "replayCoinPercent": 40, "gems": gem_rewards(i)},
    }
    if i in (5, 10):
        level["overrides"] = {"fixedChunks": ["chunk_complex_a"]}
    if i == 15:
        level["overrides"] = {"fixedChunks": ["chunk_gauntlet_a"], "ramp": True}
    return level


def main():
    modules = []
    region_ids = []
    for n, (name, palette, mod_name, mod_desc) in enumerate(REGIONS, start=1):
        rid = "region_%03d_%s" % (n, slug(name))
        region_ids.append(rid)
        mod_id = "mod_" + slug(mod_name)
        modules.append({"id": mod_id, "name": mod_name, "description": mod_desc,
                        "regions": [n], "buildStatus": "planned"})
        weights = {"normal": 1.0} if n == 1 else {"normal": 1.0, "signature": 0.3}
        region = {
            "id": rid,
            "index": n,
            "nameKey": "region." + slug(name),
            "name": name,
            "status": "live" if n <= 20 else "teaser",
            "unlock": {
                "requires": None if n == 1 else region_ids[n - 2],
                "gemSkip": None if n == 1 else 100 + (n - 2) * 300 // 18,
            },
            "theme": {"bundle": "region_%03d" % n, "palette": palette, "music": None},
            "rules": {"signatureModule": mod_id,
                      "difficultyOffset": round(0.25 * (n - 1) / 29, 4),
                      "platformWeights": weights, "hazards": []},
            "coinFactor": round(1 + 0.03 * (n - 1), 2),
            "levels": ["lvl_%03d_%02d" % (n, i) for i in range(1, 16)],
            "availableFrom": None,
        }
        write("regions/%s.json" % rid, region)
        write("levels/%s_levels.json" % rid, [make_level(n, rid, i) for i in range(1, 16)])

    write("config/modules.json", modules)

    packs = [("handful", 0.99, 80), ("pouch", 4.99, 440), ("sack", 9.99, 920),
             ("chest", 19.99, 1900), ("vault", 49.99, 5000), ("treasury", 99.99, 10500)]
    write("economy/gem_packs.json", [
        {"id": "gempack_" + p, "nameKey": "pack." + p, "usd": usd, "gems": gems,
         "sku": "boingtower.gems." + p} for p, usd, gems in packs])

    write("items/items.json", [{
        "id": "boots_cloud_runner", "slot": "boots", "rarity": "rare", "bundle": "items_boots_01",
        "sprites": {"left": "boots_cloud_runner_L", "right": "boots_cloud_runner_R"},
        "price": {"gems": 150}, "tags": ["cloud", "season1"], "availableFrom": None, "availableTo": None}])

    write("config/remote_config.json", {
        "ramp": {"rampFloors": 25, "rampPercent": 4, "rampCap": 150, "playerSpeedShare": 0.5},
        "generator": {"endlessDifficultyFloors": 400, "screenWidthUnits": 10},
        "score": {"floorPoints": 10, "comboPointsMultiplier": 5, "coinPoints": 1},
        "economy": {"coinsPerGem": 100, "replayCoinPercent": 40, "regionCoinScalePercent": 3,
                    "vipGemsPerDay": 25, "endlessCoinDailyFullRateRuns": 3,
                    "endlessCoinOverCapMultiplier": 0.5},
    })
    print("generated %d regions, %d levels" % (len(REGIONS), len(REGIONS) * 15))


if __name__ == "__main__":
    main()

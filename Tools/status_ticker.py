"""Prints a compact roadmap status for the '5 regions x 15 levels' milestone. Used by the 5-minute background ticker.

    python Tools/status_ticker.py
Reads .claude/roadmap-5-regions.json (kept current by Claude) plus live signals: Unity running, last headless step logs, git head.
"""
import datetime
import glob
import json
import os
import subprocess

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))


def run(cmd):
    try:
        return subprocess.run(cmd, capture_output=True, text=True, cwd=ROOT, timeout=20).stdout.strip()
    except Exception:
        return ""


def main():
    with open(os.path.join(ROOT, ".claude", "roadmap-5-regions.json"), encoding="utf-8") as f:
        data = json.load(f)
    tasks = data["tasks"]
    left = [t for t in tasks if t["status"] != "done"]
    core = [t for t in left if t["id"] != "T13"]
    blocked = [t for t in left if t["status"] == "blocked"]
    print("=== %s | 5 regions x 15 levels | %d/%d tasks done ===" % (datetime.datetime.now().strftime("%H:%M"), len(tasks) - len(left), len(tasks)))
    print("%-4s %-8s %5s  %s" % ("ID", "STATUS", "MIN", "TASK  [BLOCKER]"))
    for t in tasks:
        if t["status"] == "done":
            continue
        print("%-4s %-8s %5d  %s  [%s]" % (t["id"], t["status"].upper(), t["minutes"], t["task"][:78], t["blocker"][:60]))
    print("Remaining Claude work: %d min core (%d with endless) | blocked on owner/hardware: %s" %
          (sum(t["minutes"] for t in core), sum(t["minutes"] for t in left), ", ".join(t["id"] for t in blocked) or "none"))
    unity = "running" if "Unity.exe" in run(["tasklist", "/FI", "IMAGENAME eq Unity.exe"]) else "idle"
    logs = sorted(glob.glob("E:/Unity/logs/s*_*.log"), key=os.path.getmtime)
    last = ""
    if logs:
        lf = logs[-1]
        with open(lf, encoding="utf-8", errors="ignore") as f:
            txt = f.read()
        last = "%s: errorCS=%d%s" % (os.path.basename(lf), txt.count("error CS"), " LICENSE-ERROR" if "No valid Unity Editor license" in txt else "")
    print("Signals: Unity %s | last headless log %s | git %s" % (unity, last or "n/a", run(["git", "log", "--oneline", "-1"])[:70]))
    dirty = len([l for l in run(["git", "status", "--short"]).splitlines() if l.strip()])
    print("Uncommitted files: %d" % dirty)


if __name__ == "__main__":
    main()

"""On-device smoke test: fresh install, walk through every screen, play a few seconds, check layout, state and frame time.
Run:  python Tools/android_test/smoke_test.py [--no-install]
Exit code 0 = all checks passed. Screenshots go to E:/Unity/device_shots (FAIL_<step>.png on a failure)."""
import json
import os
import sys
import time

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from bt import Device, TestFailure  # noqa: E402

STEPS = []
d = Device()


def step(fn):
    STEPS.append(fn)
    return fn


def check_screen(name):
    d.screenshot(name)
    d.assert_no_crash()
    d.assert_ui_inside_screen()
    d.assert_touch_targets()
    d.assert_no_overlap()


@step
def menu():
    d.wait_screen("Menu", 30)
    d.wait_element("PLAY")
    check_screen("01_menu")


@step
def settings_and_back():
    d.tap("Settings")
    d.wait_screen("Settings")
    time.sleep(1.5)
    check_screen("02_settings")
    d.tap("Back")
    d.wait_screen("Menu")


@step
def map_screen():
    d.tap("PLAY")
    d.wait_screen("Map")
    time.sleep(1.5)
    check_screen("03_map")


@step
def level_select():
    d.tap(name="Region1")
    d.wait_screen("Levels")
    time.sleep(1.5)
    check_screen("04_levels")


@step
def level_popup():
    d.tap(name="Level1")
    d.wait_element("Close")
    time.sleep(1.0)
    check_screen("05_level_popup")


@step
def play_level():
    d.tap("PLAY")
    d.wait(lambda: d.state.get("scene") == "Run", 20, "Run scene")
    d.wait(lambda: not d.state.get("intro", True), 10, "intro to end")
    time.sleep(0.5)
    d.screenshot("06_run_start")
    d.assert_touch_targets()
    for _ in range(4):
        for pad in ("PadRight", "PadLeft"):
            if d.state.get("over"):
                break          # the run ended (a fall is expected with random taps); the end overlay is checked in the next step
            d.multitouch_hold_and_tap(pad, "PadJump", hold_ms=1400, taps=3)
    d.screenshot("07_run_playing")
    d.assert_no_crash()
    s = dict(d.state)
    print("run state:", json.dumps(s))
    if s.get("tick", 0) < 100:
        raise TestFailure("simulation did not advance")
    if s.get("avgMs", 0) > 2.0:
        raise TestFailure("average script time %.2f ms exceeds 2 ms" % s["avgMs"])
    if s.get("fps", 60) < 50 and not s.get("overlay"):
        raise TestFailure("fps %s below 50 while playing" % s.get("fps"))


@step
def end_overlay_or_pause():
    if d.state.get("over") or d.state.get("overlay"):
        time.sleep(1.0)
        check_screen("08_run_end_overlay")
        return
    d.tap(name="Pause")
    d.wait(lambda: d.state.get("overlay"), 5, "pause overlay")
    time.sleep(0.8)
    check_screen("08_pause")
    d.tap("Resume")


def main():
    if not d.connected():
        print("no device connected")
        return 2
    if "--no-install" not in sys.argv:
        d.install()
    d.launch(clear=True)
    failed = 0
    for fn in STEPS:
        t = time.time()
        try:
            fn()
            print("PASS %-24s %.1fs" % (fn.__name__, time.time() - t))
        except TestFailure as e:
            failed += 1
            print("FAIL %-24s %s" % (fn.__name__, e))
            try:
                d.screenshot("FAIL_" + fn.__name__)
            except Exception:
                pass
            break
    d.close()
    print("RESULT:", "PASSED" if not failed else "FAILED")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())

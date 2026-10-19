# Handoff (paused by the owner for a laptop restart, 2026-10-19)

State: everything committed (HEAD 164641f or later on `main`). No agents, no Unity Editor, no builds running. OpenArt balance 8,975 credits (Plus plan); nothing pending there.

## Resume checklist
1. Open Unity Hub once and make sure you are signed in with an active Unity Personal license (headless Unity exits 198 without it).
2. Environment (user-level, persists): `DOTNET_ROOT=E:\dotnet`, `NUGET_PACKAGES=E:\dotnet\nuget`, `DOTNET_CLI_HOME=E:\dotnet\cli-home`, `E:\dotnet` on PATH. Art scripts need `PYTHONPATH=E:/pylibs` (SciPy). Unity Editor 6000.3.26f1 is at `E:\Unity\Hub\Editor\6000.3.26f1\`; logs go to `E:\Unity\logs\`. Keep caches off C: (C: was nearly full).
3. Read `CLAUDE.md`, `.claude/roadmap-5-regions.json` (`python Tools/status_ticker.py`) and `.claude/pendingdecisions/` first.
4. Quick health check: `dotnet test Tests/Simulation` (80 tests, ~2 min) and the Unity EditMode run (52 tests, see CLAUDE.md for the command).

## Where we are
Done: T0 Unity setup, T1 art pipeline (66 owner-picked assets sliced to 296 sprites), T2 runtime content, T3 gameplay completeness, T4 bot (75/75 levels solved), T8 save/wallet/rewards/unlocks.
Next (unblocked): T5 Run scene (renderer, camera, parallax, HUD, level flow), T6 input, T7 menus/map/level select/results, then T12 end-to-end test, optional T13 endless.
Blocked on the owner: T9 playtest tuning, T10 audio source decision, T11 Android phone/emulator for a device build.
Owner decisions waiting: see `.claude/pendingdecisions/open-items.md` (T1 art limitations, T3/T4 design questions, T8 economy assumptions, hibernation file, temp files on C:, second Unity Editor 6000.6.6f1).

## Guardrails (mandatory)
- OpenArt: never spend credits without the owner's explicit approval of the exact batch (`.claude/rules/openart-credit-guardrail.md`).
- No git commands from subagents; the main session commits. One Unity instance at a time.

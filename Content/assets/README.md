# Asset production workflow (OpenArt)

Status: **planning only. No credits have been spent.** The owner is on the OpenArt **Plus** plan (12,000 credits/month).
The mandatory guardrail is `.claude/rules/openart-credit-guardrail.md`: no generation without the owner's explicit approval of the exact batch.

## Files
- `manifest.json` - every asset request (id, prompt, aspect, size, mode, references, dependencies, save path, QA checks, planning cost, batch). Generated.
- `CATALOG.md` - totals, credit scenarios, and the ordered list of approval batches. Generated.
- `catalog/<class>.md` - the full per-asset detail with the exact prompt. Generated.
- `approvals/<batch>.md` - approval packets prepared for the owner (one per batch). Written by the prep agent, read-only OpenArt calls only.
- `Tools/asset_cli/gen_asset_manifest.py` - the generator. Edit this, then re-run; never hand-edit generated files.
- `Tools/asset_cli/APPROVAL_TEMPLATE.md` - the exact format of an approval packet.

## Roles
| Who | May do | May NOT do |
|---|---|---|
| Prep agent (background) | Read-only OpenArt calls (account, model list/cost/form, creation list), verify prompts against the manifest and style lock, write approval packets | Call `openart_generate_image` / `openart_generate_video`, upload, or create anything that costs credits |
| Main session (after the owner's yes) | Run exactly the approved batch, report actual credits used and the balance | Run anything not in the approved packet, retry, or vary |
| Owner | Approve / change / reject each batch; pick winners; upload the reference image (OpenArt widget step) | - |

## Per-batch flow
1. Prep agent writes `approvals/<batch>.md` from the template: purpose, MCP and tool, model and mode, exact form parameters, each prompt word for word, images per asset, per-job credits from `openart_model_cost`, total, balance before/after, cheaper alternatives, where results go, what the owner must decide afterwards.
2. Main session shows the packet to the owner and waits for a clear yes in chat. Changes mean a new packet.
3. After the yes, only the approved calls run. Results are reviewed in OpenArt (`openart_creation_show`), the owner picks winners, the pick is recorded in `assets_log.csv` (id, model, settings, seed, prompt, date, credits).
4. Post-processing (key removal `#808080`, trim, pivot, mirror boots, slice, atlas) is scripted or manual and costs no credits.
5. Next batch only after the owner reviews the previous one. Early batches (B00-B04) set the model, style and pipeline for everything after them.

## Hard rules for every prompt
- Always the STYLE LOCK v2 (see `CATALOG.md`). Backgrounds that must be keyed use flat `#808080`.
- No text, letters or numerals inside art (text is rendered by Unity UI).
- Never name another game, character, brand or studio. Describe, do not reference.
- Boots are generated once and mirrored. Outfits/hats/accessories use the rig template as an image reference when available.
- One model per asset class once locked in Gate 0 (B00). Do not switch mid-set.

## Budget reality (planning numbers)
Full catalog at the default tiers is about 17,280 credits (1,118 images), roughly 1.4 months of Plus. Cheaper models or fewer candidates change this a lot, see the sensitivity table in `CATALOG.md`. Credits per image depend on the model; each batch is re-quoted before approval.

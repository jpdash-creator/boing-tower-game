# Approval packet: <BATCH ID> - <title>

> Prepared with READ-ONLY OpenArt calls. Nothing has been generated. Credits are spent only after the owner replies with a clear yes to THIS packet.

## 1. What and why
- Batch: <id>, position <n> of <total>
- Assets in this batch: <ids and names>
- Why now / what it unlocks: <dependency chain>
- What happens if rejected: <impact>

## 2. MCP and tools to be called
- MCP server: OpenArt MCP (connector id as shown in the tool name)
- Tool: `openart_generate_image` (and no other credit-consuming tool)
- Number of calls: <n> (one per prompt, sequential / parallel: <how>)
- Read-only tools already used for this packet: `openart_account_get`, `openart_model_list`, `openart_model_form_get`, `openart_model_cost`

## 3. Model and exact settings (from `openart_model_form_get`)
| Field | Value |
|---|---|
| Model (id / display name) | |
| Mode | text2image / image2image |
| Resolution / size | |
| Aspect ratio | |
| Quality | |
| Images per call | |
| Seed | (fixed value or "random, logged after") |
| Reference image(s) | none / owner-uploaded id / earlier creation id |

## 4. Prompts (verbatim; one block per call)
### Call 1 - <asset id>
```
<the exact prompt>
```
Forbidden-content check: no other game/brand names, no text inside art, style lock present. Result: PASS/FAIL

## 5. Cost (from `openart_model_cost` for these exact settings)
| Call | Asset | Images | Credits per call | Subtotal |
|---|---|---|---|---|
| 1 | | | | |
- **Total credits for this batch:** <n>
- **Balance now:** <n> | **Balance after:** <n> | Plus monthly allowance used this month so far: <n>
- Planning estimate in the manifest was <n>; difference explained: <text>

## 6. Alternatives that cost less
- <cheaper model / fewer candidates / lower resolution>, saving <n> credits, trade-off <text>

## 7. After generation
- Where results can be viewed: OpenArt creations (`openart_creation_show`); files are saved to <paths> after the owner picks winners.
- Owner decisions needed afterwards: <list, e.g. pick one image; upload as reference>
- Risks / things that may go wrong: <list>

## 8. Approval needed
Reply "approve <BATCH ID>" to run exactly this packet, or tell me what to change. Any change means a new packet.

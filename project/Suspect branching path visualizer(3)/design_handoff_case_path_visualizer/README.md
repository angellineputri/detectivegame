# Handoff: City of Lies — UI Screens

## Overview
Three UI screens for a pixel-art detective/mystery game ("City of Lies"): a Main Menu, a Case Briefing screen, and a Case Path Visualizer (an investigation-progress "case board" showing branching suspect paths). All three share one cool-toned, chunky pixel-art visual system.

## About the Design Files
The `.html` files in this bundle are **design references**, built in HTML to pin down exact layout, color, type, and state behavior. They are not code to paste into Unity. The task is to **recreate these screens in Unity** (UI Toolkit or uGUI — whichever this project already uses) using the specs below. If some measurement isn't listed, open the HTML file directly in a browser and inspect it — every value is a literal inline style, nothing is computed or hidden in a stylesheet.

## Fidelity
High-fidelity. Colors, type, spacing, and state logic below are final — recreate pixel-for-pixel where a value is given. The one placeholder is artwork: evidence photos and any background art are drag-and-drop placeholder slots (dashed-border boxes) — replace with real sprite/photo assets in Unity.

## Shared Design System (applies to all 3 screens)

**Palette (cool, grayish — no warm browns):**
- Page background: `#1c2024` (near-black cool slate)
- Panel / sidebar background: `#22262b`
- Panel border / divider: `#33383e`
- Card background: `#e4e7ea` (cool light gray, reads as "paper")
- Card ink / text-on-card: `#242a30`
- Primary "unresolved / pin" accent: `#5b8fae` (cool blue)
- Primary "resolved / tick / confirmed" accent: `#4f8a86` (teal) — also used for the CONFIRM/START button fill
- Muted / neutral accent (dashed lines, disabled, dead-ends): `#5c6570`
- Button fill (secondary/neutral): `#ccd2d8`, ink text `#242a30`
- Button/card border (all chunky borders): `#0e1114` (near-black, NOT the same as page bg — keep it distinct so borders stay visible)
- Purple tab / character accent: `#b98fd1` (only used for the active case tab)

**Typography:**
- Headings, labels, buttons: **Press Start 2P** (Google Font) — always uppercase, small sizes (11–20px), pixel look.
- Body/paragraph copy: **VT323** (Google Font) — larger sizes (17–24px) since it's a thin pixel font, generous line-height (1.5–1.7).
- No other fonts anywhere.

**Component language:**
- Every card/button/panel: **flat fill + thick hard-edged border** (typically 4px, panels 6px), color `#0e1114`. No drop shadows, no gradients, no rounded corners (hard 0px corner radius throughout — genuinely pixel/blocky).
- Buttons have a `:hover` state (fill lightens slightly) and were previously spec'd with an offset "pixel shadow" on `:active` — current version has **no shadows anywhere** (removed per latest revision); active/hover should just be a fill-color change.
- Reveal animation: new elements fade + slide up ~6–8px over 0.4–0.6s ease. Used whenever a node/line/card newly appears due to a state change — never on first paint of the whole screen.

---

## Screen 1: Main Menu
File: `Main Menu.dc.html`

**Purpose:** Game entry point — Start, Endings, Settings, Credits.

**Layout:** Full-viewport flex container, centered content, flat `#1c2024` background (no photo/gradient — intentionally flat to match the other two screens).
- Title "CITY OF LIES": Press Start 2P, responsive size `clamp(28px, 4vw, 44px)`, color `#e4e7ea`, letter-spacing 0.06em, centered.
- Button stack below title, vertical, 18px gap, max-width 340px (90vw on small screens):
  1. START — Press Start 2P 15px, fill `#4f8a86` (teal / primary), ink `#e4e7ea` (light text on teal), border 4px `#0e1114`.
  2. ENDINGS, SETTINGS, CREDITS — same size/border, fill `#ccd2d8` (neutral gray), ink `#242a30`.
- All buttons: padding 18px 20px, no shadow, hover = fill lightens one step (`#63a39d` for teal, `#c3cad1` for gray — pick any 1-step-lighter tint).

**Interactions:** Standard button click → navigate to respective screen. No other states designed (no loading/error states specified).

---

## Screen 2: Case Briefing
File: `Case Briefing.dc.html`

**Purpose:** Shown when starting/loading a case — establishes the victim, investigator, and objective before gameplay begins.

**Layout:** Centered card on the `#1c2024` background, flex row containing [main panel] + [case-tab rail], both vertically centered as siblings in one flex row (this avoids the rail ever being pushed off-screen — do this in Unity as two elements anchored in the same horizontal layout group, not the rail free-floating over the panel).

**Main panel:** background `#e4e7ea`, border 6px `#0e1114`, internal padding ~36px 52px 44px, max-width 1240px, scrolls internally if content taller than viewport. Two-column grid inside: left column (evidence + info + actions) / 6px divider `#0e1114` / right column (objectives), roughly 1fr : 1.05fr.

- **Left column, top → bottom:**
  - "EVIDENCE" label (Press Start 2P 15px, color `#5c6570`... actually use the shared header accent — see note below).
  - Evidence photo: a slightly-rotated cream card behind (decorative, `#c3cad1`, rotate -3deg) + a framed photo slot on top: outer border 4px `#0e1114`, inner mat `#9aa7b0` with 6px padding, photo slot itself bordered 2px `#0e1114`. Overall footprint ~200×160px.
  - "INFO" label, same style as Evidence label.
  - Info paragraph: VT323 22px, line-height 1.5, current copy: "Julian Reyes is dead, and everyone close to him has something to hide. / Investigator: You. / Guided by Chief Inspector Marcus Doyle, uncover the truth behind the murder. One suspect, one lie at a time."
  - Action row (bottom, right-aligned): BACK button (neutral fill `#ccd2d8`) + CONFIRM button (teal fill `#4f8a86`, light text). Both Press Start 2P 13px, padding 16px 26–32px, border 4px `#0e1114`, no shadow.
- **Right column:** "OBJECTIVES" label + a larger paragraph (VT323 24px) — currently mirrors the Info text (intentional in this build; swap in real objective copy per-case).
- **Header label color note:** all three section headers (EVIDENCE / INFO / OBJECTIVES) currently render in `#5c6570` (cool slate) — keep this consistent across the screen; don't reintroduce a warm accent.

**Case tab rail:** to the right of the panel, same flex row, 56px wide, vertical stack of tabs, 10px gap, vertically centered:
  - Active case tab: fill `#b98fd1` (purple), vertical text (writing-mode: vertical-rl) reading "CITY OF LIES", Press Start 2P 11px, ink `#0e1114`... actually text renders in dark ink for contrast on purple.
  - Locked/other case tabs: fill `#ccd2d8`, centered "?" glyph, Press Start 2P 18px, muted color `#7a8590`. Two of these currently (placeholders for future cases).
  - All tabs: 140px tall, border 4px `#0e1114`, no shadow.

**Interactions:** BACK returns to previous screen; CONFIRM proceeds into the case. Tab rail: clicking a locked "?" tab should do nothing yet (future case, not unlocked) — clicking the active tab is a no-op (already here).

---

## Screen 3: Case Path Visualizer ("The Case Board")
File: `Case Path Visualizer.dc.html`

**Purpose:** A read-only progress map showing the player's investigation as two branching suspect paths that eventually converge on the real culprit. **This is the most complex screen — read the state table carefully, it drives everything.**

### ⚠️ Critical note for the Unity build
The left sidebar with buttons ("Complete Playthrough 1", "Run Court Scene", etc.) in this file is a **developer test harness only** — it exists so the states below could be demoed without a real game behind them. **Do not build that sidebar.** In the real game this is a **read-only overlay** with no buttons: it re-renders itself from real game/save state every time it's opened, and the merge step (see below) triggers automatically off game state, not a click.

### Data model
Three integers drive the whole screen:
- `stageA` (0–5): Ex-GF branch progress.
- `stageB` (0–5): Assistant branch progress.
- `stageFinal` (0–2): final-culprit resolution.

### Stage → game event mapping (apply to BOTH branches, A and B)
| Stage | Real game event | Visual result |
|---|---|---|
| 0 | Branch not chosen yet | Slot 1 position shows a "???" placeholder card |
| 1 | Player picks this suspect's item | Slot 1: pin icon, two-line label "suspect:" / `<suspect name>` |
| 2 | Playthrough 1 complete (pre–court scene) | Slot 1 flips to tick icon, label becomes "arrested:" / `<arrest-1 name>` |
| 3 | Court scene 1 finished, Playthrough 2 begins | Arrow from Slot 1 → Slot 2 draws for the first time; Slot 2 appears: pin icon, "suspect:" / `<same suspect name>` |
| 4 | Playthrough 2 complete (pre–court scene 2) | Slot 2 flips to tick, "arrested:" / `<arrest-2 name>` |
| 5 | Court scene 2 finished | Slot 3 "???" placeholder appears at the end of this branch. If the OTHER branch is below stage 5, also show a dead-end dialogue bubble under this "???" |

**Current branch labels (swap for real names/story beats as needed):**
- Branch A (Ex-GF): suspect `ex-gf` → arrest 1 `head chef` → arrest 2 `ex-gf`.
- Branch B (Assistant): suspect `assistant` → arrest 1 `assistant` → arrest 2 `ebp`.

**Dead-end dialogue copy:** "Hmm, I've reached a dead end. I should go back to the victim's apartment and look from another perspective."

### Auto-merge (must be automatic, not a button)
The instant BOTH `stageA >= 5` and `stageB >= 5`, `stageFinal` auto-advances 0 → 1: both branches' "???" slot-3 cards disappear, and two connector lines animate converging into one new node, labeled "The real culprit:" / "???" (pin icon, unresolved). Implement this as a listener on the underlying game-state, independent of whether the player currently has this screen open.

### Final resolution
A separate, explicit event (finishing "Playthrough 3" / resolving the case) advances `stageFinal` 1 → 2: the merged node flips to tick icon and its label becomes "chief".

### Layout geometry (reference values from the HTML build — reproduce proportionally)
- Two rows: Row A (Ex-GF) center-y ≈ 100px, Row B (Assistant) center-y ≈ 280px, in a 380px-tall diagram area.
- START node: far left, vertical center ≈ 200px (i.e., between the two rows) — both branches' first connector lines originate here as diagonals.
- Slot 1 x ≈ 230px. Slot 2 x ≈ 470px. Slot 3 ("???"/dead-end) x ≈ 710px. Merged final node x ≈ 850px, vertical center ≈ 190px (between rows).
- The diagram's overall width **animates** as more gets revealed: 380px (only Slot 1 visible or less) → 620px (Slot 2 revealed) → 860px (Slot 3 / dead-end revealed) → 1000px (merged). It stays horizontally centered while narrow and only grows the layout box as needed — don't hard-code a wide canvas that sits off-center in early states.
- Connector lines: dashed + `#5c6570` while the destination isn't resolved yet; solid + `#4f8a86` (teal) with an arrowhead once traveled/resolved. A connector between two slots is only drawn once the **source** slot is actually resolved (e.g., the Slot 1 → Slot 2 arrow doesn't appear until Playthrough 2 actually starts at stage 3 — not merely once Slot 1 is pinned at stage 1).

### Node card visual spec
- Size ~130×60px (100×60px for "???" placeholder cards).
- Fill `#e4e7ea`, ink `#242a30`, 4px left accent border only (not all 4 sides) — accent color `#5b8fae` (blue) while unresolved/pin, `#4f8a86` (teal) once resolved/tick.
- Icon (14×14, left of text): open circle stroke `#5b8fae` = pin/unresolved; checkmark stroke `#4f8a86` = tick/resolved.
- Two-line text: small uppercase Press Start 2P/monospace caption ("suspect:" / "arrested:"), then the value in bold.
- Dead-end dialogue bubble: small italic card below the "???" node, background `#262b31`, border `#3a4046`, text `#c7ccd1`.

### Sidebar (dev-only — do not port, described only so the state machine is legible)
Buttons: per-branch "advance" button (label changes contextually per stage) + reset; global "advance final" button (disabled until both branches hit stage 5); reset-all. None of this is production UI.

---

## Design Tokens (all 3 screens)
- `#1c2024` page bg · `#22262b` panel bg · `#33383e` panel border/divider
- `#e4e7ea` card bg · `#242a30` card ink
- `#5b8fae` pin/unresolved accent · `#4f8a86` tick/resolved + primary-action accent
- `#5c6570` muted/neutral accent · `#ccd2d8` secondary button fill
- `#0e1114` all chunky borders (buttons, cards, panels) · `#b98fd1` active-tab purple
- Fonts: Press Start 2P (headings/labels/buttons), VT323 (body copy), both via Google Fonts

## Assets
No bitmap art shipped — all shapes are flat CSS. Evidence photo and menu background are drag-and-drop placeholder slots (`<image-slot>`, a custom web component used only for this HTML prototype) — replace with real sprite/photo assets in Unity; there is no other artwork to extract.

## Files
- `Main Menu.dc.html`
- `Case Briefing.dc.html`
- `Case Path Visualizer.dc.html`
- `image-slot.js` — support script for the placeholder image drop-zones in the HTML files only; not needed in Unity.

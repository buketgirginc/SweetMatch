# Sweet Match (Unity Prototype)

A 2D match-2 puzzle prototype built from scratch in **Unity**, with a focus on clean object-oriented architecture, decoupled systems, and disciplined incremental development.

This project was created as a **portfolio prototype** to demonstrate how I approach gameplay system design — not in terms of content volume, but in terms of architectural reasoning, separation of concerns, and the kind of decisions I make when a small game has to scale cleanly.

---

## Overview

Sweet Match implements a match-2 puzzle core loop: players tap connected groups of two or more same-type sweets to clear them, with cascades, gravity, and refill driving continuous play. The prototype was scoped intentionally to three hand-tuned levels and a small set of mechanics, kept small on purpose so that the underlying systems could be designed and tested thoroughly without the scope expanding into content production.

The goal was to build a foundation that could grow without rewrites: adding a new level, a new item type, or a new visual effect should require minimal changes to existing code. Where possible, I deferred features and complexity until they were justified by a real need (YAGNI), and I treated working code as something to be protected — not refactored for refactor's sake.

---

## Gameplay Features

- Match-2 core loop with tap-to-clear connected groups of same-type sweets
- Five collectible sweet types and three special item types:
  - **Cupcake** — bursts when an adjacent match occurs (neighbor trigger)
  - **Croissant** — cannot be matched; must be dropped off the bottom row to be removed
  - **CandyBar** — a power-up created by matching 5 or more sweets at once; clears its row or column
- Cascading clears, gravity, and refill
- Goal-driven progression: each level defines its own goal set, move budget, and starting layout
- Configurable grid dimensions via a separate data asset, independent of level content
- Three levels of increasing difficulty, each centered on a different mechanic
- Move-limited play with win / lose end states
- Context-aware end panel (Restart, Next Level, or Restart Game depending on outcome and progression)
- Start screen with a brief rules overview shown on first launch
- Runtime deadlock detection with automatic board reshuffle
- Animation, sound, and particle feedback layered onto the core mechanics

---

## Design Principles

The project is structured around a few principles that shaped almost every implementation decision:

- **Layered architecture.** Five distinct layers: Model (pure C# game state), Events (decoupled communication contracts), Systems (game rules and logic), Presentation (Unity-side rendering, animation, audio, VFX), and Bootstrap (composition root). Layers depend downward only; rendering knows about model, model knows nothing about rendering.

- **Event-driven systems.** A single in-memory `EventBus` connects systems. Systems do not hold references to one another — `GoalSystem` does not know `ClearSystem` exists; it only listens for `ItemsClearedEvent`. This decoupling made it possible to add sound effects, particle feedback, and animation in later phases as new event subscribers, without modifying any existing gameplay code.

- **Finite state machine for game flow.** A small, explicit FSM (`Loading → Idle → Resolving → Won / Lost`) governs what the game allows at any given moment. Input is filtered through state, terminal states are protected by transition rules ("a won game cannot be lost"), and asynchronous flows are coordinated through coroutine resolution rather than scattered booleans.

- **ScriptableObject-driven configuration.** Levels, item visuals, and grid configuration are data assets. Adding a fourth or fifth level requires only authoring a new asset — no code changes, no system modifications.

- **Single Responsibility, applied carefully.** Each system owns one concern: `MatchDetector` finds matches, `ClearSystem` removes items, `MovesTracker` counts moves and nothing else. When two responsibilities accidentally merged during development, the fix was to extract the decision back to the orchestrating system rather than patch around it.

---

## Development Process

The project was built in nine planned phases, each broken into atomic commits with conventional commit messages. Phases moved roughly from inside-out: data model first, then the event bus, then game systems, then state management, then presentation, then animation and polish, then multi-level support, then the start screen, and finally build and deployment.

Each commit was kept small enough to be independently testable and revertible. Refactors were always isolated from feature work — a single commit either changed behavior or restructured code, never both. When a design decision did not feel right in practice, it was tested, evaluated, and discarded rather than committed reluctantly.

---

## Tech Stack

- **Unity 6** (2D)
- **C#** with an event-bus and ScriptableObject-based composition
- **DOTween** for animation tweening
- **TextMeshPro** for UI typography
- **WebGL** build target for portfolio playtesting

---

## Levels

- **Level 1.** Introduces the core match-2 loop with three sweet-collection goals and a generous move budget. The initial board is lightly seeded to make a five-or-more match (and thus a CandyBar power-up) likely within the first few moves.

- **Level 2.** Introduces the cupcake mechanic (bursts when an adjacent match occurs) alongside three sweet goals, with a tighter move budget.

- **Level 3.** Introduces the croissant mechanic (drops off the board) on top of cupcakes and sweet goals. Planning the column underneath each croissant becomes the main strategic challenge, since croissants cannot be matched and must be cleared by gravity alone.

---

## Assets & Credits

All visual and audio assets used in this prototype are free-to-use, publicly available assets. Gameplay logic, systems, and architecture were implemented from scratch.

- **Sweet sprites and background** — [CraftPix Free Candy Match-3 Game Items](https://craftpix.net/freebies/free-candy-match-3-game-items/)
- **UI panels and buttons** — [Kenney Puzzle Pack 1](https://www.kenney.nl/assets/puzzle-pack-1) (CC0)
- **Particle textures** — [Kenney Particle Pack](https://kenney.nl/assets/particle-pack) (CC0)
- **Sound effects** — sourced from various contributors on [Pixabay](https://pixabay.com/) (CC0)
- **Font** — [Fredoka SemiBold](https://fonts.google.com/specimen/Fredoka) (Open Font License)
- **Animation library** — [DOTween](http://dotween.demigiant.com/) (free version)

---

## Playable Build

A playable WebGL build and gameplay video are available via an external portfolio link.

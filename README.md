# Classic 3-Reel Slot Machine

## Game Overview

A modular 3-reel, single-payline slot machine game developed in Unity, targeting WebGL. The project uses a deterministic slot loop: weighted random outcomes are calculated before animation, reels scroll continuously within a masked viewport, matching symbols smoothly snap onto the center payline, and win/loss feedback is displayed while bankroll credits are tracked.

### Key Specifications

- **Target Platform:** WebGL (HTML5 / WebAssembly)
- **Engine:** Unity 6.3 LTS (6000.3.19f1)
- **Canvas Resolution:** 1920 × 1080 (responsive UI scaling)
- **Core Mechanics:** 3 reels, one center payline, weighted random distribution, dynamic win/loss dialogue, and credit tracking

---

## Instructions to Run the WebGL Build

The compiled WebGL release build is expected to be located in `Build/WebGL/`.

Modern browsers restrict loading WebAssembly (`.wasm`) and data files directly through `file://` URLs. Serve the build through a local HTTP server instead.

### Option 1: Python HTTP Server (Recommended)

1. Navigate to build directory and open a terminal or Command Prompt in the WebGL folder.
2. Start a local server:

   ```bash
   # Python 3
   python -m http.server 8000
   ```

   Depending on your environment, you can also use `py -m http.server 8000` or `python3 -m http.server 8000`.

4. Open a browser and visit:

   `http://localhost:8000`

### Option 2: Node.js (`npx serve`)

1. Open a terminal in `Build/WebGL/`.
2. Run:

   ```bash
   npx serve .
   ```

3. Open the local address printed in the terminal, typically `http://localhost:3000`.

---

## Bonus Features

- **Staggered Reel Stops:** Reels stop one after another from left to right to emulate the cadence of a physical slot machine.
- **Elastic Recoil Animation:** Reels ease into place and perform a small spring-like bounce before settling.
- **Non-Winning Feedback:** Non-winning spins display an automatically dismissing “Better Luck Next Time!” message.
- **Data-Driven ScriptableObject Architecture:** Symbol definitions, payout multipliers, and drop weights are stored in modular data assets rather than hard-coded into gameplay logic.
- **Responsive Aspect Scaling:** The UI uses Unity's Canvas Scaler with height-priority constraints to help prevent cropping across common laptop and desktop aspect ratios.

---

## Thought Process and Architectural Approach

### 1. Deterministic Outcomes vs. Physical Simulation

Reel outcomes are selected before the animation begins rather than relying on physics collisions or frame-dependent timing.

- `SlotGameManager` requests the outcome from `RNGManager` when a spin starts.
- `RNGManager` selects symbols using a cumulative weighted-random algorithm based on each symbol's configured drop weight.
- The selected outcome is assigned to each reel before the animation sequence begins.

This separates game logic from presentation and ensures that animation timing does not determine the result.

### 2. Separation of Concerns (Object-Oriented Architecture)

- **`SymbolData` (ScriptableObject):** Holds symbol identifiers, sprite references, payout multipliers, and drop weights.
- **`RNGManager` (static service):** Contains the weighted-random selection logic and can be tested or replaced independently of the UI and gameplay scripts.
- **`ReelController` (component):** Handles reel animation, recycling off-screen symbols, deceleration, and vertical grid snapping.
- **`SlotGameManager` (master controller):** Coordinates credits, bets, button lockouts during spins, reel dispatch, payline evaluation, and win/loss messages.

### 3. Reel Looping and Smooth Snapping

Instead of repeatedly creating and destroying symbol objects during a spin—which can cause garbage-collection spikes—each reel reuses a fixed pool of four UI `Image` elements.

- When a symbol passes the bottom boundary, it is repositioned at the top while preserving the loop offset.
- During deceleration, the four slots are smoothly aligned to fixed row offsets: `-symbolHeight`, `0`, `+symbolHeight`, and `+2*symbolHeight`.
- The preselected outcome is placed at index `1`, aligning it with the center payline.

This approach reduces runtime object allocation and makes the final reel positions predictable.

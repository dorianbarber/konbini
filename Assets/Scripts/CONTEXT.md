# Konbini — Dev Context

## The Game

**Konbini** is a hectic convenience store management game set during Japan's **Golden Week** holiday. Each day of Golden Week is a level, with difficulty escalating across the run. The player manages a konbini solo — restocking shelves, serving customers at the counter, and keeping pace as foot traffic surges each day.

Core fantasy: the organized chaos of a real konbini during the busiest week of the year.

---

## Current Codebase

All scripts live in `Assets/Scripts/`. The current implementation covers the **customer AI and interaction loop**.

### Scripts

| File | Purpose |
|---|---|
| `Customer.cs` | Main AI agent. Autonomous customer that works through a list of Intents. |
| `Intent.cs` | Data container. Pairs a target interactable with a navigation waypoint. |
| `ICustomerInteractable.cs` | Interface for objects customers can interact with. |
| `IPlayerInteractable.cs` | Interface for objects the player can interact with. |
| `Stock.cs` | A shelf/item stack. Customers grab items; player restocks. |
| `Service.cs` | A service point (checkout, food station). Customers wait; player holds interact. |

---

## Architecture

```
Customer (NavMeshAgent AI)
  └─ List<Intent>
      └─ Intent { Target: ICustomerInteractable, NavigationTarget: Transform }

ICustomerInteractable          IPlayerInteractable
  └─ Stock (implements both)       └─ Stock
  └─ Service (implements both)     └─ Service
```

### Customer State Machine

```
Idle → MovingToTarget → TakingStock (auto-timed, ~0.8s)
                      → WaitingForService (patience ticks down, needs player)
                      → [next intent or...]
                      → Leaving
```

- `Customer.OnFinished` event fires when all intents complete or patience runs out.
- `Customer.PatienceRatio` (0–1) feeds into the rating system.

### Interaction Model

- **Stock**: self-completing. Customer grabs item automatically after grab animation. Player restocks to max.
- **Service**: player-driven. Customer waits passively. Player holds interact to fill a progress timer (`serviceTime`, default 3s).
- **Reservation system on Stock**: HashSet prevents two customers claiming the same last item simultaneously.

---

## Systems Referenced but Not Yet Built

| System | Role |
|---|---|
| `CustomerSpawner` | Creates customers, assigns their Intent lists |
| `RatingManager` | Listens to `Customer.OnFinished`, scores performance |
| `PlayerController` | Polls `IPlayerInteractable` each frame for input |
| Animator integration | Customer state → animation hooks (TODO stubs in Customer.cs) |

---

## Design Principles

- **Interface-first**: `ICustomerInteractable` and `IPlayerInteractable` decouple store objects from specific implementations.
- **Event-driven**: `Customer.OnFinished` lets external systems (rating, spawning) react without tight coupling.
- **Dual-role objects**: Stock and Service respond to both customer AI and player input.
- **Patience as pressure**: time-limited customers create escalating tension across levels.

---

## Game Structure (Planned)

- **One run = Golden Week** (~4–7 days)
- **Each day = one level** with a distinct difficulty profile
- Difficulty levers: customer count, patience duration, intent list length, stock depletion rate
- Endgame rating based on aggregate `PatienceRatio` across all served customers

---

## Open Questions / Next Steps

- Build `CustomerSpawner` (wave config per day)
- Build `RatingManager` (aggregate score, end-of-day summary)
- Build `PlayerController` (movement + interaction polling)
- Design level data format (how each day's difficulty is defined)
- Animator integration for customer states
- Store layout / scene setup

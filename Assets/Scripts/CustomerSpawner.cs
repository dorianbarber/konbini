using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// =============================================================================
// SCENE SETUP GUIDE — read this before placing CustomerSpawner in a scene
// =============================================================================
//
// 1. NAVMESH
//    The scene must have a baked NavMesh. Customers use Unity's NavMeshAgent
//    to walk — without a NavMesh they will spawn and stand still.
//    Window > AI > Navigation > Bake.
//
// 2. CUSTOMER PREFAB
//    The Customer prefab must have a NavMeshAgent component on it.
//    Use the default NavMeshAgent settings to start; tune Speed and
//    Stopping Distance later once you have a real layout.
//
// 3. SPAWN POINT
//    Create an empty GameObject where customers should enter the scene.
//    Assign it to the Spawn Point field. Place it on the NavMesh.
//
// 4. INTENT POOL
//    Every Stock shelf and Service counter in the scene must be registered
//    here for customers to visit them. For each one, add an entry:
//      - Interactable: drag the Stock or Service component from the scene
//      - Navigation Target: drag a separate empty GameObject placed in front
//        of that object (where the customer should stand to interact)
//    Only objects in this list will be visited. Anything not listed is ignored.
//
// 5. DAY DATA
//    Assign a DayData asset (Create > Konbini > Day Data).
//    DayData controls how many customers spawn, how often, and how patient
//    they are. Start with the defaults — tweak once the scene is running.
//
// WHAT TO EXPECT IN A TEST SCENE
//    Without a PlayerController, customers assigned to a Service point will
//    wait until their patience runs out and then leave. This is expected.
//    Customers assigned only Stock shelves will complete their loop fully:
//    spawn → walk to shelf → grab → (repeat for each intent) → leave.
//
// =============================================================================

/// <summary>
/// Reads a DayData asset and spawns customers for the level.
/// Builds each customer's intent list by randomly sampling the intent pool.
/// Fires OnAllCustomersFinished when every customer for the day has left.
/// </summary>
public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private DayData dayData;
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Every Stock and Service in the scene that customers can be sent to visit.")]
    [SerializeField] private List<InteractableSlot> intentPool = new List<InteractableSlot>();

    /// <summary>Fired when every customer for the day has left or abandoned.</summary>
    public event Action OnAllCustomersFinished;

    private int finishedCount;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (dayData == null)
        {
            Debug.LogError("CustomerSpawner: no DayData assigned.", this);
            return;
        }
        StartCoroutine(SpawnRoutine());
    }

    // ── Spawning ──────────────────────────────────────────────────────────────

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < dayData.totalCustomers; i++)
        {
            if (i > 0)
                yield return new WaitForSeconds(dayData.spawnInterval);
            SpawnCustomer();
        }
    }

    private void SpawnCustomer()
    {
        var customer = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        customer.SetPatience(dayData.customerPatience);
        customer.SetIntents(BuildIntents());
        customer.OnFinished += OnCustomerFinished;
        customer.Begin();
    }

    // ── Intent Building ───────────────────────────────────────────────────────

    // Picks a random subset of the pool (no repeats) and returns it as an
    // Intent list. Count is chosen randomly between DayData's min and max.
    //
    // NOTE: flat random draw only — no weighting or archetype logic yet.
    // When DayData grows to support those, this is the method to extend.
    private List<Intent> BuildIntents()
    {
        int count = UnityEngine.Random.Range(dayData.minIntents, dayData.maxIntents + 1);
        count = Mathf.Min(count, intentPool.Count);

        var indices = new List<int>(intentPool.Count);
        for (int i = 0; i < intentPool.Count; i++) indices.Add(i);
        Shuffle(indices);

        var intents = new List<Intent>(count);
        for (int i = 0; i < count; i++)
        {
            var slot = intentPool[indices[i]];
            var interactable = slot.Interactable as ICustomerInteractable;

            if (interactable == null)
            {
                Debug.LogWarning(
                    $"CustomerSpawner: '{slot.Interactable?.name}' does not implement " +
                    "ICustomerInteractable and was skipped. Only assign Stock or Service objects " +
                    "to the Intent Pool.", this);
                continue;
            }

            intents.Add(new Intent
            {
                Target = interactable,
                NavigationTarget = slot.NavigationTarget
            });
        }
        return intents;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ── Tracking ──────────────────────────────────────────────────────────────

    private void OnCustomerFinished(Customer customer)
    {
        customer.OnFinished -= OnCustomerFinished;
        finishedCount++;

        if (finishedCount >= dayData.totalCustomers)
            OnAllCustomersFinished?.Invoke();
    }
}

// =============================================================================
// InteractableSlot — appears in the CustomerSpawner Intent Pool list
// =============================================================================
//
// Each entry pairs a scene object with a navigation waypoint:
//   Interactable     — the Stock or Service component on the object
//   Navigation Target — an empty GameObject placed where the customer stands
//
// Both fields must be filled. An entry with a missing or wrong Interactable
// will be skipped at runtime and a warning will appear in the Console.
//
// =============================================================================
[Serializable]
public class InteractableSlot
{
    [Tooltip("The Stock or Service component on this scene object.")]
    public MonoBehaviour Interactable;

    [Tooltip("Empty GameObject placed where the customer should stand to interact.")]
    public Transform NavigationTarget;
}

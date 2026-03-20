using UnityEngine;

/// <summary>
/// Defines the difficulty and pacing profile for a single day (level).
/// Read by CustomerSpawner at level start. Expand as new systems come online.
/// </summary>
[CreateAssetMenu(fileName = "Day_01", menuName = "Konbini/Day Data")]
public class DayData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Which day of Golden Week this is. Informational for now.")]
    public int dayNumber = 1;

    // NOTE: The flat values below (spawnInterval, patience, intent counts) are a
    // starting point — sufficient for prototyping but not for the shipped game.
    // Real difficulty tuning will likely require per-wave spawn timing (bursts,
    // lulls), weighted intent pools (some customers always checkout, some browse
    // first), and customer archetypes that tie specific intent sequences to a
    // type. This data format will need to grow to support that. For now we are
    // keeping it minimal and expanding it alongside the systems that consume it.

    [Header("Spawning")]
    [Tooltip("Total customers that arrive this day.")]
    public int totalCustomers = 10;

    [Tooltip("Seconds between each customer spawn.")]
    public float spawnInterval = 6f;

    [Header("Customer Behaviour")]
    [Tooltip("How many seconds a customer will wait before abandoning. Applied per customer at spawn.")]
    public float customerPatience = 60f;

    [Tooltip("Min number of stops (intents) a customer will make.")]
    [Min(1)] public int minIntents = 1;

    [Tooltip("Max number of stops (intents) a customer will make.")]
    [Min(1)] public int maxIntents = 3;

    // ── Stubs — wire up as systems come online ────────────────────────────────

    // [Header("Service")]
    // [Tooltip("Overrides Service.serviceTime for this day. Lower = harder.")]
    // public float serviceTimeOverride = 3f;

    // [Header("Flavour")]
    // public string dayTitle;
    // public AudioClip bgm;

    // ─────────────────────────────────────────────────────────────────────────

    private void OnValidate()
    {
        if (maxIntents < minIntents)
            maxIntents = minIntents;
    }
}

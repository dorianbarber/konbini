using UnityEngine;

/// <summary>
/// A single goal the customer wants to fulfill. Holds the interactable target
/// and the transform the customer should navigate to when pursuing this intent.
/// Built at runtime by CustomerSpawner from level config.
/// </summary>
public class Intent
{
    public ICustomerInteractable Target;

    /// <summary>World position the customer walks to before interacting.</summary>
    public Transform NavigationTarget;
}

using UnityEngine;

/// <summary>
/// A service point (e.g. hot food counter, checkout) where the customer waits
/// and the player must come to complete the interaction over time.
/// Interaction hinges on a timed completion by the player.
/// </summary>
public class Service : MonoBehaviour, ICustomerInteractable, IPlayerInteractable
{
    [SerializeField] private float serviceTime = 3f;

    private Customer waitingCustomer;
    private float serviceProgress;
    private bool playerServicing;

    public bool IsOccupied => waitingCustomer != null;

    // ── ICustomerInteractable ────────────────────────────────────────────────

    public bool CustomerCanInteract(Customer customer)
    {
        // Only one customer can occupy a service point at a time.
        return !IsOccupied;
    }

    public void CustomerInteract(Customer customer)
    {
        // Customer arrives and waits — hold reference, reset progress.
        waitingCustomer = customer;
        serviceProgress = 0f;
        playerServicing = false;
    }

    public void CustomerCompleteInteraction(Customer customer)
    {
        waitingCustomer = null;
        serviceProgress = 0f;
        playerServicing = false;
    }

    // ── IPlayerInteractable ──────────────────────────────────────────────────

    public bool PlayerCanInteract()
    {
        return IsOccupied && !playerServicing;
    }

    public void PlayerInteract()
    {
        // Called each frame by PlayerController while the player holds the
        // interact input. Advances the service timer.
        playerServicing = true;
        serviceProgress += Time.deltaTime;

        if (serviceProgress >= serviceTime)
        {
            PlayerCompleteInteraction();
        }
    }

    public void PlayerCompleteInteraction()
    {
        if (waitingCustomer != null)
        {
            waitingCustomer.CustomerCompleteInteraction(this);
        }

        waitingCustomer = null;
        serviceProgress = 0f;
        playerServicing = false;
    }
}

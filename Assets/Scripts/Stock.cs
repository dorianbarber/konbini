using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A shelf or item stack that customers can take from and the player can restock.
/// Interaction hinges on stock count.
/// </summary>
public class Stock : MonoBehaviour, ICustomerInteractable, IPlayerInteractable
{
    [SerializeField] private int currentStock = 5;
    [SerializeField] private int maxStock = 5;

    // Tracks which customers have reserved a unit of this stock so two
    // customers don't claim the same last item simultaneously.
    private readonly HashSet<Customer> reservations = new HashSet<Customer>();

    public int CurrentStock => currentStock;

    // ── ICustomerInteractable ────────────────────────────────────────────────

    public bool CustomerCanInteract(Customer customer)
    {
        // Available units = stock not yet reserved by other customers.
        int available = currentStock - reservations.Count;
        return available > 0 && !reservations.Contains(customer);
    }

    public void CustomerInteract(Customer customer)
    {
        // Stock pickup is instantaneous; reserve the unit while the customer
        // is in the grab animation so nothing else claims it.
        reservations.Add(customer);
    }

    public void CustomerCompleteInteraction(Customer customer)
    {
        reservations.Remove(customer);
        currentStock = Mathf.Max(0, currentStock - 1);
    }

    // ── IPlayerInteractable ──────────────────────────────────────────────────

    public bool PlayerCanInteract()
    {
        return currentStock < maxStock;
    }

    public void PlayerInteract()
    {
        // Real-time restocking animation tick — logic driven by PlayerController.
    }

    public void PlayerCompleteInteraction()
    {
        currentStock = maxStock;
    }
}

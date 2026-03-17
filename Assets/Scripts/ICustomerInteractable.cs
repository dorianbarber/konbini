public interface ICustomerInteractable
{
    /// <summary>Returns true if this object is in a state where the customer can begin interacting.</summary>
    bool CustomerCanInteract(Customer customer);

    /// <summary>Called each frame while the customer is actively interacting (real-time process).</summary>
    void CustomerInteract(Customer customer);

    /// <summary>Called once when the customer's interaction has fully completed.</summary>
    void CustomerCompleteInteraction(Customer customer);
}

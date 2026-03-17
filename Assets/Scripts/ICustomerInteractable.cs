public interface ICustomerInteractable
{
    /// <summary>
    /// True for service points where the customer waits passively for the player to act.
    /// False for self-completing interactions like picking up stock.
    /// </summary>
    bool RequiresPlayerService { get; }

    /// <summary>Returns true if this object is in a state where the customer can begin interacting.</summary>
    bool CustomerCanInteract(Customer customer);

    /// <summary>Called once when the customer arrives and begins interacting.</summary>
    void CustomerInteract(Customer customer);

    /// <summary>Called once when the customer's interaction has fully completed.</summary>
    void CustomerCompleteInteraction(Customer customer);
}

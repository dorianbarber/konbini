public interface IPlayerInteractable
{
    /// <summary>Returns true if this object is in a state where the player can begin interacting.</summary>
    bool PlayerCanInteract();

    /// <summary>Called each frame while the player is actively interacting (real-time process).</summary>
    void PlayerInteract();

    /// <summary>Called once when the player's interaction has fully completed.</summary>
    void PlayerCompleteInteraction();
}

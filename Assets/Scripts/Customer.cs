using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;

public enum CustomerState
{
    Idle,
    MovingToTarget,
    TakingStock,        // grabbing stock — self-timed
    WaitingForService,  // at a service point, waiting for player
    Leaving
}

/// <summary>
/// Owns an ordered list of intents and works through them one by one.
/// Spawned and configured by CustomerSpawner; rated by RatingManager via OnFinished.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour
{
    [SerializeField] private float grabDuration = 0.8f;
    [SerializeField] private float maxPatience = 60f;
    [SerializeField] private float arrivalDistance = 0.5f;

    private List<Intent> intents = new List<Intent>();
    private int currentIntentIndex = -1;

    private CustomerState state = CustomerState.Idle;
    public CustomerState State => state;

    private float interactionTimer;
    private float patience;

    /// <summary>0–1 ratio of remaining patience. Used by RatingManager.</summary>
    public float PatienceRatio => patience / maxPatience;

    /// <summary>Fired when the customer finishes all intents or runs out of patience.</summary>
    public event Action<Customer> OnFinished;

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        patience = maxPatience;
    }

    void Update()
    {
        switch (state)
        {
            case CustomerState.MovingToTarget:
                if (HasArrivedAtTarget())
                    OnArrivedAtTarget();
                break;

            case CustomerState.TakingStock:
                interactionTimer -= Time.deltaTime;
                if (interactionTimer <= 0f)
                    CompleteCurrentInteraction();
                break;

            case CustomerState.WaitingForService:
                patience -= Time.deltaTime;
                if (patience <= 0f)
                    Abandon();
                break;
        }
    }

    // ── Setup ────────────────────────────────────────────────────────────────

    /// <summary>Called by CustomerSpawner to assign this customer's goal list.</summary>
    public void SetIntents(List<Intent> newIntents)
    {
        intents = newIntents;
    }

    /// <summary>Starts the customer working through their intents.</summary>
    public void Begin()
    {
        currentIntentIndex = 0;
        MoveToCurrentIntent();
    }

    // ── Navigation ───────────────────────────────────────────────────────────

    private void MoveToCurrentIntent()
    {
        if (currentIntentIndex >= intents.Count)
        {
            Leave();
            return;
        }

        var target = intents[currentIntentIndex].NavigationTarget;
        if (target != null)
            agent.SetDestination(target.position);

        SetState(CustomerState.MovingToTarget);
    }

    private bool HasArrivedAtTarget()
    {
        return agent.hasPath && !agent.pathPending && agent.remainingDistance <= arrivalDistance;
    }

    // ── Interaction ──────────────────────────────────────────────────────────

    private void OnArrivedAtTarget()
    {
        var interactable = CurrentTarget;
        if (interactable == null) return;

        if (!interactable.CustomerCanInteract(this))
            return; // not ready yet — remain at target and retry next frame

        interactable.CustomerInteract(this);

        if (interactable.RequiresPlayerService)
        {
            SetState(CustomerState.WaitingForService);
        }
        else
        {
            interactionTimer = grabDuration;
            SetState(CustomerState.TakingStock);
        }
    }

    /// <summary>
    /// Called by service interactables (e.g. Service) when the player has
    /// finished servicing this customer.
    /// </summary>
    public void SignalInteractionComplete(ICustomerInteractable source)
    {
        CompleteCurrentInteraction();
    }

    private void CompleteCurrentInteraction()
    {
        CurrentTarget?.CustomerCompleteInteraction(this);
        currentIntentIndex++;
        MoveToCurrentIntent();
    }

    // ── Exit ─────────────────────────────────────────────────────────────────

    private void Abandon()
    {
        // Patience expired — leave with whatever rating remains (near zero).
        Leave();
    }

    private void Leave()
    {
        SetState(CustomerState.Leaving);
        OnFinished?.Invoke(this);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetState(CustomerState newState)
    {
        state = newState;
        // Hook animator here: animator.SetInteger("State", (int)state);
    }

    private ICustomerInteractable CurrentTarget =>
        currentIntentIndex >= 0 && currentIntentIndex < intents.Count
            ? intents[currentIntentIndex].Target
            : null;
}

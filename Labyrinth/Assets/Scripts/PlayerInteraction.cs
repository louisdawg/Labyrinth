using UnityEngine;

// E = aufheben / droppen, F = Fleisch essen (oder einen fressenden Slime)
// Zeigt ein kleines [E]-Symbol über dem nächsten Item in Reichweite.
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float pickupRange = 0.6f;

    void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
        {
            InteractPrompt.Hide();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E)) UseOrPickupOrDrop();
        if (Input.GetKeyDown(KeyCode.F)) Eat();

        UpdatePrompt();
    }

    private void UpdatePrompt()
    {
        // Tür, Hebel usw. haben Vorrang
        IInteractable interactable = FindNearestInteractable();
        if (interactable != null)
        {
            string prompt = interactable.GetPrompt(GameManager.Instance);
            if (prompt != null)
            {
                InteractPrompt.Show(interactable.PromptPosition, prompt);
                return;
            }
        }

        // Mit vollem Mund gibt es nichts aufzuheben (E legt ab, das steht in der Legende)
        if (GameManager.Instance.HeldItem != Item.None)
        {
            InteractPrompt.Hide();
            return;
        }

        // Ein Slime, der gerade frisst, kann mit F gefressen werden
        Slime eatable = FindNearestEatableSlime();
        if (eatable != null)
        {
            InteractPrompt.Show(eatable.transform.position, "[F]");
            return;
        }

        ItemPickup nearest = FindNearestPickup();
        if (nearest == null)
        {
            InteractPrompt.Hide();
            return;
        }

        string text = nearest.item == Item.Fleisch ? "[E] [F]" : "[E]";
        InteractPrompt.Show(nearest.transform.position, text);
    }

    private void UseOrPickupOrDrop()
    {
        GameManager gm = GameManager.Instance;

        IInteractable interactable = FindNearestInteractable();
        if (interactable != null && interactable.CanInteract(gm))
        {
            interactable.Interact(gm);
            gm.UsedInteract = true;
            return;
        }

        PickupOrDrop();
    }

    private void PickupOrDrop()
    {
        GameManager gm = GameManager.Instance;
        WorldItemManager world = WorldItemManager.Instance;
        if (world is null)
        {
            Debug.LogWarning("Kein WorldItemManager in der Szene.");
            return;
        }

        if (gm.HeldItem != Item.None)
        {
            world.DropItem(gm.HeldItem, transform.position);
            gm.HeldItem = Item.None;
            gm.UsedInteract = true;
            return;
        }

        ItemPickup nearest = FindNearestPickup();
        if (nearest == null) return;

        gm.HeldItem = nearest.item;
        world.RemoveItem(nearest);
        gm.UsedInteract = true;
    }

    private void Eat()
    {
        GameManager gm = GameManager.Instance;

        if (gm.HeldItem == Item.Fleisch)
        {
            gm.Eat();
            gm.HeldItem = Item.None;
            gm.UsedEat = true;
            return;
        }

        if (gm.HeldItem != Item.None) return;

        // Mund leer: erst ein fressender Slime in Reichweite, dann Fleisch am Boden
        Slime slime = FindNearestEatableSlime();
        if (slime != null)
        {
            slime.GetEaten();
            gm.UsedEat = true;
            return;
        }

        WorldItemManager world = WorldItemManager.Instance;
        ItemPickup nearest = FindNearestPickup();
        if (world is not null && nearest is not null && nearest.item == Item.Fleisch)
        {
            gm.Eat();
            world.RemoveItem(nearest);
            gm.UsedEat = true;
        }
    }

    private IInteractable FindNearestInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);
        IInteractable best = null;
        float bestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            IInteractable candidate = hit.GetComponent<IInteractable>();
            if (candidate == null) continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }
        return best;
    }

    private Slime FindNearestEatableSlime()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);
        Slime best = null;
        float bestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            Slime slime = hit.GetComponent<Slime>();
            if (slime == null || !slime.CanBeEaten) continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = slime;
            }
        }
        return best;
    }

    private ItemPickup FindNearestPickup()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);
        ItemPickup best = null;
        float bestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            ItemPickup pickup = hit.GetComponent<ItemPickup>();
            if (pickup is null) continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = pickup;
            }
        }
        return best;
    }
}
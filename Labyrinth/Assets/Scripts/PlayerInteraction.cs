using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float pickupRange = 0.6f;
    [SerializeField] private GameObject meatPrefab;
    [SerializeField] private GameObject keyPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) PickupOrDrop();
        if (Input.GetKeyDown(KeyCode.F)) Eat();
    }

    private void PickupOrDrop()
    {
        GameManager gm = GameManager.Instance;
        
        if (gm.HeldItem != Item.None)
        {
            GameObject prefab = GetPrefab(gm.HeldItem);
            if (prefab is null) return;

            Instantiate(prefab, transform.position, Quaternion.identity);
            gm.HeldItem = Item.None;
            return;
        }
        
        ItemPickup nearest = FindNearestPickup();
        if (nearest is null) return;

        gm.HeldItem = nearest.item;
        Destroy(nearest.gameObject);
    }

    private void Eat()
    {
        GameManager gm = GameManager.Instance;
        
        if (gm.HeldItem == Item.Fleisch)
        {
            gm.Eat();
            gm.HeldItem = Item.None;
            return;
        }
        
        if (gm.HeldItem != Item.None) return;

        ItemPickup nearest = FindNearestPickup();
        if (nearest is not null && nearest.item == Item.Fleisch)
        {
            gm.Eat();
            Destroy(nearest.gameObject);
        }
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

    private GameObject GetPrefab(Item item)
    {
        switch (item)
        {
            case Item.Fleisch: return meatPrefab;
            case Item.Schluessel: return keyPrefab;
            default: return null;
        }
    }
}
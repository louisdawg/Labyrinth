using UnityEngine;

// Hier erscheint der Schlüssel, sobald das Rätsel gelöst ist.
// Kein Collider nötig (der Altar soll den Weg zum Schlüssel nicht versperren).
public class KeyAltar : MonoBehaviour
{
    void Start()
    {
        SymbolPuzzle.Instance.Register(this);
    }

    public void SpawnKey()
    {
        WorldItemManager world = WorldItemManager.Instance;
        if (world == null)
        {
            Debug.LogWarning("Kein WorldItemManager in der Szene.");
            return;
        }
        world.DropItem(Item.Schluessel, transform.position);
    }
}
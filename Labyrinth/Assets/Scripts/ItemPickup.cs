using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item = Item.Fleisch;
    
    [System.NonSerialized] public ItemRecord record;
}
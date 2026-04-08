using UnityEngine;
using ItemData = DungeonScavenger.Inventory.ItemData;
using PlayerInventory = DungeonScavenger.Inventory.PlayerInventory;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private AudioClip customPickupSound;

    private void OnTriggerEnter(Collider other)
    {
        // Professional: Layer-based checking is faster than tag comparison
        if (!other.CompareTag("Player")) return;
        
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            bool wasPickedUp = inventory.AddItem(itemData);
            
            if (wasPickedUp)
            {
                Debug.Log($"[Pickup] Collected {itemData.itemName}. Inventory size: {inventory.items.Count}");
                
                // Audio feedback (will work once AudioManager is set up)
                if (customPickupSound != null)
                {
                    // AudioManager.Instance.PlaySFX(customPickupSound);
                }
                
                if (destroyOnPickup)
                    Destroy(gameObject);
            }
            else
            {
                Debug.Log($"[Pickup] Inventory full! Cannot collect {itemData.itemName}");
            }
        }
    }
    
    // Professional: Visual debugging in Scene view
    private void OnDrawGizmos()
    {
        if (itemData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, 
                $"Pickup: {itemData.itemName}");
            #endif
        }
    }
}
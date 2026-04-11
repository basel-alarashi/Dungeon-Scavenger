using UnityEngine;

namespace DungeonScavenger.Inventory
{
    [RequireComponent(typeof(Collider))]
    public class PickupItem : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity = 1;
        
        [Header("Pickup Settings")]
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private AudioClip customPickupSound;
        
        [Header("Debug")]
        [SerializeField] private bool logPickup = true;
        
        private void OnTriggerEnter(Collider other)
        {
            // ✓ CHECK: Only player can pick up
            if (!other.CompareTag("Player")) 
                return;
            
            // ✓ CHECK: Get inventory component
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                Debug.LogError($"[PickupItem] Player has no PlayerInventory component!");
                return;
            }
            
            // ✓ CHECK: Attempt to add item
            bool wasPickedUp = inventory.AddItem(itemData, quantity);
            
            if (wasPickedUp)
            {
                // ✓ CHECK: Debug log for testing
                if (logPickup)
                    Debug.Log($"[Pickup] Collected {quantity}x {itemData.itemName}");
                
                // TODO: Audio feedback (Phase 6)
                
                // ✓ CHECK: Destroy on pickup
                if (destroyOnPickup)
                    Destroy(gameObject);
            }
            else
            {
                if (logPickup)
                    Debug.Log($"[Pickup] Cannot collect {itemData.itemName} - Inventory full!");
            }
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Visual debugging in Scene view
            if (itemData != null)
            {
                Gizmos.color = itemData.itemColor;
                Gizmos.DrawWireSphere(transform.position, 1f);
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2, 
                    $"Pickup: {itemData.itemName}");
            }
        }
        #endif
    }
}
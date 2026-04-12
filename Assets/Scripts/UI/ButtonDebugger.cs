using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonDebugger : MonoBehaviour
{
    [SerializeField] private Button targetButton;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Check what UI element is under the mouse
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            Debug.Log($"=== UI Raycast Results ({results.Count} hits) ===");
            foreach (var result in results)
            {
                Debug.Log($"  Hit: {result.gameObject.name} (Layer: {result.gameObject.layer})");
            }

            // Check if our button was hit
            bool buttonHit = false;
            foreach (var result in results)
            {
                if (result.gameObject == targetButton.gameObject)
                {
                    buttonHit = true;
                    break;
                }
            }

            Debug.Log($"Button was hit: {buttonHit}");
        }
    }
}
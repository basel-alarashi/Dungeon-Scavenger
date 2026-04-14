using UnityEngine;

namespace DungeonScavenger.Items
{
    public class PickupGlow : MonoBehaviour
    {
        [SerializeField] private Color glowColor = Color.yellow;
        [SerializeField] private float glowRange = 2f;
        [SerializeField] private float glowIntensity = 0.5f;

        private Light pickupLight;

        private void Start()
        {
            // Create light component
            pickupLight = gameObject.AddComponent<Light>();
            pickupLight.type = LightType.Point;
            pickupLight.color = glowColor;
            pickupLight.range = glowRange;
            pickupLight.intensity = glowIntensity;
        }
    }
}
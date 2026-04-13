using UnityEngine;
using UnityEngine.UI;
using DungeonScavenger.Core;

namespace DungeonScavenger.UI
{
    /// <summary>
    /// Automatically adds click sound to any button.
    /// Attach this to buttons that should play sound on click.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonSound : MonoBehaviour
    {
        [SerializeField] private bool playSound = true;
        [SerializeField] private AudioClip customClickSound;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (playSound)
            {
                button.onClick.AddListener(PlayClickSound);
            }
        }

        private void PlayClickSound()
        {
            if (AudioManager.Instance == null) return;

            if (customClickSound != null)
            {
                AudioManager.Instance.PlayUISound(customClickSound);
            }
            else
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlayClickSound);
            }
        }
    }
}
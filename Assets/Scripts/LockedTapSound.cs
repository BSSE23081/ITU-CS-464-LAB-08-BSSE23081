using UnityEngine;
using UnityEngine.EventSystems;

// Tapping a locked card (Zen mode) gives error feedback instead of silence.
public class LockedTapSound : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData e) { AudioManager.PlayUI(UISound.Error); }
}

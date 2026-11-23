using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Sits on a Slider. Loads the saved value, writes changes to the mixer, and plays a
// sample when you let go so you can hear the new level.
public class VolumeSlider : MonoBehaviour, IPointerUpHandler
{
    public string key = VolumeSettings.Master;
    public Slider slider;
    public TMP_Text valueText;

    void OnEnable()
    {
        slider.SetValueWithoutNotify(VolumeSettings.Get(key));
        Refresh();
        slider.onValueChanged.AddListener(OnChanged);
    }

    void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnChanged);
    }

    void OnChanged(float v)
    {
        VolumeSettings.Set(key, v);
        Refresh();
    }

    void Refresh()
    {
        if (valueText != null) valueText.text = Mathf.RoundToInt(slider.value * 100f) + "%";
    }

    public void OnPointerUp(PointerEventData e)
    {
        VolumeSettings.Save();
        if (key == VolumeSettings.Sfx) AudioManager.PlaySfx(Sfx.Coin);
        else if (key != VolumeSettings.Music) AudioManager.PlayUI(UISound.Click);
    }
}

using UnityEngine;

// Lets UnityEvents (button onClick in the Inspector) play UI sounds without needing an AudioSource.
public class UISoundTrigger : MonoBehaviour
{
    public void PlayOpen()    { AudioManager.PlayUI(UISound.Open); }
    public void PlayClose()   { AudioManager.PlayUI(UISound.Close); }
    public void PlaySuccess() { AudioManager.PlayUI(UISound.Success); }
    public void PlayError()   { AudioManager.PlayUI(UISound.Error); }
}

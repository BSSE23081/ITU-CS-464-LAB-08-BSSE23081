# NEON DASH - Lab 08

ITU CS-464 Lab 08, BSSE23081

Demo video: [(https://drive.google.com/file/d/1mPuMT54yabuBBrMLnjltMS3Jx6Pi0A9b/view?usp=drive_link)]
Unity version: [6000.3.24f1 LTS]

This is my Lab 07 menu project, now turned into a small playable game. I kept the same canvas, `ScreenManager`, tweens and neon look, and added a loading screen, UI and game sounds, an audio mixer with volume sliders that save, and a simple runner where the game reacts through C# events.

How the game works: tap, click or press Space to jump over the red blocks. Run through the gold diamonds to collect coins and the green ones to heal. You have 3 health. On Mode Select you choose between Endless (no goal, the game gets faster until you die) and Time Trial (survive 60 seconds to win). Escape pauses.

## What I changed from Lab 07

- START RUN on the Loadout screen now goes to a loading screen and then the real gameplay scene. The old placeholder Run panel is gone.
- Pause and Results moved into the gameplay scene, since that's where they're used. They look the same. Pause > Settings > Back still returns to Pause because of the history stack in `ScreenManager`.
- I removed WATCH AD: CONTINUE from Results. It was only a placeholder.
- Settings now has four volume sliders (Master, Music, SFX, UI) in place of the SOUND and MUSIC toggles. Vibration is still a toggle.
- Mode Select passes the chosen mode to the game.
- Collected coins and the best distance are saved with PlayerPrefs. The Main Menu, Shop and my row on the Leaderboard show the real numbers now.
- `ButtonFeedback`, `CycleOption`, `ToggleSwitch` and `ScreenManager` got a few lines each to play sounds. The tweens are the same as before.

Everything else from Lab 07 (screens, navigation, layout) is unchanged.

## Setup

1. Open the project in Unity [UNITY VERSION]. TextMeshPro needs to be installed.
2. Make the mixer. Create a folder `Assets/Resources`, right click inside it and choose Create > Audio Mixer, and name it `GameMixer`. In the Audio Mixer window, add three child groups under Master called `Music`, `SFX` and `UI`. For each of the four groups, click it and right click the word Volume in the Inspector, then choose "Expose to script". Then open Exposed Parameters at the top right and rename the four parameters to exactly `MasterVolume`, `MusicVolume`, `SFXVolume` and `UIVolume`.
3. Open the menu scene, run Tools > Build Neon UI, and save the scene.
4. With the menu scene still open, run Tools > Build Gameplay Scene. It creates `Assets/Scenes/Gameplay.unity` and sets Build Settings so the menu is scene 0 and Gameplay is scene 1. The code loads scenes by those index numbers.
5. Press Play from the menu scene.

The sound files are already in `Assets/Resources/Audio`. To regenerate them, run `python3 tools/generate_audio.py` (it needs numpy).

## Loading flow

START RUN, RETRY and MAIN MENU all call `LoadingScreen.Load(index)`. The loading screen builds its own canvas in code and uses `DontDestroyOnLoad`, so it stays on top while one scene is swapped for the other.

It works like this:

1. The overlay fades in and blocks all clicks.
2. `SceneManager.LoadSceneAsync` starts with `allowSceneActivation = false`, so the new scene can't start by itself.
3. The bar shows the real `AsyncOperation.progress`. Unity stops at 0.9 while activation is blocked, so I divide by 0.9 to get 0 to 100%. The bar moves smoothly toward that number, and the screen stays up for at least 0.8 seconds so it doesn't just flash.
4. Once the loading is done and the bar has reached 100%, I set `allowSceneActivation = true` and fade the overlay out.

So the player can't get into gameplay before loading has finished.

## Audio mixer

There's one mixer with this layout:

```
Master  (MasterVolume)
  Music (MusicVolume)
  SFX   (SFXVolume)
  UI    (UIVolume)
```

![Audio Mixer](images/audio-mixer.png)

`AudioManager` creates every AudioSource, so the routing is all in one place. It makes itself before the first scene loads and survives scene changes. It owns two music sources (Music group), a pool of six SFX sources (SFX group) and one UI source (UI group). No button has its own AudioSource. `ButtonFeedback` already existed on every button for the press tween, so I made it call `AudioManager.PlayUI` as well.

## Volume mapping and saving

A slider gives 0 to 1 but the mixer takes decibels, and we hear loudness logarithmically. Plugging the slider in as a linear volume makes the bottom half of the slider feel almost the same, so I convert it:

```
dB = 20 * log10(value)
```

That gives 0 dB at full and about -6 dB at half. `log10(0)` is minus infinity, so anything at or below 0.0001 is clamped to -80 dB, which is the lowest the mixer goes and works as mute. The result is also clamped between -80 and 0 so it can never be infinity or NaN (`VolumeSettings.ToDecibels`).

Every change is stored in PlayerPrefs under the same four names, and saved when you let go of the slider. On startup `AudioManager` reads them and applies them to the mixer (in `Start`, because setting a mixer value in `Awake` can be ignored). Letting go of a slider plays a short sample so you can hear the level. Defaults are 100%, except music at 70%.

If the mixer is missing or a parameter name is wrong, `SetFloat` fails silently. To catch that, `VolumeSettings` prints an `[Audio]` warning in the Console saying what's missing, and `AudioManager` sets the volume directly on the AudioSources instead, so the sliders still do something.

## Sounds

UI sounds all go through the UI group:

- hover when the mouse enters a button (mouse only, so phones don't get a double sound)
- click when any button is pressed
- back when a back button is pressed
- open when moving forward to another screen, and open/close for the Credits popup
- select when cycling a loadout option or flipping a toggle
- pause and resume
- error when tapping the locked ZEN card

Gameplay sounds go through the SFX group: jump, coin (also used for healing), hit, death, win and lose. The lose sound plays 0.7 seconds after the death sound so they don't pile up.

Short effects use `PlayOneShot` so they can overlap, with a random pitch between 0.94 and 1.06 so repeated coins don't sound identical. Pitch belongs to a source rather than a single sound, which is why there's a pool of six sources used in turn.

## Music

There are three loops: menu, gameplay and results. `AudioManager.PlayMusic` starts the new loop on the idle source at zero volume while the old one fades out, using unscaled time so it also works while paused.

- Menu music starts when the menu scene loads.
- Menu to gameplay: crossfade over 1.2 seconds when the `GameStarted` event fires.
- Gameplay to results: crossfade over 1.5 seconds on `GameWon` or `GameLost`.
- Retry goes from results back to the gameplay loop the same way.

## Pause and audio

Pausing sets `Time.timeScale = 0` and raises `GameFlow.PauseChanged`, which `AudioManager` listens to. I decided what should happen on purpose:

- Gameplay sounds freeze and carry on when you resume (`AudioListener.pause = true`).
- Music keeps playing but drops to 35% volume.
- UI sounds keep working, otherwise the pause menu would be silent.

Music and UI use `ignoreListenerPause`, the gameplay sources don't.

## C# events

The classes that raise events:

- `PlayerHealth`: `Damaged(current, max)`, `Healed(current, max)`, `Died`
- `CoinWallet`: `Collected(total)`
- `PlayerController`: `Jumped`
- `GameController`: `GameStarted`, `GameWon`, `GameLost`
- `GameFlow`: `PauseChanged(bool)` (static)

They only update their own data and announce it. `PlayerHealth` has no idea the HUD, sounds or camera exist.

The classes that react:

- `HudListener` updates the health blocks and coin count (`Damaged`, `Healed`, `Collected`)
- `GameSfxListener` plays the sounds for jump, coin, hit, heal, death, win and lose
- `GameMusicListener` does the music crossfades (`GameStarted`, `GameWon`, `GameLost`)
- `CameraShake` shakes on `Damaged` and `Died`
- `HitFlash` flashes the player and blinks during the invulnerability time on `Damaged`
- `ResultsListener` fills in and opens the Results screen on `GameWon` and `GameLost`
- `GameController` listens to `Died` and turns it into `GameLost`
- `AudioManager` listens to `PauseChanged` for the ducking and pause sounds

All of them subscribe with `+=` in `OnEnable` and unsubscribe with `-=` in `OnDisable`, and events are raised with `?.Invoke`.

Declaring and raising an event (`PlayerHealth.cs`):

```csharp
public event Action<int, int> Damaged;   // (current, max)
public event Action Died;

public bool TakeDamage(int amount)
{
    if (IsDead || Time.time < invulnerableUntil) return false;
    Current = Mathf.Max(0, Current - amount);
    invulnerableUntil = Time.time + invulnerableTime;
    Damaged?.Invoke(Current, maxHealth);
    if (Current == 0) Died?.Invoke();
    return true;
}
```

Subscribing and unsubscribing (`CameraShake.cs`):

```csharp
void OnEnable()
{
    if (health == null) return;
    health.Damaged += OnDamaged;
    health.Died += OnDied;
}

void OnDisable()
{
    if (health == null) return;
    health.Damaged -= OnDamaged;
    health.Died -= OnDied;
}
```

## Files

```
Assets/
  Editor/NeonUIBuilder.cs        builds the menu and the gameplay scene
  Resources/
    GameMixer.mixer              made by hand, see Setup
    Audio/                       19 generated .wav files
  Scripts/                       all the scripts above, plus the Lab 07 ones
tools/generate_audio.py          generates the audio
```

## Credits

- All sound effects and music were made by me. `tools/generate_audio.py` synthesizes every file from sine, triangle, saw and square waves and noise, with no samples or third-party audio. I'm releasing them as CC0, so there's no licence to follow.
- The look of the menus is inspired by the mobile game Neon Runner (Google Play). No assets from it are used.
- Orbitron (SIL OFL 1.1) is used only if `Assets/Fonts/Orbitron SDF.asset` exists.
- Unity UGUI, TextMeshPro and Unity's built-in UI sprite. The gameplay shapes are plain coloured squares made in code.

The Shop and Leaderboard are still the static screens from Lab 07, apart from the coin and distance numbers.
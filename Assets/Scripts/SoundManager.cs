using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public AudioSource audioSourceOneShot;
    public AudioSource background;
    public AudioSource stationSounds;

    public AudioClip actionDone;
    public AudioClip backgroundMusic;
    public AudioClip boilingWater;
    public AudioClip burnerClicks;
    public AudioClip cut;
    public AudioClip errorNoise;
    public AudioClip howToPlay;
    public AudioClip inventorySwoosh;
    public AudioClip itemPickup;
    public AudioClip mixing;
    public AudioClip orderSent;
    public AudioClip platingNoise;
    public AudioClip trashing;
    public AudioClip timeEnd;
    public AudioClip waterSplash;

    private void Awake()
    {
        background.clip = backgroundMusic;
        background.Play();

        stationSounds.clip = boilingWater;
    }

    public void PlayActionDone() => PlaySound(actionDone);
    public void PlayBoilingWater(bool play) => PlayBoilingSound(play);
    public void PlayBurnerClicks() => PlaySound(burnerClicks);
    public void PlayCut() => PlaySound(cut);
    
    //TODO
    public void PlayErrorNoise() => PlaySound(errorNoise);
    
    //TODO
    public void PlayHowToPlay() => PlaySound(howToPlay);
    public void PlayInventorySwoosh() => PlaySound(inventorySwoosh);
    public void PlayItemPickup() => PlaySound(itemPickup);
    public void PlayMixing() => PlaySound(mixing);
    public void PlayOrderSent() => PlaySound(orderSent);
    public void PlayPlatingNoise() => PlaySound(platingNoise);
    public void PlayTrashing() => PlaySound(trashing);
    
    //TODO
    public void PlayTimeEnd() => PlaySound(timeEnd);
    public void PlayWaterSplash() => PlaySound(waterSplash);

    private void PlaySound(AudioClip clip)
    {
        if (audioSourceOneShot != null && clip != null)
        {
            audioSourceOneShot.PlayOneShot(clip);
        }

        audioSourceOneShot.Play();
    }

    public void PlayBoilingSound(bool play)
    {
        if (play)
        {
            stationSounds.Play();
        }
        else
        {
            stationSounds.Stop();
        }
    }




}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

class AudioGroups {
    public static String Master { get { return "Master"; } }
    public static String Buildup { get { return "Buildup"; } }
    public static String RetroBass { get { return "RetroBass"; } }
    public static String RetroArps { get { return "RetroArps"; } }
    public static String DreamsArps { get { return "DreamsArps"; } }

    public static String DrumtrackAlternate { get { return "DrumtrackAlternate"; } }

    public static String FuzzBass { get { return "FuzzBass"; } }

    public static String AnalogLead { get { return "AnalogLead"; } }
    public static String Tension { get { return "Tension"; } }
    public static String TensionBass { get { return "TensionBass"; } }
    public static String TensionLead { get { return "TensionLead"; } }
    public static String TensionArps { get { return "TensionDreamArps"; } }
    public static String TensionBassArps { get { return "TensionBassArps"; } }
    public static String TensionAnalogLeadDistorted { get { return "TensionAnalogLeadDistorted"; } }
    public static String Drumtrack { get { return "Drumtrack"; } }
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    public AudioMixer audioMixer;
    public AudioSource referenceSource;

    private float runLoopTime = 16.0f;
    private float activeRunLoopTime = 0.0f;

    public MusicState activeMusicState;
    private MusicState pendingMusicStateTransition;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        activeMusicState = new BuildupMusicState();
        activeMusicState.OnStart();

        runLoopTime = (referenceSource.clip.length / 2f) - 0.1f;
        pendingMusicStateTransition = new TensionMusicState();
    }

    private void Update()
    {
        activeRunLoopTime += Time.deltaTime;

        if (activeRunLoopTime >= runLoopTime)
        {
            activeRunLoopTime = 0.0f;
            runLoopCompleted();
        }
    }

    private void runLoopCompleted()
    {
        if (pendingMusicStateTransition != null)
        {
            pendingMusicStateTransition.OnStart();
            activeMusicState = pendingMusicStateTransition;
            pendingMusicStateTransition = null;
        }
        else
        {
            activeMusicState.Variate();
        }
    }

    public void MuteAudioGroup(String audioGroupName, float duration = 0.0f)
    {
        InterpolateValue(0f, -80f, duration, lerpedValue =>
        {
            audioMixer.SetFloat($"{audioGroupName}Volume", lerpedValue);
        });
    }

    public void UnmuteAudioGroup(String audioGroupName, float duration = 0)
    {
        InterpolateValue(-80f, 0f, duration, lerpedValue =>
        {
            audioMixer.SetFloat($"{audioGroupName}Volume", lerpedValue);
        });
    }

    public void SetDeathMusic() 
    {
        var startingValue = 1f;
        audioMixer.GetFloat("MasterPitch", out startingValue);

        InterpolateValue(startingValue, 0.4f, 1, lerpedValue =>
        {
            audioMixer.SetFloat("MasterPitch", lerpedValue);
        });
        
    }

    void InterpolateValue(float startValue, float targetValue, float duration, Action<float> callback)
    {
        var transition = TransitionValueCoroutine(startValue, targetValue, duration, callback);
        StartCoroutine(transition);
    }

    IEnumerator TransitionValueCoroutine(float startValue, float endValue, float duration, Action<float> callback)
    {
        if (startValue == endValue) {
            callback(startValue);
            yield break;
        }

        float progress = 0.0f;

        while (progress < 1.0)
        {
            progress += Time.unscaledDeltaTime / duration;
            var lerpedValue = Mathf.Lerp(startValue, endValue, progress);
            callback(lerpedValue);
            yield return null;
        }
    }
}

public interface MusicState
{
    void OnStart();
    void Variate();
}

class BuildupMusicState: MusicState
{
    int variationCount = 0;
    public void OnStart()
    {
        MusicManager.Instance.MuteAudioGroup(AudioGroups.Tension);
        MusicManager.Instance.MuteAudioGroup(AudioGroups.Drumtrack);
        MusicManager.Instance.UnmuteAudioGroup(AudioGroups.Buildup);
        MusicManager.Instance.UnmuteAudioGroup(AudioGroups.RetroBass, 1.25f);
    }

    public void Variate()
    {
        variationCount++;

        if (variationCount == 1)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.RetroArps, 1.25f);
        }

        else if (variationCount == 2)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.DrumtrackAlternate, 0.25f);
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.DreamsArps, 1.25f);
        }

        else if (variationCount == 3)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.FuzzBass, 0.0f);
        }

        else if (variationCount == 4)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.AnalogLead, 0.25f);
        }

        else if (variationCount >= 5)
        {
            if (variationCount % 2 == 0)
            {
                MusicManager.Instance.MuteAudioGroup(AudioGroups.DrumtrackAlternate, 0.0f);
                MusicManager.Instance.UnmuteAudioGroup(AudioGroups.AnalogLead, 0.0f);
            }
            else
            {
                MusicManager.Instance.MuteAudioGroup(AudioGroups.AnalogLead, 0.25f);
                MusicManager.Instance.UnmuteAudioGroup(AudioGroups.DrumtrackAlternate, 0.25f);
            }
        }
    }
}

class TensionMusicState : MusicState
{
    int variationCount = 0;
    public void OnStart()
    {
        MusicManager.Instance.UnmuteAudioGroup(AudioGroups.Tension);
        MusicManager.Instance.UnmuteAudioGroup(AudioGroups.TensionBass);
        MusicManager.Instance.MuteAudioGroup(AudioGroups.Buildup, 4.0f);
        
    }

    public void Variate()
    {
        variationCount++;

        if (variationCount == 1)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.TensionLead);
        }

        else if (variationCount == 2)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.Drumtrack);
        }

        else if (variationCount == 3)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.TensionArps, 0.75f);
        }

        else if (variationCount == 4)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.TensionAnalogLeadDistorted, 0.5f);
        }

        else if (variationCount >= 5)
        {
            if (variationCount % 2 == 0)
            {
                MusicManager.Instance.MuteAudioGroup(AudioGroups.TensionAnalogLeadDistorted, 0.0f);
            }
            else
            {
                MusicManager.Instance.UnmuteAudioGroup(AudioGroups.TensionAnalogLeadDistorted, 0.0f);
            }
        }
    }
}


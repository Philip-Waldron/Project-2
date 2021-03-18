using Project2.Scripts.Game_Logic;
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

    private double lastLoopTime;
    private double runLoopTime;
    double activeRunLoopTime = 0.0f;

    public MusicState activeMusicState;
    private MusicState pendingMusicStateTransition;

    public AnimationCurve linearAnimationCurve;
    public AnimationCurve logarithmicAnimationCurve;
    public AnimationCurve exponentialAnimationCurve;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        activeMusicState = new BuildupMusicState();
        activeMusicState.OnStart();

        lastLoopTime = AudioSettings.dspTime;
        runLoopTime = (((double)referenceSource.clip.samples / referenceSource.clip.frequency) / 2d);

        //pendingMusicStateTransition = new TensionMusicState();
    }

    private void Update()
    {
        activeRunLoopTime = (AudioSettings.dspTime);

        if ((activeRunLoopTime - lastLoopTime) >= runLoopTime)
        {
            lastLoopTime = activeRunLoopTime;
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
        audioMixer.GetFloat($"{audioGroupName}Volume", out float existingValue);

        InterpolateValue(existingValue, -80f, duration, lerpedValue =>
        {
            audioMixer.SetFloat($"{audioGroupName}Volume", lerpedValue);
        }, exponentialAnimationCurve);
    }

    public void UnmuteAudioGroup(String audioGroupName, float duration = 0)
    {
        audioMixer.GetFloat($"{audioGroupName}Volume", out float existingValue);

        InterpolateValue(existingValue, -0.05f, duration, lerpedValue =>
        {
            audioMixer.SetFloat($"{audioGroupName}Volume", lerpedValue);
        }, logarithmicAnimationCurve);
    }

    public void OnTimeBelowThreshold()
    {
        pendingMusicStateTransition = new TensionMusicState();
    }

    public void SetDeathMusic() 
    {
        audioMixer.GetFloat("MasterPitch", out float startingValue);

        InterpolateValue(startingValue, 0.4f, 1, lerpedValue =>
        {
            audioMixer.SetFloat("MasterPitch", lerpedValue);
        }, linearAnimationCurve);
    }

    void InterpolateValue(float startValue, float targetValue, float duration, Action<float> callback, AnimationCurve curve)
    {
        var transition = TransitionValueCoroutine(startValue, targetValue, duration, callback, curve);
        StartCoroutine(transition);
    }

    IEnumerator TransitionValueCoroutine(float startValue, float endValue, float duration, Action<float> callback, AnimationCurve curve)
    {
        if (startValue == endValue || duration == 0) {
            callback(endValue);
            yield break;
        }

        float progress = 0.0f;

        while (progress < 1.0)
        {
            progress += Time.unscaledDeltaTime / duration;
            var animationProgress = curve.Evaluate(progress);
            var lerpedValue = Mathf.LerpUnclamped(startValue, endValue, animationProgress);
            callback(lerpedValue);
            yield return null;
        }
        callback(endValue);
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
        MusicManager.Instance.UnmuteAudioGroup(AudioGroups.RetroBass, 1.4f);
    }

    public void Variate()
    {
        variationCount++;

        if (variationCount == 1)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.RetroArps, 0.0f);
        }

        else if (variationCount == 2)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.DrumtrackAlternate, 0.0f);
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.DreamsArps, 0.0f);
        }

        else if (variationCount == 3)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.FuzzBass, 0.0f);
        }

        else if (variationCount == 4)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.AnalogLead, 0.0f);
        }

        else if (variationCount >= 5)
        {
            if (variationCount % 2 == 0)
            {
                MusicManager.Instance.MuteAudioGroup(AudioGroups.DrumtrackAlternate, 0.1f);
                MusicManager.Instance.UnmuteAudioGroup(AudioGroups.AnalogLead, 0.1f);
            }
            else
            {
                MusicManager.Instance.UnmuteAudioGroup(AudioGroups.DrumtrackAlternate, 0.0f);
                MusicManager.Instance.MuteAudioGroup(AudioGroups.AnalogLead, 0.0f);
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
        MusicManager.Instance.UnmuteAudioGroup(AudioGroups.TensionBass, 0.1f);
        MusicManager.Instance.MuteAudioGroup(AudioGroups.Buildup, 4.0f);
        
    }

    public void Variate()
    {
        variationCount++;

        if (variationCount == 1)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.TensionLead, 0.0f);
        }

        else if (variationCount == 2)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.Drumtrack, 0.0f);
        }

        else if (variationCount == 3)
        {
            MusicManager.Instance.UnmuteAudioGroup(AudioGroups.TensionArps, 0.0f);
        }
    }
}


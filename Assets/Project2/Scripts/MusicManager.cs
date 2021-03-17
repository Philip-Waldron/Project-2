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
    public static String Tension { get { return "Tension"; } }
    public static String TensionBass { get { return "TensionBass"; } }
    public static String TensionLead { get { return "TensionLead"; } }
    public static String TensionArps { get { return "TensionDreamArps"; } }
    public static String Drumtrack { get { return "Drumtrack"; } }
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance { get; private set; }
    public AudioMixer audioMixer;
    public AudioSource referenceSource;

    private float runLoopTime = 16.0f;
    private float activeRunLoopTime = 0.0f;

    public MusicState activeMusicState;
    private MusicState pendingMusicStateTransition;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        activeMusicState = new BuildupMusicState();
        activeMusicState.OnStart();

        runLoopTime = (referenceSource.clip.length / 2f) - 0.1f;
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

    public void MuteAudioGroup(String audioGroupName)
    {
        audioMixer.SetFloat($"{audioGroupName}Volume", -80.0f);
    }

    public void UnmuteAudioGroup(String audioGroupName)
    {
        audioMixer.SetFloat($"{audioGroupName}Volume", 0.0f);
    }

    public void SetDeathMusic() 
    {
        audioMixer.SetFloat("MasterPitch", 0.4f);
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
        MusicManager.instance.MuteAudioGroup(AudioGroups.Tension);
        MusicManager.instance.MuteAudioGroup(AudioGroups.Drumtrack);
        MusicManager.instance.UnmuteAudioGroup(AudioGroups.Buildup);
        MusicManager.instance.UnmuteAudioGroup(AudioGroups.RetroBass);
    }

    public void Variate()
    {
        variationCount++;

        if (variationCount == 1)
        {
            MusicManager.instance.UnmuteAudioGroup(AudioGroups.Drumtrack);
        }

        else if (variationCount == 2)
        {
            MusicManager.instance.UnmuteAudioGroup(AudioGroups.RetroArps);
        }

        else if (variationCount == 3)
        {
            MusicManager.instance.UnmuteAudioGroup(AudioGroups.DreamsArps);
        }

        else if (variationCount >= 3)
        {
            var randomValue = new System.Random().Next(0, 2);
            if (randomValue == 1)
            {
                MusicManager.instance.UnmuteAudioGroup(AudioGroups.Drumtrack);
                MusicManager.instance.UnmuteAudioGroup(AudioGroups.RetroArps);
                MusicManager.instance.MuteAudioGroup(AudioGroups.DreamsArps);
            }
            else if(randomValue == 2)
            {
                MusicManager.instance.UnmuteAudioGroup(AudioGroups.Drumtrack);
                MusicManager.instance.UnmuteAudioGroup(AudioGroups.DreamsArps);
                MusicManager.instance.MuteAudioGroup(AudioGroups.RetroArps);
            }
            else
            {
                MusicManager.instance.MuteAudioGroup(AudioGroups.Drumtrack);
                MusicManager.instance.UnmuteAudioGroup(AudioGroups.RetroArps);
                MusicManager.instance.UnmuteAudioGroup(AudioGroups.DreamsArps);
            }
        }
    }
}

class TensionMusicState : MusicState
{
    int variationCount = 0;
    public void OnStart()
    {
        MusicManager.instance.MuteAudioGroup(AudioGroups.Buildup);
        MusicManager.instance.MuteAudioGroup(AudioGroups.Drumtrack);
        MusicManager.instance.UnmuteAudioGroup(AudioGroups.Tension);
        MusicManager.instance.UnmuteAudioGroup(AudioGroups.TensionBass);
    }

    public void Variate()
    {
        variationCount++;

        if (variationCount == 1)
        {
            MusicManager.instance.UnmuteAudioGroup(AudioGroups.TensionLead);
        }

        else if (variationCount == 2)
        {
            MusicManager.instance.UnmuteAudioGroup(AudioGroups.Drumtrack);
        }

        else if (variationCount == 3)
        {
            MusicManager.instance.UnmuteAudioGroup(AudioGroups.TensionArps);
        }

        else if (variationCount >= 3)
        {
            var randomValue = new System.Random().Next(0, 1);
            if (randomValue == 0)
            {
                MusicManager.instance.UnmuteAudioGroup(AudioGroups.TensionArps);
            }
            else if (randomValue == 1)
            {
                MusicManager.instance.MuteAudioGroup(AudioGroups.TensionArps);
            }
        }
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;

public class MultiSpeakerQueue : MonoBehaviour
{
    public Queue<AudioClip> audioQueue = new Queue<AudioClip>();

    int clipChoose = 0;

    int sampleRate = 48000;

    int samplesPerCall = 480;

    public AudioSource[] audioSources;

    #region AudioClips

    //Create audioclip names
    public AudioClip clip1;
    public AudioClip clip2;
    public AudioClip clip3;
    public AudioClip clip4;
    public AudioClip clip5;
    public AudioClip clip6;
    public AudioClip clip7;
    public AudioClip clip8;
    public AudioClip clip9;
    public AudioClip clip10;

    #endregion

    public AudioClip[] clipArray;

    //to switch between audiosources
    int toggle = 0;

    double nextStartTime = 0;

    public void Awake()
    {
        foreach (AudioSource speaker in audioSources)
        {
            speaker.volume = 1f;
        }

        #region clips
        clip1 = AudioClip.Create("_clip1", samplesPerCall, 2, sampleRate, false);
        clip2 = AudioClip.Create("_clip2", samplesPerCall, 2, sampleRate, false);
        clip3 = AudioClip.Create("_clip3", samplesPerCall, 2, sampleRate, false);
        clip4 = AudioClip.Create("_clip4", samplesPerCall, 2, sampleRate, false);
        clip5 = AudioClip.Create("_clip5", samplesPerCall, 2, sampleRate, false);
        clip6 = AudioClip.Create("_clip6", samplesPerCall, 2, sampleRate, false);
        clip7 = AudioClip.Create("_clip7", samplesPerCall, 2, sampleRate, false);
        clip8 = AudioClip.Create("_clip8", samplesPerCall, 2, sampleRate, false);
        clip9 = AudioClip.Create("_clip9", samplesPerCall, 2, sampleRate, false);
        clip10 = AudioClip.Create("_clip10", samplesPerCall, 2, sampleRate, false);
        #endregion

        clipArray = new AudioClip[] { clip1, clip2, clip3, clip4, clip5, clip6, clip7, clip8, clip9, clip10 };

        audioQueue.Clear();
    }


    //Function "Queue" called everytime and AudioFrame callback is called from ServerIntialise script
    public void Queue(AudioFrame audioFrame)
    {
        if (clipChoose == 9)
        {
            clipChoose = 0;
        }
        Debug.Log("Clip: "+clipChoose);


        //converts byte[] to float[]
        float[] clipFloat = byteToFloat(audioFrame.buffer);
        

        Dispatcher.ExecuteOnMainThread.Enqueue(() =>
        {
            //Sets data of AudioClip 0 to incoming AudioFrame
            clipArray[clipChoose].SetData(clipFloat, 0);
                
            //Adds to back of queue
            audioQueue.Enqueue(clipArray[clipChoose]);
        });

        ++clipChoose;
    }

    public float[] byteToFloat(byte[] input)
    {
        int inputSamples = input.Length / 2;
        float[] output = new float[inputSamples];
        int outputIndex = 0;
        for (int n = 0; n < inputSamples; n++)
        {
            short sample = BitConverter.ToInt16(input, n * 2);
            output[outputIndex++] = sample / 32768f;
        }
        return output;
    }


    private void Update()
    {
        if (audioQueue.Count != 0)
        {
            //if the clock is greater than the start time of next clip - 0.005 (clip is 0.01)
            if (AudioSettings.dspTime > nextStartTime - 0.005)
            {
                //pull AudioClip from the front of queue
                audioSources[toggle].clip = audioQueue.Dequeue();

                //set to play on correct source and at correct time
                audioSources[toggle].PlayScheduled(nextStartTime);

                //time difference needed to playback the next clip as soon as current clip ends
                double duration = (double)audioSources[toggle].clip.samples / audioSources[toggle].clip.frequency;

                //adds on new difference
                nextStartTime = nextStartTime + duration;

                Debug.Log("Next start time: " + nextStartTime);

                //flips to second audiosource
                toggle = 1 - toggle;
            }
        }
    }
}

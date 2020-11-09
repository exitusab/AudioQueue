﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;

public class AudioQueue : MonoBehaviour
{
    public Queue<float[]> audioQueue = new Queue<float[]>();

    public AudioSource audioSource;
    AudioClip audioFrameClip;

    private int queueMax = 5;
    bool isPlaying = false;
    bool clipCreated = false;

    public void Awake()
    {
        audioQueue.Clear();
    }

    public void Queue(AudioFrame audioFrame)
    {
        Dispatcher.ExecuteOnMainThread.Enqueue(() =>
        {
            if (!clipCreated)
            {
                audioFrameClip = AudioClip.Create("clip", audioFrame.samples, audioFrame.channels, audioFrame.samplesPerSec, false);
                audioSource.clip = audioFrameClip;
                clipCreated = true;
            }
        });
        float[] clipFloat = PCM2Floats(audioFrame.buffer);

        audioQueue.Enqueue(clipFloat);
        if (audioQueue.Count >= queueMax)
        {
            Dispatcher.ExecuteOnMainThread.Enqueue(() =>
            {
                if (!isPlaying && clipCreated)
                {
                    PlayClip(audioQueue.Peek());
                }
            });
        }
    }

    private void PlayClip(float[] clipFloat)
    {
        audioFrameClip.SetData(clipFloat, 0);
        audioSource.Play();
        audioQueue.Dequeue();
   
    }

    public static float[] PCM2Floats(byte[] bytes)
    {
        float max = -short.MinValue;
        float[] samples = new float[bytes.Length / 2];

        for (int i = 0; i < samples.Length; i++)
        {
            short int16sample = BitConverter.ToInt16(bytes, i * 2);
            samples[i] = int16sample / max;
        }

        return samples;
    }
}

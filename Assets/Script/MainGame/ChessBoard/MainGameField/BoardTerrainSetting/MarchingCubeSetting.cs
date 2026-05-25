using System;
using UnityEngine;
using Zenject;


[Serializable]
public class MarchingCubeSetting 
{
    public int width;
    public int height;
    public float resolution;
    public float noiseScale;
    public float heightThreshold;
    public bool use3DNoise = false;

    public float[,,] heights;
}

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Serialization;
using MyBox;

[Serializable]
public class Ring
{
    public GameObject parent;
    public GameObject[] cubes;

    public Ring() { }

    public Ring(GameObject parent, GameObject[] cubes)
    {
        this.parent = parent;
        this.cubes = cubes;
    }
}

public enum RingState 
{
    Single,
    Multiple
}
public enum ColorState
{
    None,
    Hue, 
    Saturation
}

public class AudioVisualizer : MonoBehaviour
{
    public GameObject prefab;
    public GameObject parent;
    public AudioSource src;
    public RingState ringState = RingState.Single;
    [ConditionalField("ringState", false, RingState.Single)] 
    public float radius = 15f;
    [ConditionalField("ringState", false, RingState.Multiple), Min(2)] 
    public int numberOfRings = 2;
    [ConditionalField("ringState", false, RingState.Multiple)] 
    public float maxRadius = 20f;
    [ConditionalField("ringState", false, RingState.Multiple)] 
    public float minRadius = 10f;
    
    public float yScale = 1f;
    public float xScale = 1f;
    public int range = 1024;
    public float ringRotateSpeed;
    public float rotateSpeed;
    public bool topOnly;
    public ColorState changeColor;

    [ConditionalField("changeColor", false, ColorState.Hue), Min(0)]
    public float startingHue = 0.67f;

    [ConditionalField("changeColor", false, ColorState.Hue)]
    public float shiftFactor = -4f;

    public Ring[] rings;

    private void Start ()
    {
        rings = new Ring[ringState == RingState.Single ? 1 : numberOfRings];
        for (int i = 0; i < rings.Length; i++)
        {
            rings[i] = new Ring
            {
                parent = new GameObject(),
                cubes = new GameObject[ringState == RingState.Single 
                    ? Convert.ToInt32(Mathf.PI * radius * xScale)
                    : Convert.ToInt32(Mathf.PI * (maxRadius - ((maxRadius - minRadius) * Convert.ToSingle(Mathf.Log(1f + (i) / (numberOfRings - 1f), 2)))) * xScale)
                ]
            };
            rings[i].parent.transform.parent = parent.transform;
            for (int j = 0; j < rings[i].cubes.Length; j++) 
            {
                float angle = j * Mathf.PI * 2 / rings[i].cubes.Length;
                Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) 
                    * (ringState == RingState.Single ? radius : (maxRadius - ((maxRadius - minRadius) * Convert.ToSingle(Mathf.Log(1f + (i)/(numberOfRings-1f), 2)))));
                GameObject tmp = Instantiate(prefab, pos, Quaternion.identity);
                tmp.transform.parent = rings[i].parent.transform;
                rings[i].cubes[j] = tmp;
            }
        }
    }

    private void Update () 
    {
        float[] spectrum = new float[range];
        src.GetSpectrumData (spectrum, 0, FFTWindow.Hanning);
        for (int i = 0; i < rings.Length; i++) 
        {
            for (int j = 0; j < rings[i].cubes.Length; j++) 
            {
                rings[i].cubes[j].transform.localScale = new Vector3(rings[i].cubes[j].transform.localScale.x, spectrum[(rings[i].cubes.Length*i) + j] * yScale * (topOnly ? 25 : 50), rings[i].cubes[j].transform.localScale.z);;
                rings[i].cubes[j].transform.position = new Vector3(rings[i].cubes[j].transform.position.x, topOnly ? (spectrum[(rings[i].cubes.Length*i) + j] * yScale) * 12.5f : 0, rings[i].cubes[j].transform.position.z);
                if (changeColor == ColorState.Hue)
                {
                    startingHue = Mathf.Clamp(startingHue, 0f, 1f);
                    rings[i].cubes[j].GetComponent<Renderer>().material.SetColor("_Color", Color.HSVToRGB(Mathf.Clamp(startingHue+(spectrum[(rings[i].cubes.Length*i) + j]*shiftFactor), 0f, 1f), 1, 1));
                }
            }
        }

        for (int i = 0; i < rings.Length; i++) 
        {
            rings[i].parent.transform.Rotate(0, (i % 2 == 0 ? ringRotateSpeed : -ringRotateSpeed), 0);
            for (int j = 0; j < rings[i].cubes.Length; j++) 
            {
                rings[i].cubes[j].transform.Rotate (0, (i % 2 == 0 ? rotateSpeed : -rotateSpeed), 0);		
            }
        }
    }
}
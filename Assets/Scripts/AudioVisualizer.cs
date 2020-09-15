using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Serialization;

[Serializable]
public class Ring
{
    public GameObject parent;
    public GameObject[] cubes;

    public Ring()
    {
    }

    public Ring(GameObject parent, GameObject[] cubes)
    {
        this.parent = parent;
        this.cubes = cubes;
    }
}

public class AudioVisualizer : MonoBehaviour
{
    public GameObject prefab;
    public GameObject parent;
    public int numberOfRings = 1;
    public AudioSource src;
    public float maxRadius = 20f;
    public float minRadius = 10f;
    public float yScale = 1f;
    public float xScale = 1f;
    public int range = 1024;
    public float ringRotateSpeed;
    public float rotateSpeed;
    public bool changeColor;
    public bool topOnly;

    public Ring[] rings;

    private void Start ()
    {
        rings = new Ring[numberOfRings];
        for (int i = 0; i < numberOfRings; i++)
        {
            rings[i] = new Ring
            {
                parent = new GameObject(),
                cubes = new GameObject[Convert.ToInt32(
                    Mathf.PI * (maxRadius - ((maxRadius - minRadius) *
                                             Convert.ToSingle(Mathf.Log(1f + (i) / (numberOfRings - 1f), 2)))) *
                    xScale)]
            };
            rings[i].parent.transform.parent = parent.transform;
            for (int j = 0; j < rings[i].cubes.Length; j++) {
                float angle = j * Mathf.PI * 2 / rings[i].cubes.Length;
                Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) *
                              (maxRadius - ((maxRadius - minRadius) * Convert.ToSingle(Mathf.Log(1f + (i)/(numberOfRings-1f), 2))));
                GameObject tmp = Instantiate(prefab, pos, Quaternion.identity);
                tmp.transform.parent = rings[i].parent.transform;
                rings[i].cubes[j] = tmp;
            }
        }
    }

    private void Update () {
        float[] spectrum = new float[range];
        src.GetSpectrumData (spectrum, 0, FFTWindow.Hanning);
        for (int i = 0; i < numberOfRings; i++) {
            for (int j = 0; j < rings[i].cubes.Length; j++) {
                rings[i].cubes[j].transform.localScale = new Vector3(rings[i].cubes[j].transform.localScale.x, spectrum[(rings[i].cubes.Length*i) + j] * yScale * (topOnly ? 25 : 50), rings[i].cubes[j].transform.localScale.z);;
                rings[i].cubes[j].transform.position = new Vector3(rings[i].cubes[j].transform.position.x, topOnly ? (spectrum[(rings[i].cubes.Length*i) + j] * yScale) * 12.5f : 0, rings[i].cubes[j].transform.position.z);
                if (changeColor) {
                    rings[i].cubes[j].GetComponent<Renderer>().material.SetColor("_Color", Color.HSVToRGB(0.67f-(spectrum[(rings[i].cubes.Length*i) + j]*4f), 1, 1));
                }
            }
        }

        for (int i = 0; i < numberOfRings; i++) {
            rings[i].parent.transform.Rotate(0, (i % 2 == 0 ? ringRotateSpeed : -ringRotateSpeed), 0);
            for (int j = 0; j < rings[i].cubes.Length; j++) {
                rings[i].cubes[j].transform.Rotate (0, (i % 2 == 0 ? rotateSpeed : -rotateSpeed), 0);		
            }
        }
    }
}
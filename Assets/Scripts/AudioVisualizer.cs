using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Serialization;

public class AudioVisualizer : MonoBehaviour
{
    public GameObject prefab;
    public GameObject parent;
    public int rings = 1;
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
    
    private GameObject[][] _cubes;
    private GameObject[] _ringsParents;

    private void Start () {
        _cubes = new GameObject[rings][];
        _ringsParents = new GameObject[rings];
        for (int i = 0; i < rings; i++) {
            _ringsParents[i] = new GameObject();
            _ringsParents[i].transform.parent = parent.transform;
            _cubes[i] = new GameObject[Convert.ToInt32(Mathf.PI * (maxRadius - (i * ((maxRadius-minRadius)/(rings-1)))) * xScale)];
            for (int j = 0; j < _cubes[i].Length; j++) {
                float angle = j * Mathf.PI * 2 / _cubes[i].Length;
                Vector3 pos = new Vector3 (Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (maxRadius - (i * ((maxRadius-minRadius)/(rings-1))));
                GameObject tmp = Instantiate(prefab, pos, Quaternion.identity);
                tmp.transform.parent = _ringsParents[i].transform;
                _cubes[i][j] = tmp;
            }
        }
    }

    private void Update () {
        float[] spectrum = new float[range];
        src.GetSpectrumData (spectrum, 0, FFTWindow.Hanning);
        for (int i = 0; i < rings; i++) {
            for (int j = 0; j < _cubes[i].Length; j++) {
                _cubes[i][j].transform.localScale = new Vector3(_cubes[i][j].transform.localScale.x, spectrum[(_cubes[i].Length*i) + j] * yScale * (topOnly ? 25 : 50), _cubes[i][j].transform.localScale.z);;
                _cubes[i][j].transform.position = new Vector3(_cubes[i][j].transform.position.x, topOnly ? (spectrum[(_cubes[i].Length*i) + j] * yScale) * 12.5f : 0, _cubes[i][j].transform.position.z);
                if (changeColor) {
                    _cubes[i][j].GetComponent<Renderer>().material.SetColor("_Color", Color.HSVToRGB(0.67f-(spectrum[(_cubes[i].Length*i) + j]*4f), 1, 1));
                }
            }
        }

        for (int i = 0; i < rings; i++) {
            _ringsParents[i].transform.Rotate(0, (i % 2 == 0 ? ringRotateSpeed : -ringRotateSpeed), 0);
            for (int j = 0; j < _cubes[i].Length; j++) {
                _cubes[i][j].transform.Rotate (0, (i % 2 == 0 ? rotateSpeed : -rotateSpeed), 0);		
            }
        }
    }
}
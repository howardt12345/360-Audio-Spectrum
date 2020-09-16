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

    public Ring(GameObject parent)
    {
        this.parent = parent;
        cubes = new GameObject[parent.gameObject.transform.childCount];
        for (int i = 0; i < cubes.Length; i++)
        {
            cubes[i] = parent.gameObject.transform.GetChild(i).gameObject;
        }
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
    public AudioSource audioSource;
    public bool startAudioWithDelay;
    [ConditionalField(nameof(startAudioWithDelay))]
    public int delay = 2;
    public bool generate = true;
    [ConditionalField(nameof(generate))] 
    public bool addAnimationRecorder = true;
    [Separator]
    public RingState ringState = RingState.Single;
    [ConditionalField(nameof(ringState), false, RingState.Single)] 
    public float radius = 15f;
    [ConditionalField(nameof(ringState), false, RingState.Multiple), Min(2)] 
    public int numberOfRings = 2;
    [ConditionalField(nameof(ringState), false, RingState.Multiple)] 
    public float maxRadius = 20f;
    [ConditionalField(nameof(ringState), false, RingState.Multiple)] 
    public float minRadius = 10f;

    [Separator]
    public float yScale = 20f;
    public float xScale = 1f;
    [DefinedValues(64, 128, 256, 512, 1024, 2048, 4096, 8192)]
    public int range = 1024;
    public float ringRotateSpeed;
    public float rotateSpeed;
    public bool topOnly;
    
    [Separator]
    public ColorState changeColor;
    [ConditionalField(nameof(changeColor), false, ColorState.Hue, ColorState.Saturation)]
    public bool updateOnRuntime;
    [ConditionalField(nameof(changeColor), false, ColorState.Hue), Min(0)]
    public float startingHue = 0.67f;
    [ConditionalField(nameof(changeColor), false, ColorState.Hue)]
    public float shiftFactor = -4f;

    [Separator] 
    public GameObject audioVisualizerParent;
    public Ring[] rings;
    #if UNITY_EDITOR
    [ButtonMethod]
    private string CollectCubes()
    {
        int total = 0;
        if (audioVisualizerParent != null)
        {
            rings = new Ring[audioVisualizerParent.gameObject.transform.childCount];
            for (int i = 0; i < rings.Length; i++)
            {
                rings[i] = new Ring(audioVisualizerParent.gameObject.transform.GetChild(i).gameObject);
                total += rings[i].cubes.Length;
            }
        }
        else
        {
            for (int i = 0; i < rings.Length; i++)
            {
                if (rings[i].parent != null)
                {
                    rings[i] = new Ring(rings[i].parent);
                    total += rings[i].cubes.Length;
                }
                else
                {
                    Debug.LogError("Parent at index " + i + " is null.");
                }
            }
        }
        return total + " cubes from " + rings.Length + " rings collected.";
    }
    [ButtonMethod]
    private void RegenerateCubes()
    {
        for (int i = 0; i < rings.Length; i++)
        {
            for (int j = 0; j < rings[i].cubes.Length; j++)
            {
                Destroy(rings[i].cubes[j]);
            }
            Destroy(rings[i].parent);
        }
        Generate();
    }
    #endif

    private bool _canRotate;

    private void Start ()
    {
        if (generate)
            Generate();
        
        if (startAudioWithDelay)
            StartCoroutine(PlaySoundAfterDelay());
        else
        {
            audioSource.Play();
            _canRotate = true;
        }
    }

    private void Generate()
    {
        int total = 0;
        
        //Instantiate Parent object
        GameObject p = new GameObject();
        //Set parent properties
        p.gameObject.transform.position = parent.transform.position;
        p.gameObject.transform.rotation = parent.transform.rotation;
        p.gameObject.transform.localScale = parent.transform.localScale;
        p.gameObject.name = "Audio Visualizer";
        audioVisualizerParent = p;
        if(addAnimationRecorder)
            p.AddComponent<UnityAnimationRecorder>();
        
        //Initialize Rings
        rings = new Ring[ringState == RingState.Single ? 1 : numberOfRings];
        
        for (int i = 0; i < rings.Length; i++)
        {
            //Create new Ring
            rings[i] = new Ring
            {
                parent = new GameObject(),
                cubes = new GameObject[
                    ringState == RingState.Single 
                        ? Convert.ToInt32(Mathf.PI * radius * xScale)
                        : Convert.ToInt32(Mathf.PI * (maxRadius - ((maxRadius - minRadius) * Convert.ToSingle(Mathf.Log(1f + (i) / (numberOfRings - 1f), 2)))) * xScale)
                ]
            };
            //Set Ring Property
            rings[i].parent.transform.parent = p.transform;
            rings[i].parent.gameObject.name = "Ring " + i;
            //Initialize the cubes in the ring
            for (int j = 0; j < rings[i].cubes.Length; j++) 
            {
                //Calculate the location that the cube will
                float angle = j * Mathf.PI * 2 / rings[i].cubes.Length;
                Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) 
                  * (ringState == RingState.Single 
                      ? radius 
                      : (maxRadius - ((maxRadius - minRadius) * Convert.ToSingle(Mathf.Log(1f + (i)/(numberOfRings-1f), 2))))
                  );
                //Instantiate cube from prefab
                GameObject tmp = Instantiate(prefab, pos, Quaternion.identity);
                //Set cube property
                tmp.transform.parent = rings[i].parent.transform;
                tmp.gameObject.name = "Ring " + i + " Cube " + j;
                //Add color change component
                tmp.AddComponent<ColorChange>();
                UpdateColor(tmp.GetComponent<ColorChange>());
                //Add to list
                rings[i].cubes[j] = tmp;
            }
            total += rings[i].cubes.Length;
        }
        Debug.Log(rings.Length + " rings generated. " + total + " cubes generated. ");
    }
    
    private IEnumerator PlaySoundAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        audioSource.Play();
        _canRotate = true;
    }

    private void Update () 
    {
        float[] spectrum = new float[range];
        audioSource.GetSpectrumData (spectrum, 0, FFTWindow.Hanning);
        for (int i = 0; i < rings.Length; i++) 
        {
            if(_canRotate)
                rings[i].parent.transform.Rotate(0, (i % 2 == 0 ? ringRotateSpeed : -ringRotateSpeed), 0);
            for (int j = 0; j < rings[i].cubes.Length; j++) 
            {
                rings[i].cubes[j].transform.localScale = new Vector3(rings[i].cubes[j].transform.localScale.x, spectrum[(rings[i].cubes.Length*i) + j] * yScale * (topOnly ? 2 : 4), rings[i].cubes[j].transform.localScale.z);;
                rings[i].cubes[j].transform.position = new Vector3(rings[i].cubes[j].transform.position.x, topOnly ? (spectrum[(rings[i].cubes.Length*i) + j] * yScale) : 0, rings[i].cubes[j].transform.position.z);
                if (updateOnRuntime)
                {
                    ColorChange cubeColor = rings[i].cubes[j].GetComponent<ColorChange>();
                    UpdateColor(cubeColor);
                }
                if(_canRotate)
                    rings[i].cubes[j].transform.Rotate (0, (i % 2 == 0 ? rotateSpeed : -rotateSpeed), 0);	
            }
        }
    }

    private void UpdateColor(ColorChange cubeColor)
    {
        cubeColor.changeColor = changeColor;
        cubeColor.yScale = yScale;
        cubeColor.startingHue = startingHue;
        cubeColor.shiftFactor = shiftFactor;
        cubeColor.topOnly = topOnly;
    }
}
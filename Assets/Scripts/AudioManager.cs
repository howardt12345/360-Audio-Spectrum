using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public bool startAudioWithDelay;
    [ConditionalField(nameof(startAudioWithDelay))]
    public int delay = 2;

    // Start is called before the first frame update
    void Start()
    {
        if (startAudioWithDelay)
            StartCoroutine(PlaySoundAfterDelay());
        else
        {
            audioSource.Play();
        }
    }
    
    private IEnumerator PlaySoundAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        audioSource.Play();
    }
}

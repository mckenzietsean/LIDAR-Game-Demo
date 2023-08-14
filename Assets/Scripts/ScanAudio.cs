using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public void StartClip()
    {
        audioSource.volume = 1;
        audioSource.Play();
    }

    public void EndClip()
    {
        StartCoroutine(SlowlyEndClip());
    }

    private IEnumerator SlowlyEndClip()
    {
        for (float i = 1; i > 0; i -= 0.15f)
        {
            audioSource.volume = i;
            yield return new WaitForSeconds(0.05f);
        }    
        audioSource.Stop();
    }
}

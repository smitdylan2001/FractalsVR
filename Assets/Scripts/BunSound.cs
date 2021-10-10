using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunSound : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(StartSounds(GetComponent<AudioSource>()));
    }

    IEnumerator StartSounds(AudioSource ac)
	{
        yield return new WaitForSeconds(8f);
        if(!ac.isPlaying) ac.Play();
	}
}

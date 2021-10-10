using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    private AudioSource _monsterAudio;
    private GameObject _monsterAudioObject;
    private AudioClip _monsterStart;
    private GameObject _playerGO;

	public AudioManager(GameObject player, AudioClip defaultaudio)
	{
		_playerGO = player;

		_monsterAudioObject = MakeAudioGO();
		_monsterAudio = MakeAudioSource(_monsterAudioObject);
		_monsterAudio.clip = _monsterStart = defaultaudio;
	}

	private GameObject MakeAudioGO()
	{
		GameObject go = new GameObject("Monster Audio");
		go.transform.parent = _playerGO.transform;

		return go;
	}

	private AudioSource MakeAudioSource(GameObject go)
	{
		AudioSource audioSource = go.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.maxDistance = 30;
		audioSource.spatialBlend = 1;
		audioSource.volume = 1;

		return audioSource;
	}

    public void PlayAudio(Vector3 direction)
    {
        _monsterAudioObject.transform.position = direction *3;
        _monsterAudio.clip = _monsterStart;
        _monsterAudio.Play();
    }
    public void PlayAudio(AudioClip clip, Vector3 direction)
	{
        _monsterAudioObject.transform.position = direction.normalized;
        _monsterAudio.clip = clip;
        _monsterAudio.Play();
	}
}

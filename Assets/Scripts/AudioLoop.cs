using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLoop : MonoBehaviour
{
    AudioSource _ac;
    [SerializeField] float _waitTime = 5f;
    [SerializeField] GameObject _player;
    static public bool willLoop = true;

    void Start()
    {
        _ac = GetComponent<AudioSource>();
        _player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(AudioRoutine());
    }

    IEnumerator AudioRoutine()
	{
		while (willLoop)
		{
			yield return new WaitForSeconds(Mathf.Clamp(_waitTime * (Vector3.Distance(_player.transform.position, gameObject.transform.position) /20), 0.2f, 10f));
            _ac.Stop();
            _ac.Play();
		}

		yield return null;
	}
}

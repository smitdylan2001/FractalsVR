using System.Collections;
using UnityEngine;
using BNG;

public class GameManager : MonoBehaviour
{
    public GameObject Player { get; private set; }
    [SerializeField] private GameObject[] _levers;
    [SerializeField] private GameObject _startGO;
    TaskManager _taskManager;
    AudioManager _audioManager;
    FractalPP _fractalScript;
    bool _audioPlayed = true;
    [SerializeField] GameObject _bunGO;
    [SerializeField] private BunBun _bun;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");

		_taskManager = GetComponent<TaskManager>();
        _fractalScript = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FractalPP>();

        _audioManager = new AudioManager(Player, Resources.Load<AudioClip>("Audio/MonsterStart"));
        _audioManager.PlayAudio(-Vector3.forward *10);
        
        _audioPlayed = true;
    }

    private void Update()
    {
        if(_fractalScript._bunnyTime)
        {
            _bun.SpawnBunBun();
            _audioPlayed = false;
        }
        if (InputBridge.Instance.RightThumbstickDown)
        {
            _fractalScript._CurrentSwitch = _taskManager.AnotherSliderDown();
        }
    }

    public void OnHandleDown(GameObject go)
	{
        var l = go.GetComponent<Lever>();

        if (l)
        {
            l.enabled = false;
		}

        AudioSource ac = go.transform.parent.parent.GetComponent<AudioSource>();

		if (ac)
		{
            ac.enabled = false;
		}

        SwitchScene();

        if(_fractalScript._CurrentSwitch == 1)
		{
            StartCoroutine(RepeatAudio());
        }
        else if(_fractalScript._CurrentSwitch == 5)
		{
            foreach (GameObject g in _levers)
            {
                g.GetComponentInChildren<Lever>().enabled = false;
            }
        }
        else if(_fractalScript._CurrentSwitch == 6)
		{
            _audioPlayed = false;
		}
        
    }

    void SwitchScene()
	{
        _fractalScript._CurrentSwitch = _taskManager.AnotherSliderDown();
    }

	public void EnableHandles()
	{
        foreach(GameObject g in _levers)
		{
            g.SetActive(true);
		}
	}

    private IEnumerator RepeatAudio()
	{
        _audioManager.PlayAudio(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)));
        while (_audioPlayed)
        {
            yield return new WaitForSeconds(Random.Range(5f, 15f)); 
            _audioManager.PlayAudio(new Vector3(Random.Range(-6f, 6f), Random.Range(-6f, 6f), Random.Range(-6f, 6f)));
        }
        yield return null;
	}

    public void RemoveStart()
	{

        _startGO.SetActive(false);
		
	}
}
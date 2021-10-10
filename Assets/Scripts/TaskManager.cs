using UnityEngine;

public class TaskManager : MonoBehaviour
{
	int _currentTask;

	
    void Start()
	{
		_currentTask = 0;
	}

	public int AnotherSliderDown()
	{
		if(_currentTask < 6 )	
			return ++_currentTask;

		return 0;
	}

	
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskCheck : MonoBehaviour
{
    [SerializeField] private Tasks[] _tasks;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private bool IsBigger(float value1, float value2)
	{
        return value1 > value2;
	}
    private bool IsBiggerOrEqual(float value1, float value2)
    {
        return value1 >= value2;
    }
    private bool IsSmaller(float value1, float value2)
    {
        return value1 < value2;
    }
    private bool IsSmallerOrEqual(float value1, float value2)
    {
        return value1 <= value2;
    }
    private bool IsVector3Close(Vector3 value1, Vector3 value2, float accuracy)
    {
        return Vector3.Distance(value1,value2)<accuracy;
    }
    private bool IsVector2Close(Vector2 value1, Vector2 value2, float accuracy)
    {
        return Vector2.Distance(value1, value2) < accuracy;
    }
}

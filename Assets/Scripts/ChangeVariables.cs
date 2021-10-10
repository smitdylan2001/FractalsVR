using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeVariables : MonoBehaviour
{
    private FractalPP _fractalScript;

    void Start()
    {
        _fractalScript = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FractalPP>();
    }
}

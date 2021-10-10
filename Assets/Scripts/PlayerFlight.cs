using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class PlayerFlight : MonoBehaviour
{
    [SerializeField] GameObject _rightHand;
    [SerializeField] GameObject _player;
    [SerializeField] float _speed = 0.01f;
    void Update()
    {
        if(InputBridge.Instance.RightTrigger > 0.1)
		{
            _player.transform.position = Vector3.MoveTowards(_player.transform.position, _player.transform.position + _rightHand.transform.forward, _speed * InputBridge.Instance.RightTrigger * Time.deltaTime * 40);
		}
    }
}

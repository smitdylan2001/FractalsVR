using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BunBun : MonoBehaviour
{

    [SerializeField] private Transform _playerTransform;
    [SerializeField] private GameObject _bunBun;
    [SerializeField] private Animator _bunAnim;
    [SerializeField] private FractalPP fractalPP;

    public bool bunrunOver = false;

    private int bunrun = 0;

    private void Update()
    {
        if(!fractalPP._bunnyTime) _bunBun.SetActive(false);

     //   Debug.Log(fractalPP._bunnyTime);

    }

    public void SpawnBunBun()
    {

        if (!_bunBun.activeSelf) _bunBun.SetActive(true);
        if (bunrun <= 1)
        {
            _bunBun.transform.position = _playerTransform.position + _playerTransform.forward  * 10.0f;
        }
               
        else if(bunrun < 600 && bunrun > 1)
        {
            _bunBun.transform.position += (_playerTransform.position - _bunBun.transform.position).normalized * 0.01f;

            _bunBun.transform.LookAt(_playerTransform);
        }

        else if (bunrun >= 3000)
        {

            //fractalPP._bunnyTime = false;

            //fractalPP._CurrentSwitch = 0;

            _bunBun.SetActive(false);
            Scene thisScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(thisScene.name);
            
        }

        else if (bunrun >= 600)
        {
            _bunAnim.SetBool("IDLE", true);
        }      

        bunrun++;
    }
}

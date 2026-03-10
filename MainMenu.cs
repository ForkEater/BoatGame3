using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void SinglePlayer()
    {
        return;
        //SceneManager.LoadScene("SinglePlayer");
    }
    public void MPLocal()
    {
        SceneManager.LoadScene("BAREMP");
    }
    public void MPOnline()
    {
        return;
       // SceneManager.LoadScene("MPOnline");
    }  
}

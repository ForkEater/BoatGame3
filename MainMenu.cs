using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void SinglePlayer()
    {
        SceneManager.LoadScene("Singleplayer");
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

using UnityEngine;
using UnityEngine.SceneManagement;

public class ButonScripts : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
   public void Exit()
    {
        Application.Quit();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void OnStartButton()
    {
        SceneManager.LoadScene("DevScene");
    }
}

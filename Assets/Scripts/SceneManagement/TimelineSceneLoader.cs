using UnityEngine;
using UnityEngine.SceneManagement;

public class TimelineSceneLoader : MonoBehaviour
{
    public void LoadNextScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

using UnityEngine;

public class TimelineFaderBridge : MonoBehaviour
{
    public void FadeOut(float seconds = 0.5f)
    {
        var f = RPG.SceneManagement.Fader.Instance;
        if (f != null) f.FadeOut(seconds);
    }
    public void FadeIn(float seconds = 0.5f)
    {
        var f = RPG.SceneManagement.Fader.Instance;
        if (f != null) f.FadeIn(seconds);
    }
    public void FadeOutImmediate()
    {
        var f = RPG.SceneManagement.Fader.Instance;
        if (f != null) f.FadeOutImmediate();
    }

    public void FadeOut05() { FadeOut(0.5f); }
    public void FadeIn05() { FadeIn(0.5f); }
}

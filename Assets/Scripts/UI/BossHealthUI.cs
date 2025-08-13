using UnityEngine;
using RPG.Attributes;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] HealthBar bossBarPrefab;
    HealthBar bar;

    void Start()
    {
        var hud = FindObjectOfType<Canvas>();
        bar = Instantiate(bossBarPrefab, hud.transform);   // �������� ����, �ʵ忡 ����

        var bossHP = GetComponent<Health>();
        bar.SetHealth(bossHP);

        // CanvasGroup ����
        var cg = bar.GetComponent<CanvasGroup>();
        if (cg == null) cg = bar.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0;
    }

    public void ShowBar(float _)
    {
        if (!bar) return;
        var cg = bar.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 1;
    }

    public void HideBar()
    {
        if (!bar) return;
        Destroy(bar.gameObject);
    }
}

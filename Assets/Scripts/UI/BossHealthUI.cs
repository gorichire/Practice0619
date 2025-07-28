using UnityEngine;
using RPG.Attributes;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] HealthBar bossBarPrefab;
    HealthBar bar;                       // 인스턴스 보관용

    void Start()
    {
        var hud = FindObjectOfType<Canvas>();
        var bar = Instantiate(bossBarPrefab, hud.transform);

        // ① 보스 HP 연결
        var bossHP = GetComponent<Health>();
        bar.SetHealth(bossHP);

        // ② 처음엔 숨기기
        if (bar.TryGetComponent(out CanvasGroup cg)) cg.alpha = 0;
    }


    public void ShowBar(float _)        // TakeDamage(float)용
    {
        bar.GetComponent<CanvasGroup>().alpha = 1;
    }

    public void HideBar()               // OnDie()용
    {
        Destroy(bar.gameObject);
    }
}

using UnityEngine;
using RPG.Attributes;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] HealthBar bossBarPrefab;
    HealthBar bar;                       // �ν��Ͻ� ������

    void Start()
    {
        var hud = FindObjectOfType<Canvas>();
        var bar = Instantiate(bossBarPrefab, hud.transform);

        // �� ���� HP ����
        var bossHP = GetComponent<Health>();
        bar.SetHealth(bossHP);

        // �� ó���� �����
        if (bar.TryGetComponent(out CanvasGroup cg)) cg.alpha = 0;
    }


    public void ShowBar(float _)        // TakeDamage(float)��
    {
        bar.GetComponent<CanvasGroup>().alpha = 1;
    }

    public void HideBar()               // OnDie()��
    {
        Destroy(bar.gameObject);
    }
}

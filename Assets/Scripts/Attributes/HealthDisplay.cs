using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Attributes
{
    public class HealthDisplay : MonoBehaviour
    {
        [SerializeField] Image healthFillImage = null;
        Health health;

        private void Awake()
        {
            health = GameObject.FindWithTag("Player").GetComponent<Health>();
        }

        private void Update()
        {
            healthFillImage.fillAmount = health.GetFraction();
            //GetComponent<Text>().text = String.Format("{0:0}/{1:0}", health.GetHealthPoints(), health.GetMaxHealthPoints());
        }
    }
}
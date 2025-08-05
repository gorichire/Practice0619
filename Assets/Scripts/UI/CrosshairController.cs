using UnityEngine;
using UnityEngine.UI;

namespace RPG.UI
{
    public class CrosshairController : MonoBehaviour
    {
        [SerializeField] Image crosshair;
        [SerializeField] float zoomScale = 0.6f;
        [SerializeField] float lerpTime = 0.1f;

        Vector3 normalScale;
        Vector3 aimScale; 
        bool isAiming;

        void Awake()
        {
            if (!crosshair) Debug.LogError("Crosshair Image not assigned!");
            normalScale = crosshair.rectTransform.localScale;
            aimScale = normalScale * zoomScale;
            crosshair.enabled = false; 
        }

        public void SetAiming(bool aiming)
        {
            if (isAiming == aiming) return;
            isAiming = aiming;
            crosshair.enabled = aiming;  
            StopAllCoroutines();
            if (aiming) StartCoroutine(LerpScale(aimScale));
            else StartCoroutine(LerpScale(normalScale));
        }

        System.Collections.IEnumerator LerpScale(Vector3 target)
        {
            RectTransform rt = crosshair.rectTransform;
            Vector3 start = rt.localScale;
            float t = 0;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / lerpTime;
                rt.localScale = Vector3.Lerp(start, target, t);
                yield return null;
            }
            rt.localScale = target;
        }
    }

}
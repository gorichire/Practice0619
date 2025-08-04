using UnityEngine;
using Cinemachine;
using RPG.Combat;
using RPG.Attributes;
using RPG.Core;

public class BowHandler : MonoBehaviour, IAction
{
    [Header("Camera")]
    [SerializeField] CinemachineVirtualCamera vCam;  
    [SerializeField] float aimFOV = 35f;     
    [SerializeField] float normalFOV = 60f;      
    [SerializeField] float zoomTime = 0.12f;     

    [Header("Bow")]
    [SerializeField] string bowTag = "Bow";
    [SerializeField] Transform arrowSpawn;

    [SerializeField] LayerMask aimMask = ~0;   // 맞춰야 할 레이어(Env, Enemy 등)
    [SerializeField] float minAimDistance = 1.0f;   // 너무 가까운 히트 무시
    [SerializeField] bool ignoreSelf = true;

    Fighter fighter;
    Animator anim;
    ActionSchduler scheduler;
    bool isHolding;
    float fovVel;
    bool fullyDrawn;
    Coroutine fovRoutine;
    public bool IsAiming => isHolding;

    void Awake()
    {
        fighter = GetComponent<Fighter>();
        anim = GetComponent<Animator>();
        scheduler = GetComponent<ActionSchduler>();
    }

    public void Tick()
    {
        Weapon w = fighter.GetCurrentWeapon();
        bool holdingBow = w && w.HasTag(bowTag);
        if (!holdingBow) { CancelHold(); return; }

        if (Input.GetMouseButtonDown(0)) BeginHold();
        if (isHolding && !Input.GetMouseButton(0))
        {
            if (fullyDrawn) Fire(); 
            else CancelEarly();
        }
    }
    void BeginHold()
    {
        if (isHolding) return;
        scheduler.StartAction(this);      
        isHolding = true;
        fullyDrawn = false;

        anim.ResetTrigger("bowShoot");
        anim.ResetTrigger("bowCancel");

        anim.SetTrigger("bowDraw");
        StartFOV(aimFOV);
    }

    void CancelEarly()                  
    {
        anim.SetTrigger("bowCancel");    
        CancelHold();
    }
    void Fire()
    {
        if (!fullyDrawn) return;
        anim.ResetTrigger("bowDraw");
        anim.ResetTrigger("bowCancel");
        anim.SetBool("bowHold", false);
        anim.SetTrigger("bowShoot");
        CancelHold();                      

        // 투사체 정보
        WeaponConfig cfg = fighter.GetCurrentWeaponConfig();
        if (cfg == null || !cfg.HasProjectile()) return;

        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(new Vector2(cam.pixelWidth * .5f,
                                                      cam.pixelHeight * .5f));

        RaycastHit hit;
        Vector3 hitPos;

        bool hasHit = Physics.Raycast(ray, out hit, 100f, aimMask,
                                      QueryTriggerInteraction.Ignore);

        if (hasHit)
        {
            if (ignoreSelf && hit.transform.root == transform.root) hasHit = false;

            Vector3 camToSpawn = arrowSpawn.position - cam.transform.position;
            Vector3 camToHit = hit.point - cam.transform.position;

            if (Vector3.Dot(camToHit, camToSpawn) < 0f ||
                Vector3.Distance(arrowSpawn.position, hit.point) < minAimDistance)
            {
                hasHit = false;
            }
        }

        hitPos = hasHit ? hit.point : ray.GetPoint(100f);

        Quaternion rot = Quaternion.LookRotation(hitPos - arrowSpawn.position);

        Projectile p = Instantiate(cfg.GetProjectilePrefab(),
                                   arrowSpawn.position, rot);
        p.SetTarget(null, gameObject, fighter.CalculateAttackDamage());
    }

    /* ---------------- 조준 해제 ---------------- */
    void CancelHold()
    {
        if (!isHolding) return;
        isHolding = false;
        fullyDrawn = false;

        anim.SetBool("bowHold", false);
        StartFOV(normalFOV);
    }
    public void OnDrawCheckpoint()
    {
        if (Input.GetMouseButton(0))
        {
            anim.SetBool("bowHold", true);
            fullyDrawn = true;
        }
        else
        { 
            anim.SetTrigger("bowCancel");
            CancelHold();
        }
    }

    public void Cancel() => CancelHold();

    System.Collections.IEnumerator LerpFOV(float target)
    {
        while (Mathf.Abs(vCam.m_Lens.FieldOfView - target) > 0.1f)
        {
            float fov = Mathf.SmoothDamp(vCam.m_Lens.FieldOfView, target,
                                         ref fovVel, zoomTime);
            vCam.m_Lens.FieldOfView = fov;
            yield return null;
        }
        vCam.m_Lens.FieldOfView = target;
    }

    void StartFOV(float target)
    {
        if (fovRoutine != null) StopCoroutine(fovRoutine);
        fovRoutine = StartCoroutine(LerpFOV(target));
    }
}

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyDestroyer : LightUtility
{
    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waypointTolerance = 0.08f;

    [Header("Mirror Breaking")]
    [SerializeField] private float aggroSpeed = 4f;
    [SerializeField] private float mirrorSearchRadius = 20f;
    [SerializeField] private float breakDistance = 0.25f;
    [SerializeField] private bool destroyMirrorGameObject = true;
    [SerializeField] private SoftLockManager softLockManager;

    private readonly List<LightRayData> hit = new List<LightRayData>();
    private Rigidbody2D rb;
    private int patrolIndex;
    private PushableMirror targetMirror;
    private bool aggro;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public override void OnLightGraphClear()
    {
        hit.Clear();
    }

    public override void OnLightHit(LightRayData lightRayData)
    {
        UpsertHit(lightRayData);
        EnterAggro();
    }

    private void FixedUpdate()
    {
        if (aggro)
        {
            MoveToMirror();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Transform target = patrolPoints[patrolIndex];
        if (target == null)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            return;
        }

        Vector2 delta = (Vector2)(target.position - transform.position);
        if (delta.magnitude <= waypointTolerance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = delta.normalized * patrolSpeed;
    }

    private void EnterAggro()
    {
        if (aggro)
        {
            return;
        }

        targetMirror = FindNearestMirror();
        aggro = targetMirror != null;
    }

    private void MoveToMirror()
    {
        if (targetMirror == null)
        {
            rb.linearVelocity = Vector2.zero;
            NotifySoftLock();
            return;
        }

        Vector2 delta = (Vector2)(targetMirror.transform.position - transform.position);
        if (delta.magnitude <= breakDistance)
        {
            BreakMirror();
            return;
        }

        rb.linearVelocity = delta.normalized * aggroSpeed;
    }

    private PushableMirror FindNearestMirror()
    {
        PushableMirror[] mirrors = FindObjectsByType<PushableMirror>();
        PushableMirror nearest = null;
        float nearestDistance = mirrorSearchRadius;

        for (int i = 0; i < mirrors.Length; i++)
        {
            if (mirrors[i] == null) continue;

            float distance = Vector2.Distance(transform.position, mirrors[i].transform.position);
            if (distance <= nearestDistance)
            {
                nearest = mirrors[i];
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    private void BreakMirror()
    {
        rb.linearVelocity = Vector2.zero;

        if (targetMirror != null)
        {
            if (destroyMirrorGameObject)
            {
                Destroy(targetMirror.gameObject);
            }
            else
            {
                targetMirror.gameObject.SetActive(false);
            }
        }

        NotifySoftLock();
    }

    private void NotifySoftLock()
    {
        SoftLockManager manager = softLockManager != null ? softLockManager : SoftLockManager.Instance;
        if (manager != null)
        {
            manager.SetSoftLocked(true);
        }
    }

    public void ConfigurePatrolPoints(Transform[] points)
    {
        patrolPoints = points;
    }

    private void UpsertHit(LightRayData lightRayData)
    {
        for (int i = 0; i < hit.Count; i++)
        {
            if (hit[i].emitObject == lightRayData.emitObject && hit[i].emitpos == lightRayData.emitpos && hit[i].raydir == lightRayData.raydir)
            {
                hit[i] = lightRayData;
                return;
            }
        }

        hit.Add(lightRayData);
    }
}

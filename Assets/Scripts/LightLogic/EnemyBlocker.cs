using System.Collections.Generic;
using UnityEngine;

public class EnemyBlocker : LightUtility
{
    [SerializeField] private bool requireSpecificColor = true;
    [SerializeField] private LightColorChannel requiredColor = LightColorChannel.Red;
    [SerializeField] private bool requireExactColor = true;
    [SerializeField] private float luminosityToDestroy = 5f;
    [SerializeField] private float destroyHoldTime = 0.1f;
    [SerializeField] private bool destroyGameObject = true;
    [SerializeField] private GameObject clearedVisual;

    private readonly List<LightRayData> hit = new List<LightRayData>();
    private float destroyTimer;
    private bool cleared;

    public override void OnLightGraphClear()
    {
        hit.Clear();
    }

    public override void OnLightHit(LightRayData lightRayData)
    {
        UpsertHit(lightRayData);
    }

    private void Update()
    {
        if (cleared)
        {
            return;
        }

        bool shouldClear = GetMatchingLuminosity() >= luminosityToDestroy;
        if (shouldClear)
        {
            destroyTimer += Time.deltaTime;
        }
        else
        {
            destroyTimer = 0f;
        }

        if (destroyTimer >= destroyHoldTime)
        {
            ClearEnemy();
        }
    }

    public float GetMatchingLuminosity()
    {
        float sum = 0f;
        for (int i = 0; i < hit.Count; i++)
        {
            if (!requireSpecificColor || LightColorHelper.Matches(hit[i].lightColor, requiredColor, requireExactColor))
            {
                sum += hit[i].lightluminosity;
            }
        }

        return sum;
    }

    public void ConfigureRequirement(bool requireColor, LightColorChannel color, bool exactColor, float requiredLuminosity)
    {
        requireSpecificColor = requireColor;
        requiredColor = color;
        requireExactColor = exactColor;
        luminosityToDestroy = Mathf.Max(0f, requiredLuminosity);
    }

    private void ClearEnemy()
    {
        cleared = true;

        if (clearedVisual != null)
        {
            clearedVisual.SetActive(true);
        }

        if (destroyGameObject)
        {
            Destroy(gameObject);
            return;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Color color = renderers[i].color;
            color.a = 0.25f;
            renderers[i].color = color;
        }
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

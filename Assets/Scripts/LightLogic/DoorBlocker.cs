using UnityEngine;

public class DoorBlocker : MonoBehaviour
{
    [SerializeField] private bool startsOpen = false;
    [SerializeField] private bool disableCollidersWhenOpen = true;
    [SerializeField] private bool hideRenderersWhenOpen = false;
    [SerializeField] private Color closedColor = Color.white;
    [SerializeField] private Color openColor = new Color(1f, 1f, 1f, 0.25f);

    private Collider2D[] colliders;
    private SpriteRenderer[] spriteRenderers;
    private bool isOpen;

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider2D>(true);
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        SetOpen(startsOpen);
    }

    public void SetOpen(bool open)
    {
        isOpen = open;

        if (colliders != null && disableCollidersWhenOpen)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = !isOpen;
                }
            }
        }

        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] == null) continue;

                spriteRenderers[i].enabled = !hideRenderersWhenOpen || !isOpen;
                spriteRenderers[i].color = isOpen ? openColor : closedColor;
            }
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}

using UnityEngine;

public class MirrorPushZone : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer faceRenderer;
    [SerializeField] private Color highlightColor = Color.yellow;

    private Color normalColor;
    private PushableMirror parentMirror;
    private bool isHighlighted;

    private void Awake()
    {
        parentMirror = GetComponentInParent<PushableMirror>();
        if (faceRenderer != null)
        {
            normalColor = faceRenderer.color;
        }
    }

    private void OnDisable()
    {
        Highlight(false);
    }

    public PushableMirror GetMirror()
    {
        return parentMirror;
    }

    public void Highlight(bool shouldHighlight)
    {
        if (isHighlighted == shouldHighlight)
        {
            return;
        }

        isHighlighted = shouldHighlight;

        if (faceRenderer != null)
        {
            faceRenderer.color = isHighlighted ? highlightColor : normalColor;
        }

        if (parentMirror != null)
        {
            parentMirror.SetSignZoneActive(this, isHighlighted);
        }
    }
}

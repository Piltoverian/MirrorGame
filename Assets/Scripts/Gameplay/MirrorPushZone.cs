using UnityEngine;

public class MirrorPushZone : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer faceRenderer;
    [SerializeField] private Color highlightColor = Color.yellow;

    private Color normalColor;
    private PushableMirror parentMirror;

    private void Awake()
    {
        parentMirror = GetComponentInParent<PushableMirror>();

        if (faceRenderer != null)
        {
            normalColor = faceRenderer.color;
        }
    }

    public PushableMirror GetMirror()
    {
        return parentMirror;
    }

    public void Highlight(bool isHighlighted)
    {
        if (faceRenderer == null) return;

        faceRenderer.color = isHighlighted ? highlightColor : normalColor;
    }
}
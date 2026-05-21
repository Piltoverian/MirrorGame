using UnityEngine;

public class MirrorPushZone : MonoBehaviour
{
    public Vector2 pushDirection;
    public SpriteRenderer faceRenderer;

    private Color normalColor = new Color(1f, 1f, 1f, 0f); 
    public Color highlightColor = Color.yellow; 

    private PushableMirror parentMirror;

    private void Awake()
    {
        parentMirror = GetComponentInParent<PushableMirror>();
        if (faceRenderer != null) normalColor = faceRenderer.color;
    }

    public void Highlight(bool isHighlighted)
    {
        if (faceRenderer != null)
        {
            faceRenderer.color = isHighlighted ? highlightColor : normalColor;
        }
    }

    public PushableMirror GetMirror()
    {
        return parentMirror;
    }
    public Vector3 GetCalculatedPushDirection()
    {
        if (pushDirection != Vector2.zero) 
            return new Vector3(pushDirection.x, pushDirection.y, 0f);

        Vector3 dir = parentMirror.transform.position - transform.position;
        return dir.normalized;
    }
}

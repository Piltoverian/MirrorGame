using UnityEngine;

public class Confiner : MonoBehaviour
{
    [SerializeField] private Collider2D confiningCollider;
    private Camera camComponent;

    void Start()
    {
        camComponent = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        var size = camComponent.orthographicSize;

        if (confiningCollider != null)
        {
            var bounds = confiningCollider.bounds;
            float minX = bounds.min.x + size * camComponent.aspect;
            float maxX = bounds.max.x - size * camComponent.aspect;
            float minY = bounds.min.y + size;
            float maxY = bounds.max.y - size;
            Vector3 newPosition = transform.position;
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            transform.position = newPosition;
        }
    }
}

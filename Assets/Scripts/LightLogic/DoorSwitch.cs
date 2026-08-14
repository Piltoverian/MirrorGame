using UnityEngine;

[RequireComponent(typeof(LightReceiver))]
public class DoorSwitch : MonoBehaviour
{
    [SerializeField] private DoorBlocker[] linkedDoors;
    [SerializeField] private bool invertState = false;
    [SerializeField] private bool holdLastOpenState = false;

    private LightReceiver receiver;
    private bool hasOpened;

    private void Awake()
    {
        receiver = GetComponent<LightReceiver>();
    }

    private void LateUpdate()
    {
        if (receiver == null)
        {
            return;
        }

        bool shouldOpen = receiver.IsOpened();
        if (invertState)
        {
            shouldOpen = !shouldOpen;
        }

        if (holdLastOpenState && hasOpened)
        {
            shouldOpen = true;
        }

        if (shouldOpen)
        {
            hasOpened = true;
        }

        ApplyDoorState(shouldOpen);
    }

    public void ApplyDoorState(bool open)
    {
        if (linkedDoors == null)
        {
            return;
        }

        for (int i = 0; i < linkedDoors.Length; i++)
        {
            if (linkedDoors[i] != null)
            {
                linkedDoors[i].SetOpen(open);
            }
        }
    }

    public void ConfigureLinkedDoors(DoorBlocker[] doors)
    {
        linkedDoors = doors;
    }
}

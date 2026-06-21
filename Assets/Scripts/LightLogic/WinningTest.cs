using UnityEngine;

public class WinningTest : MonoBehaviour
{
    private void LateUpdate()
    {
        var doors = FindObjectsByType<LightReceiver>(FindObjectsSortMode.None);
        if (doors.Length > 0)
        {
            foreach (var door in doors)
            {
                if (!door.IsOpened())
                {
                    return;
                }
            }
            Debug.Log("You win!");
        }
    }
}

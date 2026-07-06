using System.Collections.Generic;
using UnityEngine;

public class PageListHolder : MonoBehaviour
{
    [SerializeField] List<GameObject> pageLists;
    [SerializeField]private int currentPageIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (pageLists.Count > 0)
        {
            for (int i = 0; i < pageLists.Count; i++)
            {
                pageLists[i].SetActive(i == currentPageIndex);
            }
        }
    }

    public void GoBack()
    {
        if (currentPageIndex == 0) { return; }
        pageLists[currentPageIndex].SetActive(false);
        pageLists[currentPageIndex - 1].SetActive(true);
        currentPageIndex--;
    }

    public void GoNext()
    {
        Debug.Log(pageLists.Count);
        if (currentPageIndex == pageLists.Count - 1) { return; }
        pageLists[currentPageIndex].SetActive(false);
        pageLists[currentPageIndex + 1].SetActive(true);
        currentPageIndex++;
    }
}

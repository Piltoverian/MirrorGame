using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PageChangeButton : MonoBehaviour
{
    [SerializeField] private bool BackButton = false;
    [SerializeField] GameObject pageListHolder;
    
    public void OnClick()
    {
        if (BackButton)
        {
           pageListHolder.GetComponent<PageListHolder>().GoBack();
        }
        else
        {
           pageListHolder.GetComponent<PageListHolder>().GoNext();
        }
    }
}
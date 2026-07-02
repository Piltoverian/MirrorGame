using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeandPauseManagement : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    private bool isPaused = false;
    [SerializeField] private InputAction PauseInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PauseInput.Enable();
        Time.timeScale = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseInput.IsPressed())
        {
            Debug.Log("Pause button pressed");
            if (!isPaused)
            {
                Time.timeScale = 0.0f;
                isPaused = true;
                pauseMenu.SetActive(true);
            }
            else
            {
                Time.timeScale = 1.0f;
                isPaused = false;
                pauseMenu.SetActive(false);
            }
        }
    }
}

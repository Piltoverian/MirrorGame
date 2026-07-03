using UnityEngine;

public class ComboSound : MonoBehaviour
{
    private AudioClip[] notes;

    private int comboIndex;

    private void Start()
    {
        notes = new AudioClip[]
        {
            SoundManager.Instance.shine,
            SoundManager.Instance.shine_C,
            SoundManager.Instance.shine_DE,
            SoundManager.Instance.shine_E,
            SoundManager.Instance.shine_F,
            SoundManager.Instance.shine_FG,
            SoundManager.Instance.shine_G
        };
    }

    public void PlayCorrect()
    {
        SoundManager.Instance.PlaySFX(notes[comboIndex]);

        comboIndex++;

        if (comboIndex >= notes.Length)
            comboIndex = notes.Length - 1;
    }

    public void ResetCombo()
    {
        comboIndex = 0;
    }
}
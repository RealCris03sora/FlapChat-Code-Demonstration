using UnityEngine;
using System.Collections;

public class CharacterMusicManager : MonoBehaviour
{
    public AudioClip[] characterSongs;
    public AudioClip[] intensitySongs;
    public AudioSource audioSourceN;
    public AudioSource audioSourceI;

    public float startingVolume;
    public float duration;

    private int n;
    private bool canChange;
    private float startTime;

    void Start()
    {
        canChange = true;
        audioSourceN.clip = characterSongs[CharacterSelection.character];
        audioSourceI.clip = intensitySongs[CharacterSelection.character];
        audioSourceN.volume = startingVolume;
        audioSourceI.volume = 0.0f;
        audioSourceN.Play(); 
        audioSourceI.Play() ;
    }

    void Update()
    {
        if (KillCounter.waveNumber == 2 && canChange == true)
        {
            StartCoroutine(Swapper());
        }
    }

    IEnumerator Swapper()
    {
        startTime = Time.time;
        canChange = false;
        while (audioSourceN.volume > 0)
        {
            float elapsedTime = Time.time - startTime;
            float volumeN = Mathf.Lerp(startingVolume, 0.0f, elapsedTime / duration);
            audioSourceN.volume = volumeN;
            float volumeI = Mathf.Lerp(0.0f, startingVolume, elapsedTime / duration);
            audioSourceI.volume = volumeI;

            yield return null;
        }

        audioSourceI.volume = startingVolume;
        audioSourceN.volume = 0.0f;
    }
}

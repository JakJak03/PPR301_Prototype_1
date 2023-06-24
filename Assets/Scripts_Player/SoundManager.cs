using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundBite
{
    public string name;
    public AudioClip soundClip;
    [Range(0f, 1f)] public float volume = 1;
    [Range(0.1f, 3f)] public float pitch = 1;
    [Range(0f, 1f)] public float randomPitchAmount;
}

public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundBite[] soundBites;

    private Dictionary<string, SoundBite> soundBitesDict = new Dictionary<string, SoundBite>();

    public static SoundManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);

        soundBitesDict.Clear();
        for (int i = 0; i < soundBites.Length; i++)
            soundBitesDict.Add(soundBites[i].name, soundBites[i]);
    }

    public static void PlaySound(string receivedName)
    {
        if (instance.soundBitesDict.ContainsKey(receivedName))
            instance.StartCoroutine(instance.PlaySoundCoroutine(receivedName));
        else
            print("Could not find Sound Bite of name: " + receivedName);
    }
    public static void PlaySound(int minRange, int maxRange)
    {
        if (minRange >= 0 && maxRange < SoundManager.instance.soundBites.Length)
            instance.StartCoroutine(instance.PlaySoundCoroutine(Random.Range(minRange, maxRange + 1)));
        else
            print("Out of range Random Sound index: " + minRange + ", " + maxRange);
    }

    public IEnumerator PlaySoundCoroutine(string clipName)
    {
        soundBitesDict.TryGetValue(clipName, out SoundBite currentBite);
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = currentBite.soundClip;
        source.volume = currentBite.volume;
        source.pitch = currentBite.pitch + Random.Range(-currentBite.randomPitchAmount, currentBite.randomPitchAmount);
        source.Play();
        yield return new WaitWhile(() => source.isPlaying);
        Destroy(source);
    }
    public IEnumerator PlaySoundCoroutine(int index)
    {
        SoundBite currentBite = soundBites[index];
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = currentBite.soundClip;
        source.volume = currentBite.volume;
        source.pitch = currentBite.pitch + Random.Range(-currentBite.randomPitchAmount, currentBite.randomPitchAmount);
        source.Play();
        yield return new WaitWhile(() => source.isPlaying);
        Destroy(source);
    }
}

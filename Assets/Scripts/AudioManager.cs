using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] bgm;
    public AudioSource[] BGM { get { return bgm; } }

    [SerializeField] private AudioSource[] sfx;
    public AudioSource[] SFX { get { return sfx; } }

    [SerializeField] private AudioMixer audioMixer;

    public static AudioManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PlayBGM(0);
        DontDestroyOnLoad(gameObject);
    }

    private void StopAllBGM()
    {
        for (int i = 0; i < bgm.Length; i++)
        {
            bgm[i].Stop();
        }
    }

    public void PlayBGM(int i)
    {
        if (!BGM[i].isPlaying)
        {
            StopAllBGM();
            if (i < BGM.Length)
            {
                BGM[i].PlayDelayed(2f);
            }
        }
    }

    public void PlaySFX(int i)
    {
        if(i < sfx.Length && !sfx[i].isPlaying)
        {
            sfx[i].Play();
        }
    }

}

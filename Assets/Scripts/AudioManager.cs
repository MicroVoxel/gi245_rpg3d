using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // สร้าง Singleton Pattern ที่รัดกุมขึ้น ป้องกันการเปลี่ยนแปลงจากภายนอก
    public static AudioManager instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource[] bgm;
    public AudioSource[] BGM => bgm;

    [SerializeField] private AudioSource[] sfx;
    public AudioSource[] SFX => sfx;

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private void Awake()
    {
        // ตรวจสอบและจัดการ Singleton ให้ถูกต้อง
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // หากมี AudioManager อยู่แล้วใน Scene ให้ทำลายตัวที่เกิดใหม่ทิ้ง
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBGM(0);
    }

    private void StopAllBGM()
    {
        // ใช้ foreach แทน for loop ทำให้โค้ดอ่านง่ายขึ้น
        foreach (var bgmSource in bgm)
        {
            bgmSource.Stop();
        }
    }

    public void PlayBGM(int index)
    {
        // Guard Clause: ตรวจสอบไม่ให้ Index เกินขอบเขตของ Array
        if (index < 0 || index >= bgm.Length) return;

        if (!bgm[index].isPlaying)
        {
            StopAllBGM();
            bgm[index].PlayDelayed(2f);
        }
    }

    /// <summary>
    /// เล่น SFX แบบปกติ (ถ้าเรียกซ้ำในขณะที่เสียงเดิมยังไม่จบ เสียงจะไม่เล่นซ้ำจนกว่าจะจบ)
    /// </summary>
    public void PlaySFX(int index)
    {
        if (index < 0 || index >= sfx.Length) return;

        if (!sfx[index].isPlaying)
        {
            sfx[index].Play();
        }
    }

    /// <summary>
    /// เล่น SFX แบบ OneShot (สามารถเล่นทับซ้อนกันได้ เหมาะสำหรับเสียง Action เช่น เสียงยิงปืน หรือเก็บของ)
    /// </summary>
    public void PlaySFXOneShot(int index)
    {
        if (index < 0 || index >= sfx.Length) return;

        // ดึง AudioClip จาก AudioSource มาเล่นแบบ OneShot
        if (sfx[index].clip != null)
        {
            sfx[index].PlayOneShot(sfx[index].clip);
        }
        else
        {
            Debug.LogWarning($"AudioManager: ไม่มี AudioClip ใน SFX index {index}");
        }
    }
}
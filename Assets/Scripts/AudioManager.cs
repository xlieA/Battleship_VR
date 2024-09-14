using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private AudioSource bgmScene;
    private AudioSource sfxButton;
    private AudioSource sfxHit;
    private AudioSource sfxMiss;

    public AudioMixer mixer;

    // Music Volume
    public float volumeMaster = 0.7f;
    public float volumeMusic = 0.7f;
    public float volumeSfx = 0.7f;

    private static AudioManager _instance = null;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("AudioManager").GetComponent<AudioManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        AudioSource[] audioSources = GetComponents<AudioSource>();
        bgmScene = audioSources[0];
        sfxButton = audioSources[1];
        sfxHit = audioSources[2];
        sfxMiss = audioSources[3];

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        if (bgmScene != null)
        {
            bgmScene.Stop();
        }

        bgmScene.loop = true;
        bgmScene.Play();
    }

    private void Start()
    {
        // Set volume
        StartCoroutine(SetVolume());
    }

    private IEnumerator SetVolume()
    {
        yield return new WaitForEndOfFrame();
        mixer.SetFloat("MasterVol", Mathf.Log10(volumeMaster + float.Epsilon) * 40);
        mixer.SetFloat("MusicVol", Mathf.Log10(volumeMusic + float.Epsilon) * 40);
        mixer.SetFloat("SfxVol", Mathf.Log10(volumeSfx + float.Epsilon) * 40);
    }

    public void Button()
    {
        sfxButton.Play();
    }

    public void Hit()
    {
        sfxHit.Play();
    }

    public void Miss()
    {
        sfxMiss.Play();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] clips;

    [SerializeField]
    public AudioClip[] fx_clips;

    [SerializeField]
    public AudioClip[] fx_clips_player;

    [SerializeField]
    private AudioClip[] bgm_clips;

    [SerializeField] private AudioSource _as1;//MASTER
    [SerializeField] private AudioSource _as2;//SFX
    [SerializeField] private AudioSource _as3;//BGM

    [SerializeField]
    private AudioMixer _mixer;

    public void PlayAudio(int x)
    {
        _as1.clip = clips[x];
        _as1.Play();
    }

    public void StopAudio()
    {
        _as1.Stop();
    }

    public void PlaySFX(int x)
    {
        _as2.clip = fx_clips[x];
        _as2.Play();
    }

    public void PlaySFX_Player(int x)
    {
        _as2.clip = fx_clips_player[x];
        _as2.Play();
    }

    public void PlayBGM(int x)
    {
        _as3.clip = bgm_clips[x];
        _as3.Play();
    }
 
    private void Start()
    {
        //clips = Resources.LoadAll<AudioClip>("AudioFile");
    }

    /*
    public IEnumerator PlayAudioWithDelay()
    {
        while (clips == null)
        {
            yield return null;
        }

        do
        {
            PlayAudio(currentClipIndex);
            var cacheWait = new WaitForSeconds(clips[currentClipIndex].length + 1);
            yield return cacheWait;
            currentClipIndex++;
        } while (currentClipIndex < clips.Length);
    }*/
}

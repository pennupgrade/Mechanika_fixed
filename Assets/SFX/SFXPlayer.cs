using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{

    [Header(" -=- Audio -=- ")]
    [SerializeField] AudioSource AudioPlayer;
    [SerializeField] SerializedDictionary<string, AudioClip> Sounds;

    //
    static List<SFXPlayer> players = new();
    static SFXPlayer ins;

    private void Awake()
    {
        players.Add(this);
        if(ins != null) return;
        players.Clear();
        ins = this;
    }

    public static void PlaySound(AudioClip clip, float volume = 1f)
    {
        AudioSource player = players.FirstOrDefault(p => !p.AudioPlayer.isPlaying)?.AudioPlayer;
        if(player is null)
            player = Instantiate(ins).AudioPlayer;

        player.volume = volume;

        player.clip = clip;
        player.Play();
    }

    public static void PlaySound(string soundName, float volume = 1f)
        => PlaySound(ins.Sounds[soundName], volume);

}
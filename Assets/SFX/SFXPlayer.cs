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
        ins = this;
    }

    //add loop parameter, volume, and stuff like that, would probably force guarantee new audioplayer
    public static void PlaySound(AudioClip clip)
    {
        AudioSource player = players.FirstOrDefault(p => !p.AudioPlayer.isPlaying)?.AudioPlayer;
        if(player is null)
            player = Instantiate(ins).AudioPlayer;

        player.clip = clip;
        player.Play();
    }

    public static void PlaySound(string soundName)
        => PlaySound(ins.Sounds[soundName]);

}
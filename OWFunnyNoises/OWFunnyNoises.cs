using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OWFunnyNoises;

[HarmonyPatch]
public class OWFunnyNoises : ModBehaviour {
    public static OWFunnyNoises Instance { get; private set; }

    private void Awake() {}

    private void Start() {
        Instance = this;
        Instance.ModHelper.Console.WriteLine($"Loading game sounds", MessageType.Info);

        GetClips(new string[] {"Assets/fart_reverb.mp3", "Assets/fart_small.mp3"});

        LoadManager.OnCompleteSceneLoad += (scene, loadScene) => {
            StartCoroutine(PatchAudio());
        };
    }

    private IEnumerator PatchAudio() {
        yield return new WaitForSecondsRealtime(1);

        Dictionary<int, AudioLibrary.AudioEntry> dict = ((Dictionary<int, AudioLibrary.AudioEntry>)AccessTools.Field(typeof(AudioManager), "_audioLibraryDict").GetValue(Locator.GetAudioManager()));

        Instance.ModHelper.Console.WriteLine($"Patching game sounds", MessageType.Info);
        Instance.PatchAudioType(dict, AudioType.ShipCockpitEject, "Assets/fart_reverb.mp3");
        Instance.PatchAudioType(dict, AudioType.Sun_Explosion, "Assets/fart_reverb.mp3");

        Instance.PatchAudioType(dict, AudioType.ToolProbeLaunch, "Assets/fart_small.mp3");
        Instance.PatchAudioType(dict, AudioType.ToolProbeLaunchUnderwater, "Assets/fart_small.mp3");
        
        Instance.ModHelper.Console.WriteLine($"All sounds patched!", MessageType.Success);
    }

    public Dictionary<string, AudioClip> Sounds = new Dictionary<string, AudioClip>();
    private AudioClip GetClip(string name) {
        if (Instance.Sounds.ContainsKey(name)) { return Instance.Sounds[name]; }
        AudioClip audioClip = ModHelper.Assets.GetAudio(name);
        Instance.Sounds.Add(name, audioClip);
        return audioClip;
    }
    private AudioClip[] GetClips(string[] names) {
        AudioClip[] clips = new AudioClip[names.Length];
        for (int i = 0; i < names.Length; i++) {
            clips[i] = GetClip(names[i]);
        }
        return clips;
    }

    private void PatchAudioType(Dictionary<int, AudioLibrary.AudioEntry> dict, AudioType type, string[] names) {
        AudioLibrary.AudioEntry entry = new AudioLibrary.AudioEntry(type, GetClips(names), 0.5f);
        try {
            dict[(int)type] = entry;
        } catch {
            dict.Add((int)type, entry);
        }
    }
    private void PatchAudioType(Dictionary<int, AudioLibrary.AudioEntry> dict, AudioType type, string name) {
        Instance.PatchAudioType(dict, type, new string[] { name });
    }
}
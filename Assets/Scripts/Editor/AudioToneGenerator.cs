using UnityEngine;
using UnityEditor;
using System.IO;

public class AudioToneGenerator : EditorWindow
{
    private float frequency = 440f;
    private float duration = 0.1f;
    private string clipName = "NewSound";

    [MenuItem("Tools/Dungeon Scavenger/Generate Test Sounds")]
    public static void ShowWindow()
    {
        GetWindow<AudioToneGenerator>("Tone Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Test Audio", EditorStyles.boldLabel);

        clipName = EditorGUILayout.TextField("Name", clipName);
        frequency = EditorGUILayout.Slider("Frequency (Hz)", frequency, 100f, 2000f);
        duration = EditorGUILayout.Slider("Duration (s)", duration, 0.05f, 2f);

        if (GUILayout.Button("Generate Pickup Sound"))
        {
            GenerateTone(880f, 0.1f, "Pickup");
        }

        if (GUILayout.Button("Generate Damage Sound"))
        {
            GenerateTone(220f, 0.2f, "Damage");
        }

        if (GUILayout.Button("Generate UI Click"))
        {
            GenerateTone(660f, 0.05f, "UIClick");
        }

        if (GUILayout.Button("Generate Custom"))
        {
            GenerateTone(frequency, duration, clipName);
        }
    }

    private void GenerateTone(float freq, float dur, string name)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * dur);

        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);

        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t);

            // Apply fade out
            float fadeOut = 1f - (t / dur);
            data[i] *= fadeOut * fadeOut;
        }

        clip.SetData(data, 0);

        // Save to file
        string path = $"Assets/Audio/SFX/{name}.wav";
        SaveAudioClip(clip, path);

        Debug.Log($"Generated audio: {path}");
    }

    private void SaveAudioClip(AudioClip clip, string path)
    {
        // This requires additional library like NAudio for WAV export
        // For now, just log that it would save
        Debug.Log($"[AudioGenerator] Would save to: {path}");
    }
}
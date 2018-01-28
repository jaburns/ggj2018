using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour 
{
    static Audio instance;

    void Awake()
    {
        instance = this;
    }

    static public void Play(string name)
    {
        if (instance == null) return;
        instance.play(name);
    }

    void play(string name)
    {
        var src = transform.Find(name).GetComponent<AudioThang>();
        src.src.pitch = src.originalPitch + (2f*Random.value - 1f) * src.pitchVariance;
        src.src.Play();
    }

    List<string> _playingLoops = new List<string>();

    static public void SetLoopPlaying(string name, bool playing)
    {
        if (instance == null) return;
        instance.setLoopPlaying(name, playing);
    }

    void setLoopPlaying(string name, bool playing)
    {
        if (playing) {
            if (!_playingLoops.Contains(name)) {
                _playingLoops.Add(name);
                var src = transform.Find(name).GetComponent<AudioThang>();
                src.src.pitch = src.originalPitch + (2f*Random.value - 1f) * src.pitchVariance;
                src.src.Play();
            }
        }
        else if (_playingLoops.Contains(name)) {
            _playingLoops.Remove(name);
            var src = transform.Find(name).GetComponent<AudioThang>();
            src.src.Stop();
        }
    }
}
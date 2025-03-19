using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{
    public void ChangeVolume(float volume)
    {
        GetComponent<AudioSource>().volume = volume;
    }
}

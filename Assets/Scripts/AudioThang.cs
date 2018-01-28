using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioThang : MonoBehaviour 
{
	public AudioSource src { get; private set; }
	public float pitchVariance;
	public float originalPitch { get; private set; }

	void Awake()
	{
		src = GetComponent<AudioSource>();
		originalPitch = src.pitch;
	}
}
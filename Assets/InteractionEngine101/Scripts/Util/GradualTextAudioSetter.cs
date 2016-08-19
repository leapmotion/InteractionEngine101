using UnityEngine;
using System.Collections;

public class GradualTextAudioSetter : MonoBehaviour {

  public GradualText _toSetAudio;

  public AudioSource _perCharAudioSource;
  public AudioClip _perCharAudioFX;

  protected void Awake() {
    _toSetAudio.SetAudio(_perCharAudioSource, _perCharAudioFX);
  }

}

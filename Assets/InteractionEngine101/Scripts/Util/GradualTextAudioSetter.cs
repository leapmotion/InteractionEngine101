/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

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

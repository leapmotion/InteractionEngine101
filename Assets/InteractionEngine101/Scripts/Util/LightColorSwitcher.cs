/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using UnityEngine;
using System.Collections;

public class LightColorSwitcher : MonoBehaviour {

  public Light _light;
  public Color _A;
  public Color _B;

  public void SetColorToA() {
    _light.color = _A;
  }

  public void SetColorToB() {
    _light.color = _B;
  }

}

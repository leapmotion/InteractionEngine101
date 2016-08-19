/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using UnityEngine;
using System.Collections;

public class AllDone : IE101Example {

  protected override void OnEnable() {
    base.OnEnable();

    _story.SetDialogueText("That's all of the examples! Now that you know what the Interaction Engine does... what will you build with it?");
  } 

}

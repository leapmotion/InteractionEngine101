using UnityEngine;
using System.Collections;

public class AllDone : IE101Example {

  protected override void OnEnable() {
    base.OnEnable();

    _story.SetDialogueText("That's all of the examples! Now that you know what the Interaction Engine does... what will you build with it?");
  } 

}

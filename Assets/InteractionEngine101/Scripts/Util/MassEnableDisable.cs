using UnityEngine;
using System.Collections;

public class MassEnableDisable : MonoBehaviour {

  public Behaviour[] toToggle;

  public void EnableComponents() {
    for (int i = 0; i < toToggle.Length; i++) {
      toToggle[i].enabled = true;
    }
  }

  public void DisableComponents() {
    for (int i = 0; i < toToggle.Length; i++) {
      toToggle[i].enabled = false;
    }
  }

}

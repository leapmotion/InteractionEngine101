using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NewlineFriendlyTextSetter : MonoBehaviour {

  public Text _toSet;

  public void SetText(string text) {
    _toSet.text = text.Replace("\\n", "\n");
  }

}

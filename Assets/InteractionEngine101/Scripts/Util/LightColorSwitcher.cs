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

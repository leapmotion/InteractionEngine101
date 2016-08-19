using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ToggleableLight : MonoBehaviour {

  public Light toggleableLight;
  public Renderer toggleableLightMaterialObj;
  public Material litMaterial;
  public Material unlitMaterial;

  public bool isLit = false;

  public UnityEvent OnLit;
  public UnityEvent OnUnlit;

  private void SetMaterialToLit() {
    if (toggleableLightMaterialObj != null && litMaterial != null) {
      toggleableLightMaterialObj.material = litMaterial;
    }
  }

  private void SetMaterialToUnlit() {
    if (toggleableLightMaterialObj != null && unlitMaterial != null) {
      toggleableLightMaterialObj.material = unlitMaterial;
    }
  }

  public void TurnLightOn() {
    isLit = true;
    SetMaterialToLit();
    if (toggleableLight != null) {
      toggleableLight.enabled = true;
    }
    OnLit.Invoke();
  }
  public void TurnLightOff() {
    isLit = false;
    SetMaterialToUnlit();
    if (toggleableLight != null) {
      toggleableLight.enabled = false;
    }
    OnUnlit.Invoke();
  }
  public void ToggleLight() {
    if (isLit) {
      TurnLightOff();
    }
    else {
      TurnLightOn();
    }
  }

}

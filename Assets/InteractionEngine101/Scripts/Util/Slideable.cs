using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Slideable : MonoBehaviour {

  [Tooltip("Called when the Slideable finished a slide.")]
  public UnityEvent OnFinishedSliding;

  // Positional sliding logic

  public AnimationCurve slideCurve;
  private Vector3 fromSlide = Vector3.zero;
  private Vector3 toSlide = Vector3.zero;
  private float slideDuration = 1.5F;
  private float slideTimer = 0F;
  private bool sliding = false;

  protected virtual void Update() {
    if (sliding) {
      slideTimer += Time.deltaTime - 0.003F + Random.value * 0.006F;
      transform.position = Vector3.LerpUnclamped(fromSlide, toSlide, slideCurve.Evaluate(slideTimer / slideDuration));
      if (slideTimer > slideDuration) {
        sliding = false;
        slideTimer = 0F;
        OnFinishedSliding.Invoke();
      }
    }
  }

  public void StartSlide(Vector3 from, Vector3 to) {
    StartSlide(from, to, 1F);
  }

  public void StartSlide(Vector3 from, Vector3 to, float slideTime) {
    if (sliding) {
      Debug.LogWarning("[Slideable] StartSlide called, but the Slideable was not finished with its original slide.");
    }
    fromSlide = from;
    toSlide = to;
    transform.position = fromSlide;
    sliding = true;
  }

}

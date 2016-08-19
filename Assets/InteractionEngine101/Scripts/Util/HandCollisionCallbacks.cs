using UnityEngine;
using System.Collections;
using Leap.Unity.Interaction;
using System;
using Leap.Unity;

/// <summary>
/// This component provides a quick interface for accessing collision callbacks specific to
/// hands, as well as a separate set of callbacks for non-hand collisions.
/// </summary>
public class HandCollisionCallbacks : MonoBehaviour {

  public Action<GameObject> OnEnterCollisionWithHand;
  public Action<GameObject, GameObject> OnEnterCollisionWithOther;

  public Action<GameObject> OnExitCollisionWithHand;
  public Action<GameObject, GameObject> OnExitCollisionWithOther;

  void OnCollisionEnter(Collision collision) {
    if (collision.gameObject.GetComponent<InteractionBrushBone>() != null /* Interaction Brush Hands (Interaction Engine) */
      || collision.gameObject.GetComponentInParent<RigidHand>() != null /* Rigid Hands (non-Interaction Engine) */) {
      if (OnEnterCollisionWithHand != null) {
        OnEnterCollisionWithHand(gameObject);
      }
    }
    else {
      if (OnEnterCollisionWithOther != null) {
        OnEnterCollisionWithOther(gameObject, collision.gameObject);
      }
    }
  }

  void OnCollisionExit(Collision collision) {
    if (collision.gameObject.GetComponent<InteractionBrushBone>() != null
     || collision.gameObject.GetComponentInParent<RigidHand>() != null) {
      if (OnExitCollisionWithHand != null) {
        OnExitCollisionWithHand(gameObject);
      }
    }
    else {
      if (OnExitCollisionWithOther != null) {
        OnExitCollisionWithOther(gameObject, collision.gameObject);
      }
    }
  }

}

/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class GameObjectEvent : UnityEvent<GameObject> { }

/// <summary>
/// Periodically checks the state of the object against a provided set of bounds as a Vector3.up-aligned
/// cylinder (XZRadiusBoundary), providing callback hooks with a trigger delay for the out-of-bounds callback.
/// </summary>
public class OutOfBoundsCallbacks : MonoBehaviour {

  public XZRadiusBoundary _boundary;
  public float _waitTime = 2F;

  public GameObjectEvent OnLeaveBounds = new GameObjectEvent();
  public GameObjectEvent OnEnterBounds = new GameObjectEvent();

  private bool _outOfBounds = false;
  private float _outOfBoundsTimer = 0F;
  private float _checkPeriod = 0.2F;

  protected void Start() {
    StartCoroutine(WaitForBoundaryAssignment());
  }

  private IEnumerator WaitForBoundaryAssignment() {
    yield return new WaitForSecondsRealtime(0.1F);
    if (_boundary == null) {
      Debug.LogWarning("[OutOfBoundsCallbacks] No bounds assigned; won't receive any boundary callbacks.");
    }
    StartCoroutine(PeriodicallyCheckBounds());
  }

  private IEnumerator PeriodicallyCheckBounds() {
    for (;;) {
      if (_boundary != null) {
        if (_boundary.IsInBounds(this.transform.position) && _outOfBounds) {
          OnEnterBounds.Invoke(this.gameObject);
          _outOfBounds = false;
          _outOfBoundsTimer = 0F;
        }
        else if (!_boundary.IsInBounds(this.transform.position) && !_outOfBounds) {
          _outOfBoundsTimer += _checkPeriod;
          if (_outOfBoundsTimer >= _waitTime) {
            OnLeaveBounds.Invoke(this.gameObject);
            _outOfBounds = true;
          }
        }
      }
      yield return new WaitForSecondsRealtime(_checkPeriod);
    }
  }

  public void AssignBoundary(XZRadiusBoundary boundary) {
    this._boundary = boundary;
  }

}

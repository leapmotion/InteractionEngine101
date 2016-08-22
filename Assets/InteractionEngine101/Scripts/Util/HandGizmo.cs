/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity;
using Leap.Unity.Interaction;

/// <summary>
/// Draws gizmos at runtime around RigidHands (red) or InteractionBrushHands (green).
/// </summary>
public class HandGizmo : MonoBehaviour, IRuntimeGizmoComponent {

  public RigidHand _rigidHandL;
  public RigidHand _rigidHandR;
  public Color _rigidHandDrawColor = Color.red;

  public InteractionBrushHand _interactionHandL;
  public InteractionBrushHand _interactionHandR;
  public Color _interactionHandDrawColor = Color.green;

  private Color _curRigidHandColor;
  private bool _curDrawingRigidHandGizmo = false;
  private Color _curInteractionHandColor;
  private bool _curDrawingInteractionHandGizmo = false;

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    drawer.color = _curRigidHandColor;
    if (_rigidHandL.IsTracked) {
      drawer.DrawColliders(_rigidHandL.gameObject);
    }
    if (_rigidHandR.IsTracked) {
      drawer.DrawColliders(_rigidHandR.gameObject);
    }
    drawer.color = _curInteractionHandColor;
    if (_interactionHandL.IsTracked) {
      drawer.DrawColliders(_interactionHandL.GetHandParent());
    }
    if (_interactionHandR.IsTracked) {
      drawer.DrawColliders(_interactionHandR.GetHandParent());
    }
  }

  public void FlashRigidHandsColliderGizmos() {
    StartCoroutine(DoFlashRigidHandsColliderGizmos());
  }

  public void FlashInteractionHandsColliderGizmos() {
    StartCoroutine(DoFlashInteractionHandsColliderGizmos());
  }

  private IEnumerator DoFlashRigidHandsColliderGizmos() {
    if (_curDrawingRigidHandGizmo) {
      yield return null;
    }
    else {
      _curRigidHandColor = _rigidHandDrawColor;
      _curDrawingRigidHandGizmo = true;
      while (_curRigidHandColor.a > 0.05F) {
        _curRigidHandColor = new Color(_curRigidHandColor.r, _curRigidHandColor.g, _curRigidHandColor.b, _curRigidHandColor.a - Time.deltaTime); 
        yield return new WaitForEndOfFrame();
      }
      _curRigidHandColor = new Color(_curRigidHandColor.r, _curRigidHandColor.g, _curRigidHandColor.b, 0F);
      _curDrawingRigidHandGizmo = false;
    }
  }

  private IEnumerator DoFlashInteractionHandsColliderGizmos() {
    if (_curDrawingInteractionHandGizmo) {
      yield return null;
    }
    else {
      _curInteractionHandColor = _interactionHandDrawColor;
      _curDrawingInteractionHandGizmo = true;
      while (_curInteractionHandColor.a > 0.05F) {
        _curInteractionHandColor = new Color(_curInteractionHandColor.r, _curInteractionHandColor.g, _curInteractionHandColor.b, _curInteractionHandColor.a - Time.deltaTime);
        yield return new WaitForEndOfFrame();
      }
      _curInteractionHandColor = new Color(_curInteractionHandColor.r, _curInteractionHandColor.g, _curInteractionHandColor.b, 0F);
      _curDrawingInteractionHandGizmo = false;
    }
  }

}

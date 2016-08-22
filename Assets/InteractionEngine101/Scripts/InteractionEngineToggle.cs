/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine.Events;

/// <summary>
/// Given an Interaction Manager, this script can handle toggling the Interaction Engine on and off.
/// <b>This script is unnecessary for using the Interaction Engine itself</b>, but demonstrates how to toggle
/// dynamically between the Interaction Engine and the non-Interaction Engine solution RigidHands,
/// which aligns kinematic rigidbodies with the Leap hands to allow rudimentary interactions, for comparative purposes.
/// 
/// NOTE: The Interaction Engine will output errors to the console when RigidHands touch InteractionBehaviours.
/// This is to prevent users from accidentally using RigidHands in combination with the Interaction Engine;
/// the error is benign for the purposes of this project, which intentionally uses both.
/// </summary>
public class InteractionEngineToggle : MonoBehaviour {

  #region PUBLIC FIELDS

  [Tooltip("The Interaction Manager whose features will be enabled and disabled by this component.")]
  public InteractionManager _interactionManager;

  [Tooltip("The left RigidHand whose colliders will be enabled and disabled by this component.")]
  public RigidHand _rigidHandL;

  [Tooltip("The right RigidHand whose colliders will be enabled and disabled by this component.")]
  public RigidHand _rigidHandR;

  [Tooltip("Whether or not the scene should start with Interaction Engine features enabled or if the RigidHands should be enabled to begin with instead.")]
  public bool _startWithIEEnabled = false;

  #endregion

  #region PRIVATE FIELDS

  private Collider[] _rigidHandLColliders;
  private Collider[] _rigidHandRColliders;

  #endregion

  #region PROPERTIES

  public bool IsInteractionEngineEnabled {
    get { return _interactionManager.ContactEnabled; }
  }

  #endregion

  #region UNITY EVENTS

  public UnityEvent OnInteractionEngineEnabled; // (RigidHands off)
  public UnityEvent OnRigidHandsEnabled;        // (Interaction Engine off)

  #endregion

  #region UNITY CALLBACKS

  void Start() {
    _rigidHandLColliders = _rigidHandL.GetComponentsInChildren<Collider>();
    _rigidHandRColliders = _rigidHandR.GetComponentsInChildren<Collider>();

    if (_startWithIEEnabled) {
      EnableInteractionEngine();
    }
    else {
      DisableInteractionEngine();
    }
  }

  #endregion

  #region PUBLIC METHODS

  public void EnableInteractionEngine() {
    DisableKinematicHands();

    EnableSoftContact();
    EnableGrasping();
  }

  public void DisableInteractionEngine() {
    DisableGrasping();
    DisableSoftContact();

    EnableKinematicHands();
  }

  public void ToggleInteractionEngine() {
    if (IsInteractionEngineEnabled) {
      DisableInteractionEngine();
    }
    else {
      EnableInteractionEngine();
    }
  }

  #endregion

  #region PRIVATE METHODS

  // Soft Contact
  // Soft Contact handles poking, crushing, and smacking objects more
  // gracefully in impossible physical situations than simply treating
  // the joints of the hand as kinematic rigidbodies.

  private void EnableSoftContact() {
    _interactionManager.ContactEnabled = true;
    OnInteractionEngineEnabled.Invoke();
  }

  private void DisableSoftContact() {
    _interactionManager.ContactEnabled = false;
  }


  // Grasping
  // Grasping takes over for Soft Contact when the Interaction Engine detects
  // that a hand has grasped an object. Interaction Materials can modify what
  // happens when objects are grasped and released; by default, objects can
  // simply be picked up and dropped as one might expect in reality.

  private void EnableGrasping() {
    _interactionManager.GraspingEnabled = true;
  }
  private void DisableGrasping() {
    _interactionManager.GraspingEnabled = false;
  }


  // Kinematic Hands toggling
  // When the Interaction Engine is disabled, the player still needs some way to
  // interact with objects in the scene. To demonstrate the most obvious solution,
  // we switch the player to having hand joints simulated as kinematic rigidbodies.
  // Because the bodies are simulated as perfectly rigid, they are able to poke and
  // smack around other objects in the scene, but crushing and brushing things can cause
  // highly unstable results and objectsplosions. Good luck picking things up in this mode!

  private void EnableKinematicHands() {
    // To enable Kinematic Hands, walk through each hand's hierarchy and manually enable each collider.
    for (int i = 0; i < _rigidHandLColliders.Length; i++) {
      _rigidHandLColliders[i].isTrigger = false;
    }
    for (int i = 0; i < _rigidHandRColliders.Length; i++) {
      _rigidHandRColliders[i].isTrigger = false;
    }
    OnRigidHandsEnabled.Invoke();
  }

  private void DisableKinematicHands() {
    // To enable Kinematic Hands, walk through each hand's hierarchy and manually disable each collider.
    for (int i = 0; i < _rigidHandLColliders.Length; i++) {
      _rigidHandLColliders[i].isTrigger = true;
    }
    for (int i = 0; i < _rigidHandRColliders.Length; i++) {
      _rigidHandRColliders[i].isTrigger = true;
    }
  }

  #endregion

}

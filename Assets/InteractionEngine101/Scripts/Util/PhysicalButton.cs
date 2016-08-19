/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine;
using UnityEngine.Events;

public class PhysicalButton : Slideable {

  [Tooltip("The Rigidbody acting as the moving button to be pressed by other physical bodies.")]
  public Rigidbody _buttonBody;

  [Tooltip("Disables the button temporarily after every valid button press. Helps prevent double-firing when hands overlap with the button.")]
  public bool _tempDisableEveryPress = false;

  [Tooltip("The source of sound effects for this button.")]
  public AudioSource _buttonAudioSource;
  [Tooltip("The possible sounds to play when the button is pressed in far enough to click.")]
  public AudioClip[] _clickSounds;

  [Tooltip("The renderer that contains the material of the button.")]
  public Renderer _buttonRenderer;

  // Normal button presses
  private float _pressTriggerDistance = 0F;
  private float _pressReleaseDistance = 0F;
  private bool  _awaitingRelease = false;
  private bool  _buttonFunctionalityEnabled = true;

  // Broken constraint logic
  private Vector3 _restPosition = Vector3.zero;
  private float _brokenDistance = 0F;
  private bool _buttonResetting = false;

  // Rotational constraint
  private Quaternion _restRotation = Quaternion.identity; // local

  // Avoiding accidental presses logic
  private float _tempDisableTime = 0.5F;
  private float _tempDisableTimer = 0F;
  private bool _tempDisabled = false;

  // Timer collider disabling logic (used for Ragdoll example)
  private float _collidersDisabledTime = 0F;
  private float _collidersDisabledTimer = 0F;
  private bool _collidersDisabled = false;

  [Tooltip("Called when the button is physically pushed in.")]
  public UnityEvent OnPress;
  [Tooltip("Called when the button is released after being physically pushed in.")]
  public UnityEvent OnRelease;

  public void EnableButtonFunctionality() {
    _buttonFunctionalityEnabled = true;
  }

  public void DisableButtonFunctionality() {
    _buttonFunctionalityEnabled = false;
  }

  protected void Start() {
    _pressTriggerDistance = -0.025F;
    _pressReleaseDistance = -0.027F;

    _restPosition = _buttonBody.transform.localPosition;
    _brokenDistance = 0.06F;

    _restRotation = transform.localRotation;
  }

  protected override void Update() {
    base.Update();
    UpdateButtonPress();
    UpdateCollidersDisabledTimer();
    UpdateDisableTimer();
  }

  private void UpdateButtonPress() {

    // Normal button presses
    if (!_buttonResetting && !_tempDisabled) {
      if (!_awaitingRelease && _buttonBody.transform.localPosition.z > _pressTriggerDistance) {
        if (_buttonFunctionalityEnabled) {
          OnPress.Invoke();
          PlayButtonClickSound();
        }
        _awaitingRelease = true;
      }
      if (_awaitingRelease && _buttonBody.transform.localPosition.z < _pressReleaseDistance) {
        if (_buttonFunctionalityEnabled) {
          OnRelease.Invoke();
          if (_tempDisableEveryPress) {
            DisableTemporarily();
          }
        }
        _awaitingRelease = false;
      }
    }

    // Occasionally PhysicalButtons get stuck in when you press them.
    // So we keep the button Rigidbody awake if it's awaiting release to prevent
    // it from freezing in place erroneously.
    if (_awaitingRelease) {
      _buttonBody.WakeUp();
    }

    // Sometimes Kinematic Hands will move the buttons beyond where they should go, so
    // we should snap it back into place. (Temporarily, the button's collider becomes a trigger,
    // so it will ignore collisions.)
    if (!_buttonResetting && (_buttonBody.transform.localPosition - _restPosition).magnitude > _brokenDistance) {
      _buttonResetting = true;
      DisableButtonColliders();
    }
    if (_buttonResetting && (_buttonBody.transform.localPosition - _restPosition).magnitude < _brokenDistance / 3F
        && _buttonBody.velocity.magnitude < 0.1F) {
      _buttonResetting = false;
      EnableButtonColliders();
    }

    // Enforce local rotation
    transform.localRotation = _restRotation;

  }

  private void UpdateCollidersDisabledTimer() {
    if (_collidersDisabled) {
      _collidersDisabledTimer += Time.deltaTime;
      if (_collidersDisabledTimer > _collidersDisabledTime) {
        _collidersDisabledTimer = 0F;
        _collidersDisabled = false;
        EnableButtonColliders();
      }
    }
  }

  /// <summary>
  /// Temporarily prevents the PhysicalButton from colliding with anything
  /// by turning its colliders into triggers. (disableDuration in seconds.)
  /// </summary>
  public void DisableColliders(float disableDuration) {
    DisableButtonColliders();

    _collidersDisabledTimer = 0F;
    _collidersDisabledTime = disableDuration;
    _collidersDisabled = true;
  }

  private void DisableButtonColliders() {
    var colliders = GetComponentsInChildren<Collider>();
    for (int i = 0; i < colliders.Length; i++) {
      colliders[i].isTrigger = true;
    }
  }

  private void EnableButtonColliders() {
    var colliders = GetComponentsInChildren<Collider>();
    for (int i = 0; i < colliders.Length; i++) {
      colliders[i].isTrigger = false;
    }
  }

  /// <summary>
  /// To avoid errant rigidbodies clicking buttons accidentally, we detect CollisionEnter events
  /// from a simple script attached to the Button rigidbody and make sure the collision event
  /// should allow the button to be depressed (only if it's part of a hand or being grabbed).
  /// Otherwise we disable the button for a half a second (won't fix all cases, but most of them).
  /// 
  /// Also disable the button if the camera angle to it is too wide -- hands sometimes freak out at
  /// the edge of tracking, so this prevents accidental button presses due to that effect.
  /// </summary>
  public void ReceiveCollisionEnter(Collision coll, Collider buttonCollider) {
    // If there's a collision with something that's a part of a PhysicalButton, ignore it
    if (coll.collider.GetComponentInParent<PhysicalButton>() != null) return;

    // Otherwise, disable the button if it collides with something
    // that is neither part of hand nor being grabbed.
    if (!IsPartOfHand(coll.collider)
        && !IsBeingGrasped(coll.collider)) {
          Debug.Log(this.transform.parent.name + ": temp-disabled because colliding with " + coll.gameObject.name);
      DisableTemporarily();
    }
  }

  private bool IsPartOfHand(Collider collider) {
    return (collider.GetComponent<InteractionBrushBone>() != null || collider.GetComponentInParent<IHandModel>() != null);
  }

  private bool IsBeingGrasped(Collider collider) {
    InteractionBehaviour interactionBehaviour = collider.GetComponent<InteractionBehaviour>();
    if (interactionBehaviour == null) return false;
    return interactionBehaviour.IsBeingGrasped;
  }

  private void DisableTemporarily() {
    _tempDisabled = true;
    _tempDisableTime = 0.5F;
    _tempDisableTimer = 0F;
  }

  private void UpdateDisableTimer() {
    if (_tempDisabled) {
      _tempDisableTimer += Time.deltaTime;
      if (_tempDisableTimer > _tempDisableTime) {
        _tempDisableTimer = 0F;
        _tempDisabled = false;
      }
    }
  }

  private void PlayButtonClickSound() {
    _buttonAudioSource.PlayOneShot(_clickSounds[Random.Range(0, _clickSounds.Length)]);
  }

}

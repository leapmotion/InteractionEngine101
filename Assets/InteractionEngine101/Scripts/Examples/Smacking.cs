/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap.Unity.Interaction;

public class Smacking : IE101Example {

  #region PRIVATE FIELDS

  private GameObject _seesawBase;
  private GameObject _seesawBoard;
  private GameObject _seesawAmmo;

  private IEnumerator _attemptSuccessfulRoutine;
  private IEnumerator _attemptFailedRoutine;
  private bool _waitingAfterAttempt = false;
  private bool _ammoTouchingBoard = false;
  private bool _boardTouchingBase = false;

  #endregion

  #region EXAMPLE IMPLEMENTATION

  protected override void OnEnable() {
    base.OnEnable();

    _waitingAfterAttempt = false;
    if (_attemptSuccessfulRoutine != null) {
      StopCoroutine(_attemptSuccessfulRoutine);
    }
    if (_attemptFailedRoutine != null) {
      StopCoroutine(_attemptFailedRoutine);
    }

    _story.SetDialogueText("Smack the seesaw to launch the block!");

    _seesawBase = _story._blockSpawner.Spawn();
    _seesawBase.transform.localScale = new Vector3(0.8F, 1F, 0.8F);
    _seesawBase.transform.position = Vector3.up * 0.16F + Vector3.forward * 0.2F;
    _seesawBase.AddComponent<InteractionBehaviour>();
    _seesawBase.AddComponent<InteractionSoundFX>();
    _seesawBase.GetComponent<Rigidbody>().mass = 0.2F * _story._blockSpawner.DefaultBlockMass;

    _seesawBoard = _story._blockSpawner.Spawn();
    _seesawBoard.transform.localScale = new Vector3(3.0F, 0.5F, 1.0F);
    _seesawBoard.transform.position = Vector3.up * 0.3F + Vector3.forward * 0.2F;
    _seesawBoard.AddComponent<InteractionBehaviour>();
    _seesawBoard.AddComponent<InteractionSoundFX>();
    _seesawBoard.GetComponent<Rigidbody>().mass = 0.1F * _story._blockSpawner.DefaultBlockMass;

    _seesawAmmo = _story._blockSpawner.Spawn();
    _seesawAmmo.transform.localScale = new Vector3(0.5F, 0.5F, 0.5F);
    _seesawAmmo.transform.position = Vector3.up * 0.8F + Vector3.left * 0.12F + Vector3.forward * 0.2F;
    _seesawAmmo.AddComponent<InteractionBehaviour>();
    _seesawAmmo.AddComponent<InteractionSoundFX>();
    _seesawAmmo.GetComponent<Rigidbody>().mass = 0.007F * _story._blockSpawner.DefaultBlockMass;

    _seesawAmmo.AddComponent<HandCollisionCallbacks>().OnEnterCollisionWithOther += OnAmmoHitNonHand;
    _seesawAmmo.GetComponent<HandCollisionCallbacks>().OnExitCollisionWithOther += OnAmmoLeftNonHand;

    _seesawBoard.AddComponent<HandCollisionCallbacks>().OnEnterCollisionWithHandWithVelocity += OnBoardHitHand;
    _seesawBoard.GetComponent<HandCollisionCallbacks>().OnEnterCollisionWithOther += OnBoardHitNonHand;
    _seesawBoard.GetComponent<HandCollisionCallbacks>().OnExitCollisionWithOther += OnBoardLeftNonHand;
  }

  public override void Respawn() {
    base.Respawn();

    OnDisable();
    OnEnable();
  }

  protected override void OnComplete() {
    if (_story._toggleIE.IsInteractionEngineEnabled && !HasBeenCompletedWithIE) {
      _story.SetDialogueText("Even though the Interaction Engine lets your hand softly push through objects, you can still smack things around.");
    }
    else if (!_story._toggleIE.IsInteractionEngineEnabled && !HasBeenCompletedWithoutIE) {
      _story.SetDialogueText("Of course, hitting things with kinematic hands still works, without using the Interaction Engine.");
    }

    base.OnComplete();
  }

  protected override void OnDisable() {
    base.OnDisable();

    GameObject.Destroy(_seesawBase);
    GameObject.Destroy(_seesawBoard);
    GameObject.Destroy(_seesawAmmo);
  }

  #endregion

  #region PRIVATE METHODS

  // Success measurement

  private void OnAmmoHitNonHand(GameObject ammoObj, GameObject otherObj) {
    if (otherObj == _seesawBoard) {
      _ammoTouchingBoard = true;
    }
  }

  private void OnAmmoLeftNonHand(GameObject ammoObj, GameObject otherObj) {
    if (otherObj == _seesawBoard) {
      _ammoTouchingBoard = false;
    }
  }

  private void OnBoardHitNonHand(GameObject seesawBoard, GameObject otherObj) {
    if (otherObj == _seesawBase) {
      _boardTouchingBase = true;
    }
  }

  private void OnBoardLeftNonHand(GameObject seesawBoard, GameObject otherObj) {
    if (otherObj == _seesawBase) {
      _boardTouchingBase = false;
    }
  }

  private void OnBoardHitHand(GameObject boardObj, float collisionSpeed) {
    if (_ammoTouchingBoard && _boardTouchingBase && (!HasBeenCompletedWithIE || !HasBeenCompletedWithoutIE) && collisionSpeed > 1.0F) {
      StartCoroutine(DoMeasureAmmoVelocity());
    }
  }

  private bool _measuringVelocity = false;
  private IEnumerator DoMeasureAmmoVelocity() {
    if (!_measuringVelocity) {
      _measuringVelocity = true;
      yield return new WaitForSecondsRealtime(0.1F);
      if (base.IsExampleActive) {  // User might have changed examples during the wait
        float ammoVelocity = _seesawAmmo.GetComponent<Rigidbody>().velocity.magnitude;
        JudgeLaunchSpeed(ammoVelocity);
        // Also play a sound effect on launch.
        var interactionSFX = _seesawAmmo.GetComponent<InteractionSoundFX>();
        if (interactionSFX != null) {
          interactionSFX.PlayThrowSFX();
        }
      }
      _measuringVelocity = false;
    }
    else {
      yield return null;
    }
  }

  private void JudgeLaunchSpeed(float speed) {
    string text = "";
    bool goodEnoughToComplete = false;
    if (speed < 1.0F) {
      text = "Whiffed it. Try again! I'll reset the seesaw for you.";
      goodEnoughToComplete = false;
    }
    else if (speed >= 1.0F && speed < 1.5F) {
      text = "Eh, just okay. Try hitting it a little harder! I'll reset the seesaw for you.";
      goodEnoughToComplete = false;
    }
    else if (speed >= 1.5F && speed < 3.5F) {
      text = "Nice! Launch velocity: " + speed.ToString("G2") + " m/s.";
      goodEnoughToComplete = true;
    }
    else {
      text = "Going, going, gone! Launch velocity: " + speed.ToString("G2") + " m/s.";
      goodEnoughToComplete = true;
    }

    _story.SetDialogueText(text);
    if (goodEnoughToComplete) {
      _attemptSuccessfulRoutine = DoCompleteAfterSeconds(2.5F);
      StartCoroutine(_attemptSuccessfulRoutine);
    }
    else {
      _attemptFailedRoutine = DoRespawnAfterSeconds(2.5F);
      StartCoroutine(_attemptFailedRoutine);
    }
  }

  #endregion

  #region COROUTINES

  private IEnumerator DoCompleteAfterSeconds(float seconds) {
    _waitingAfterAttempt = true;
    yield return new WaitForSecondsRealtime(seconds);
    if (this.IsExampleActive && _waitingAfterAttempt) {
      OnComplete();
    }
  }

  private IEnumerator DoRespawnAfterSeconds(float seconds) {
    _waitingAfterAttempt = true;
    yield return new WaitForSecondsRealtime(seconds);
    if (this.IsExampleActive && _waitingAfterAttempt) {
      Respawn();
    }
  }

  #endregion

}

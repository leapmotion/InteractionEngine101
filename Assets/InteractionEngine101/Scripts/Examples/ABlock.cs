/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;
using Leap.Unity.Interaction;

public class ABlock : IE101Example {

  #region PRIVATE FIELDs

  private GameObject _block;
  private bool _blockHeld = false;
  private bool _ieHintGiven = false;

  private Coroutine _blockIntroCoroutine;
  private Coroutine _waitForBlockPickupCoroutine;

  #endregion

  #region EXAMPLE IMPLEMENTATION

  protected override void OnEnable() {
    base.OnEnable();

    _blockIntroCoroutine = StartCoroutine(DoBlockIntro());
  }

  public override void Respawn() {
    base.Respawn();
    SpawnBlockWithCallbacks();
  }

  protected override void OnDisable() {
    base.OnDisable();

    if (_blockIntroCoroutine != null) {
      StopCoroutine(_blockIntroCoroutine);
    }
    if (_waitForBlockPickupCoroutine != null) {
      StopCoroutine(_waitForBlockPickupCoroutine);
    }
    if (_block != null) {
      Destroy(_block);
    }
  }

  #endregion

  #region PRIVATE METHODS

  private void SpawnBlockWithCallbacks() {
    if (_block != null) {
      Destroy(_block);
    }

    _block = _story.SpawnCeilingBlock();
    var blockInteractionBehaviour = _block.AddComponent<InteractionBehaviour>();
    blockInteractionBehaviour.OnGraspBeginEvent += OnBlockGrasped;
    blockInteractionBehaviour.OnGraspEndEvent += OnBlockReleased;

    _block.AddComponent<InteractionSoundFX>();

    var boundsCallbacks = _block.AddComponent<OutOfBoundsCallbacks>();
    boundsCallbacks._boundary = _story.RespawnBoundary;
    boundsCallbacks.OnLeaveBounds.AddListener(RespawnOnBlockOutOfBounds);
  }

  private void OnBlockGrasped() {
    _blockHeld = true;
  }

  private void OnBlockReleased() {
    _blockHeld = false;
  }

  private void RespawnOnBlockOutOfBounds(GameObject block) {
    Respawn();
  }

  #endregion

  #region COROUTINES

  private IEnumerator DoBlockIntro() {
    _story.SetDialogueText("This is a block.");
    yield return new WaitForSecondsRealtime(0.5F);

    SpawnBlockWithCallbacks();
    yield return new WaitForSecondsRealtime(1.3F);

    _story.SetDialogueText("To <b>pick up blocks</b>, use the <b>left thumbstick</b> and hold <b>A</b> and <b>B</b> while hovering near the center of a block...");
    yield return new WaitForSecondsRealtime(4F);

    _story.SetDialogueText("Just kidding!");
    yield return new WaitForSecondsRealtime(1.8F);

    _story.SetDialogueText("No gamepads or joysticks here. Just reach out and pick up the block with either hand.");
    yield return new WaitForSecondsRealtime(3F);

    _waitForBlockPickupCoroutine = StartCoroutine(DoWaitForBlockPickup());
  }

  private IEnumerator DoWaitForBlockPickup() {
    while (!_blockHeld) {
      yield return new WaitForSecondsRealtime(0.2F);

      if (!_story._toggleIE.IsInteractionEngineEnabled && !_ieHintGiven) {
        _ieHintGiven = true;
        _story.SetDialogueText("Hint: You won't be able to pick up this block unless you enable the Interaction Engine using the button on your right.");
      }
    }

    OnComplete();
    _story.SetDialogueText("Yep! Just like that. Drop it, throw it, nudge it or smack it around. It's easy to build physical experiences with virtual objects using the Interaction Engine.");
    yield return new WaitForSecondsRealtime(6F);

    _story.SetDialogueText("When you're ready, you can move on to the next example by hitting Next.");
  }

  #endregion

}

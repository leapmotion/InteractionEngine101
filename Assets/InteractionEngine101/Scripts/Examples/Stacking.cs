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

public class Stacking : IE101Example {

  #region PRIVATE FIELDS

  private const int NUM_BLOCKS = 4;

  private GameObject[] _blocks = new GameObject[NUM_BLOCKS];
  private float[] _blockScales = new float[] { 1.0F, 0.9F, 0.8F, 0.7F };
  private bool _respawningBlocks = false;

  private float _successHeight = 0.15F;
  private float _successTime = 0.25F;
  private bool _ableToBeCompleted = true;
  private bool _receivedFirstOOBCallback = false; // example reminds player that examples can be manually reset.

  private float[] _successTimers = new float[NUM_BLOCKS];           // Win when any block is up high enough for _successTime seconds
  private bool[] _handTouchingBlock = new bool[NUM_BLOCKS];         // can't win by holding a block
  private bool[] _blocksTouchingSlideable = new bool[NUM_BLOCKS];   // or by putting the block on one of the physical UI elements.
  private bool[] _blocksTouchingGround = new bool[NUM_BLOCKS];

  #endregion

  #region EXAMPLE IMPLEMENTATION

  protected override void OnEnable() {
    base.OnEnable();

    StartCoroutine(SpawnCeilingBlocksWithDelay(0.2F));
  }

  public override void Respawn() {
    // Only allow respawn if we're not in the middle of spawning this example's blocks.
    if (!_respawningBlocks) {
      base.Respawn();

      OnDisable();
      OnEnable();
    }
  }

  protected override void OnDisable() {
    base.OnDisable();

    for (int i = 0; i < NUM_BLOCKS; i++) {
      Destroy(_blocks[i]);
    }
  }

  protected override void OnComplete() {
    if (_story._toggleIE.IsInteractionEngineEnabled && !HasBeenCompletedWithIE) {
      _story.SetDialogueText("Nice! Be sure to try completing interactions without the Interaction Engine enabled to feel the difference.");
    }
    else if (!_story._toggleIE.IsInteractionEngineEnabled && !HasBeenCompletedWithoutIE) {
      _story.SetDialogueText("Great! It's a bit harder without the Interaction Engine, huh?");
    }

    base.OnComplete();
  }

  #endregion EXAMPLE IMPLEMENTATION

  #region PRIVATE METHODS

  private void SetupBoundsCallbacks(GameObject block) {
    var boundsCallbacks = block.AddComponent<OutOfBoundsCallbacks>();
    boundsCallbacks.AssignBoundary(_story.RespawnBoundary);
    boundsCallbacks.OnLeaveBounds.AddListener(OnBlockOutOfBounds);
  }

  private void OnBlockHandTouch(GameObject blockTouched) {
    for (int i = 0; i < NUM_BLOCKS; i++) {
      if (blockTouched == _blocks[i]) {
        _handTouchingBlock[i] = true;
        break;
      }
    }
  }

  private void OnBlockHandUntouch(GameObject blockUntouched) {
    for (int i = 0; i < NUM_BLOCKS; i++) {
      if (blockUntouched == _blocks[i]) {
        _handTouchingBlock[i] = false;
        break;
      }
    }
  }

  private void OnBlockTouchOther(GameObject block, GameObject other) {
    for (int i = 0; i < NUM_BLOCKS; i++) {
      if (block == _blocks[i]) {
        if (other.GetComponentInParent<Slideable>() != null) {
          _blocksTouchingSlideable[i] = true;
          break;
        }
        else if (other == _story._groundObj) {
          _blocksTouchingGround[i] = true;
        }
      }
    }
  }

  private void OnBlockLeaveOther(GameObject block, GameObject other) {
    for (int i = 0; i < NUM_BLOCKS; i++) {
      if (block == _blocks[i]) {
        if (other.GetComponentInParent<Slideable>() != null) {
          _blocksTouchingSlideable[i] = false;
          break;
        }
        else if (other == _story._groundObj) {
          _blocksTouchingGround[i] = false;
        }
      }
    }
  }

  private void OnBlockOutOfBounds(GameObject block) {
    if (!_receivedFirstOOBCallback) {
      StartCoroutine(NotifyOfExampleReset());
      _receivedFirstOOBCallback = true;
    }
  }

  #endregion

  #region COROUTINES

  private IEnumerator SpawnCeilingBlocksWithDelay(float delayEachBlock) {
    _respawningBlocks = true;
    for (int i = 0; i < NUM_BLOCKS; i++) {
      if (!this.IsExampleActive) {
        break; // Don't continue spawning blocks if the example has been cleaned up (destroyed)
      }

      var block = _story.SpawnCeilingBlock();
      SetupBoundsCallbacks(block);
      block.transform.localScale = Vector3.one * _blockScales[i];
      block.AddComponent<InteractionBehaviour>();
      block.AddComponent<InteractionSoundFX>();
      block.GetComponent<Rigidbody>().mass = 5F * _story._blockSpawner.DefaultBlockMass;

      var collCallbacks = block.AddComponent<HandCollisionCallbacks>();
      collCallbacks.OnEnterCollisionWithHand += OnBlockHandTouch;
      collCallbacks.OnExitCollisionWithHand += OnBlockHandUntouch;
      collCallbacks.OnEnterCollisionWithOther += OnBlockTouchOther;
      collCallbacks.OnExitCollisionWithOther += OnBlockLeaveOther;

      _blocks[i] = block;

      yield return new WaitForSecondsRealtime(delayEachBlock);
    }

    if (this.IsExampleActive) {
      _story.SetDialogueText("Try stacking these blocks!");
      StartCoroutine(PeriodicallyCheckSuccess());
    }

    _respawningBlocks = false;
  }

  private IEnumerator PeriodicallyCheckSuccess() {
    float successCheckPeriod = 0.07F;
    while (true) {

      if (!this.IsExampleActive) {
        // Example was cleaned up, so break out of success checking.
        break;
      }

      int blocksTouchingGroundCount = 0;
      for (int i = 0; i < NUM_BLOCKS; i++) {
        if (_blocksTouchingGround[i]) {
          blocksTouchingGroundCount++;
        }
      }
      if (blocksTouchingGroundCount >= 2) {
        for (int i = 0; i < _successTimers.Length; i++) {
          _successTimers[i] = 0F;
        }
        // If there are two or more blocks touching the ground,
        // the stack has been destroyed, so the user can complete the example
        // again by reconstructing a stack, e.g., with the IE toggle to a different state.
        _ableToBeCompleted = true;
      }

      for (int i = 0; i < NUM_BLOCKS; i++) {
        if (_blocks[i].transform.position.y >= _successHeight
          && _blocks[i].GetComponent<Rigidbody>().velocity.magnitude < 0.1F
          && !_blocksTouchingSlideable[i]
          && !_handTouchingBlock[i]) {
          _successTimers[i] += successCheckPeriod;
        }
        else {
          _successTimers[i] = 0F;
        }

        if (_successTimers[i] > _successTime && _ableToBeCompleted) {
          _ableToBeCompleted = false;
          OnComplete();
          break;
        }
      }

      yield return new WaitForSecondsRealtime(successCheckPeriod);
    }
  }

  private IEnumerator NotifyOfExampleReset() {
    _story.SetDialogueText("Uh oh. Need more blocks?");
    yield return new WaitForSecondsRealtime(3F);
    if (!this.IsExampleActive) {
      yield return null;
    }
    else {
      _story.SetDialogueText("You can reset the current interaction example using the panel on your left.");
      yield return new WaitForSecondsRealtime(1F);
    }
  }

  #endregion

}

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

public class Smushing : IE101Example {

  #region PRIVATE FIELDS

  private GameObject[] _cubes = new GameObject[4];
  private float[] _cubeScales = new float[4] { 0.6F, 0.75F, 0.9F, 1.05F, };
  private Vector3[] _cubeSpawnPositions = new Vector3[4] {
    new Vector3(0.0F,  0.24F, 0.1F),
    new Vector3(0.0F,  0.18F, 0.1F),
    new Vector3(0.0F,  0.10F,  0.1F),
    new Vector3(0.0F,  0.00F,  0.1F)
  };

  private bool _ieHintGiven = false;
  private bool[] _blocksTouched = new bool[4];

  #endregion

  #region EXAMPLE IMPLEMENTATION

  protected override void OnEnable() {
    base.OnEnable();

    _story.SetDialogueText("What do you think should happen if you try to smush this stack from the top down?\n(Try it!)");

    _ieHintGiven = false;
    for (int i = 0; i < 4; i++) {
      _blocksTouched[i] = false;
    }

    SpawnCube(0);
    SpawnCube(1);
    SpawnCube(2);
    SpawnCube(3);
  }

  public override void Respawn() {
    base.Respawn();

    RespawnCubes();
  }

  protected override void OnDisable() {
    base.OnDisable();

    GameObject.Destroy(_cubes[0]);
    GameObject.Destroy(_cubes[1]);
    GameObject.Destroy(_cubes[2]);
    GameObject.Destroy(_cubes[3]);
  }

  protected override void OnComplete() {
    if (_story._toggleIE.IsInteractionEngineEnabled) {
      _story.SetDialogueText("The Interaction Engine prevents instability when your hand does things that don't make physical sense, like \"smushing\" perfectly rigid objects.");
    }
    else {
      Debug.Log("Completed Smushing with IE disabled. (Ignored success condition.)");
    }

    base.OnComplete();
  }

  #endregion

  #region PRIVATE METHODS

  private void SpawnCube(int cubeIdx) {
    _cubes[cubeIdx] = _story._blockSpawner.Spawn();
    _cubes[cubeIdx].GetComponent<Rigidbody>().mass = _story._blockSpawner.DefaultBlockMass * 0.32F; // Tweak block mass for good stack behaviour
    _cubes[cubeIdx].transform.localScale = Vector3.one * _cubeScales[cubeIdx];
    _cubes[cubeIdx].transform.position = _cubeSpawnPositions[cubeIdx];
    _cubes[cubeIdx].AddComponent<InteractionBehaviour>();
    _cubes[cubeIdx].AddComponent<InteractionSoundFX>();

    // Collision callbacks for success detection
    var collCallbacks = _cubes[cubeIdx].AddComponent<HandCollisionCallbacks>();
    collCallbacks.OnEnterCollisionWithHand += OnHandCollisionEnter;

    // Out of bounds respawning
    var boundsCallbacks = _cubes[cubeIdx].AddComponent<OutOfBoundsCallbacks>();
    boundsCallbacks.OnLeaveBounds.AddListener(OnCubeOutOfBounds);
    boundsCallbacks._boundary = _story.RespawnBoundary;
  }

  private void RespawnCubes() {
    for (int i = 0; i < 4; i++) {
      if (_cubes[i] != null) {
        _cubes[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
        _cubes[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        _cubes[i].transform.rotation = Quaternion.Euler(Vector3.zero);
        _cubes[i].transform.position = _cubeSpawnPositions[i];
      }
      else {
        Debug.LogError("Unable to respawn cube; no cube currently exists at the argument index.");
      }

      // Also reset success condition tracking.
      _blocksTouched[i] = false;
    }
  }

  private void OnCubeOutOfBounds(GameObject cube) {
    // Respawn the entire stack if any one cube leaves the stage boundary.
    RespawnCubes();
  }

  // Success measurement
  // We expect the player to move their hand through the cubes from the top down.
  // For success reliability, we only care if the player has touched every other cube
  // by the time they touch the bottom cube in the stack.

  private void OnHandCollisionEnter(GameObject collidedWithObj) {
    for (int i = 0; i < _cubes.Length; i++) {
      if (collidedWithObj == _cubes[i]) {
        if (i > 1 && !_ieHintGiven && !_story._toggleIE.IsInteractionEngineEnabled) {
          _ieHintGiven = true;
          _story.SetDialogueText("With kinematic rigidbody hands, the cubes will stutter and fight to stop colliding with your hand, destroying the stack.");
        }
        else if (_story._toggleIE.IsInteractionEngineEnabled) {
          if (i == 3) {
            // Bottom block was touched. Succeed if the player has touched every other cube.
            if (_blocksTouched[0] && _blocksTouched[0] && _blocksTouched[1] && _blocksTouched[2]) {
              if (!HasBeenCompletedWithIE) { // Can only complete this exercise with IE enabled.
                OnComplete();
              }
            }
          }
          else {
            _blocksTouched[i] = true;
          }
        }
      }
    }
  }

  #endregion

}

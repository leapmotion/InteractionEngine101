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

public class Throwing : IE101Example {

  #region PUBLIC FIELDS

  public GameObject _ballPrefab;

  public AudioSource _signMovementAudioSource;
  public AudioClip[] _signMovementClips;

  #endregion

  #region PRIVATE FIELDS

  private const int NUM_TOWER_BLOCKS = 4;

  private GameObject _ammoBall;
  private GameObject[] _towerBlocks = new GameObject[NUM_TOWER_BLOCKS];

  private float[] _scales = new float[] { 1.0F, 1.1F, 1.2F, 1.3F };
  private Vector3[] _positions = new Vector3[] {
    new Vector3(0F, 0.36F, 0.8F),
    new Vector3(0F, 0.25F, 0.8F),
    new Vector3(0F, 0.13F, 0.8F),
    new Vector3(0F, 0.00F, 0.8F)
  };

  private bool _isDialogueBoxSlidBack = false;
  private Vector3 _dialogueBoxOriginalPosition;
  private Slideable _dialogueBox;

  private bool _readyToComplete = false;
  private bool _respawning = false;

  #endregion

  #region EXAMPLE IMPLEMENTATION

  protected override void OnEnable() {
    base.OnEnable();

    _readyToComplete = true;

    if (_dialogueBox == null) {
      _dialogueBox = _story._dialogueText.GetComponentInParent<Slideable>();
      _dialogueBoxOriginalPosition = _dialogueBox.transform.position;
    }
    if (!_isDialogueBoxSlidBack) {
      _dialogueBox.StartSlide(_dialogueBoxOriginalPosition, _dialogueBoxOriginalPosition + Vector3.forward * 0.7F, 1.5F);
      _isDialogueBoxSlidBack = true;
      PlaySlideFX();
    }

    _story.SetDialogueText("Throw the ball to topple the tower!");

    _ammoBall = Instantiate(_ballPrefab);
    _ammoBall.transform.position = Vector3.up * 0.7F + Vector3.forward * 0.1F;
    _ammoBall.transform.localScale = Vector3.one * 0.09F;
    _ammoBall.AddComponent<InteractionBehaviour>();
    _ammoBall.GetComponent<Rigidbody>().mass = 5F * _story._blockSpawner.DefaultBlockMass;
    _ammoBall.AddComponent<BallInteractionSoundFX>();
    var collCallbacks = _ammoBall.AddComponent<HandCollisionCallbacks>();
    collCallbacks.OnEnterCollisionWithOther += OnAmmoHit;
    var boundsCallbacks = _ammoBall.AddComponent<OutOfBoundsCallbacks>();
    boundsCallbacks._boundary = _story.RespawnBoundary;
    boundsCallbacks._waitTime = 1F;
    boundsCallbacks.OnLeaveBounds.AddListener(OnAmmoLeaveBounds);

    StartCoroutine(DoSpawnTowerAfterWait());
  }

  public override void Respawn() {
    base.Respawn();

    _respawning = true;
    OnDisable();
    OnEnable();
    _respawning = false;
  }

  protected override void OnComplete() {
    if (_story._toggleIE.IsInteractionEngineEnabled && !HasBeenCompletedWithIE) {
      _story.SetDialogueText("Got it! Picking up and throwing works pretty much like you'd expect with the Interaction Engine.");
    }
    else if (!_story._toggleIE.IsInteractionEngineEnabled && !HasBeenCompletedWithoutIE) {
      _story.SetDialogueText("Nice! The Interaction Engine makes it possible to pick up and throw objects like you'd expect, but of course, there are other ways of destroying a tower.");
    }

    if (_isDialogueBoxSlidBack) {
      _dialogueBox.StartSlide(_dialogueBoxOriginalPosition + Vector3.forward * 0.7F, _dialogueBoxOriginalPosition, 1.5F);
      _isDialogueBoxSlidBack = false;
      PlaySlideFX();
    }

    base.OnComplete();
  }

  protected override void OnDisable() {
    base.OnDisable();

    Destroy(_ammoBall);
    for (int i = 0; i < _towerBlocks.Length; i++) {
      Destroy(_towerBlocks[i]);
    }
    if (_isDialogueBoxSlidBack && !_respawning) {
      _dialogueBox.StartSlide(_dialogueBoxOriginalPosition + Vector3.forward * 0.7F, _dialogueBoxOriginalPosition, 1.5F);
      _isDialogueBoxSlidBack = false;
      PlaySlideFX();
    }
  }

  #endregion

  #region PRIVATE METHODS

  // A little fun: The sign doesn't like it when you hit him with the ball.
  private string[] _ouchPhrases = new string[] {
    "Ow! Hey watch it!",
    "Oof! Try to hit the tower, not me!",
    "Ouch! Hey!",
    "Aiyee!",
    "Not me, the tower!",
    "Okay, I guess I sort of asked for it, putting the tower there.",
    "Now you're just being mean!"
  };
  private int _timesHitSign = 0;
  private float _ouchPhraseCooldown = 0F;
  private void OnAmmoHit(GameObject ammoBlock, GameObject other) {
    if (other.transform.parent != null && other.transform.parent.parent != null && other.transform.parent.parent.name == "Primary Dialogue Box") {
      _story.SetDialogueText(_ouchPhrases[Mathf.Min(_timesHitSign++, _ouchPhrases.Length-1)]);
      _ouchPhraseCooldown = 2F;
      StartCoroutine(DoPhraseCooldown());
    }
  }

  private void OnAmmoLeaveBounds(GameObject ammoBlock) {
    _ammoBall.transform.position = Vector3.zero;
    _ammoBall.GetComponent<Rigidbody>().velocity = Vector3.zero;
    _ammoBall.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
  }

  private void OnHit(GameObject towerBlock, GameObject other) {
    if (towerBlock == _towerBlocks[0] && (other == _story._groundObj || other == _towerBlocks[NUM_TOWER_BLOCKS - 1])) {
      if (_readyToComplete) {
        OnComplete();
        _readyToComplete = false;
      }
    }
    else if (other == _ammoBall) {
      // bugfix to prevent higher-up blocks from hovering in midair on certain throws
      for (int i = 0; i < _towerBlocks.Length; i++) {
        _towerBlocks[i].GetComponent<Rigidbody>().WakeUp();
      }
    }
  }

  private void PlaySlideFX() {
    StartCoroutine(DoPlaySlideFXAfterDelay(0.4F));
    
  }

  #endregion

  #region COROUTINES

  private IEnumerator DoSpawnTowerAfterWait() {
    // Spawn the blocks immediately (far away) so they can be destroyed immediately if the example changes;
    // otherwise we'll place them after the wait duration.
    for (int i = 0; i < NUM_TOWER_BLOCKS; i++) {
      var block = _story._blockSpawner.Spawn();
      block.transform.position = _positions[i] + Vector3.up * 10000F + Vector3.left * 10000;
      block.transform.localScale = new Vector3(1F, 1F, 1F) * _scales[i];
      block.AddComponent<InteractionBehaviour>();
      block.AddComponent<InteractionSoundFX>();
      block.GetComponent<Rigidbody>().mass = _scales[i] * 0.5F * _story._blockSpawner.DefaultBlockMass;
      var collCallbacks = block.AddComponent<HandCollisionCallbacks>();
      collCallbacks.OnEnterCollisionWithOther += OnHit;

      _towerBlocks[i] = block;
    }

    yield return new WaitForSecondsRealtime(1F);

    if (!this.IsExampleActive) {
      yield return null;
    }
    else {
      for (int i = 0; i < NUM_TOWER_BLOCKS; i++) {
        var blockBody = _towerBlocks[i].GetComponent<Rigidbody>();
        blockBody.velocity = Vector3.zero;
        blockBody.angularVelocity = Vector3.zero;
        blockBody.transform.position = _positions[i];
        blockBody.transform.rotation = Quaternion.identity;
      }
    }
  }

  private IEnumerator DoPhraseCooldown() {
    float period = 0.2F;
    while (_ouchPhraseCooldown > 0F) {
      _ouchPhraseCooldown -= period;
      yield return new WaitForSecondsRealtime(period);
    }
  }

  private IEnumerator DoPlaySlideFXAfterDelay(float seconds) {
    yield return new WaitForSecondsRealtime(seconds);
    _signMovementAudioSource.PlayOneShot(_signMovementClips[Random.Range(0, _signMovementClips.Length)]);
  }

  #endregion

}

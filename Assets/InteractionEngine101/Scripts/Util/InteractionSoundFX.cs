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
using Leap;

/// <summary>
/// Statically loads collision and other interaction SFX and handles playing
/// those sound effects from a 3D AudioSource when attached to an InteractionBehaviour.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class InteractionSoundFX : MonoBehaviour {

  #region STATIC

  // Statically load and reference SFX AudioClips.

  private static bool s_audioInitialized = false;

  private static string s_grabFXPrefix = "LEAP_Box_Grab_Box_01_v_0";
  private const int NUM_GRAB_FX = 5;
  private static AudioClip[] s_grabFX = new AudioClip[NUM_GRAB_FX];

  private static string s_throwFXPrefix = "LEAP_Box_Throw_01_v_0";
  private const int NUM_THROW_FX = 5;
  private static AudioClip[] s_throwFX = new AudioClip[NUM_THROW_FX];

  private static string s_collisionFXPrefix = "LEAP_Box_Impact_Medium_01_v_0";
  private const int NUM_COLL_FX = 5;
  private static AudioClip[] s_collFX = new AudioClip[NUM_COLL_FX];

  #endregion

  #region PRIVATE FIELDS

  private AudioSource _audioSource;
  private InteractionBehaviour _interactionBehaviour;

  private bool collisionFXTimedOut = false;
  private float collisionFXTimeoutDuration = 0.1F;
  private float collisionFXTimeoutTimer = 0F;

  #endregion

  #region UNITY CALLBACKS

  protected virtual void Awake() {
    if (!s_audioInitialized) {
      for (int i = 0; i < NUM_GRAB_FX; i++) {
        s_grabFX[i] = Resources.Load<AudioClip>(s_grabFXPrefix + (i + 1));
      }
      for (int i = 0; i < NUM_THROW_FX; i++) {
        s_throwFX[i] = Resources.Load<AudioClip>(s_throwFXPrefix + (i + 1));
      }
      for (int i = 0; i < NUM_COLL_FX; i++) {
        s_collFX[i] = Resources.Load<AudioClip>(s_collisionFXPrefix + (i + 1));
      }
      s_audioInitialized = true;
    }
  }

  protected virtual void Start() {
    _audioSource = GetComponent<AudioSource>();
    _audioSource.spatialBlend = 1F;

    _interactionBehaviour = GetComponent<InteractionBehaviour>();
    if (_interactionBehaviour == null) {
      Debug.LogWarning("[InteractionSoundFX] No InteractionBehaviour attached; won't be able to play grasp or throw SFX.");
    }
    else {
      _interactionBehaviour.OnGraspBeginEvent += OnGraspBegin;
      _interactionBehaviour.OnGraspEndEvent += OnGraspEnd; 
    }

  }

  protected virtual void Update() {
    if (collisionFXTimedOut) {
      collisionFXTimeoutTimer += Time.deltaTime;
      if (collisionFXTimeoutTimer >= collisionFXTimeoutDuration) {
        collisionFXTimedOut = false;
        collisionFXTimeoutTimer = 0F;
      }
    }
  }

  protected virtual void OnCollisionEnter(Collision collision) {
    if (!collisionFXTimedOut) {

      // Play a sound when we hit something.
      float collisionSpeed = collision.relativeVelocity.magnitude;
      float collisionFXVolume = Mathf.Lerp(0F, 0.8F, (collisionSpeed - 0.5F) / 13F);

      // If the other collider is also an InteractionSoundFX, cut the volume of the FX in half
      // (Lazy/imperfect way to prevent doubly-loud audio on InteractionSoundFX-InteractionSoundFX collisions.)
      if (collision.gameObject.GetComponent<InteractionSoundFX>() != null) {
        collisionFXVolume /= 2F;
      }
      _audioSource.PlayOneShot(s_collFX[Random.Range(0, NUM_COLL_FX - 1)], collisionFXVolume);

      // Start the collision FX timeout
      collisionFXTimedOut = true;
      collisionFXTimeoutTimer = 0F;
    }
  }

  #endregion

  #region INTERACTION BEHAVIOUR CALLBACKS

  protected void OnGraspBegin() {
    // Play a sound when an object is picked up.
    _audioSource.PlayOneShot(s_grabFX[Random.Range(0, NUM_GRAB_FX - 1)], 0.05F);
  }

  protected void OnGraspEnd() {
    // Play a throw effect when an object is release with a high enough velocity.
    float throwSpeed = this.GetComponent<Rigidbody>().velocity.magnitude;
    _audioSource.PlayOneShot(s_throwFX[Random.Range(0, NUM_THROW_FX - 1)], Mathf.Lerp(0F, 0.7F, (throwSpeed - 1.5F) / 15F));
  }

  #endregion

  #region PUBLIC METHODS

  public void PlayThrowSFX() {
    float throwSpeed = this.GetComponent<Rigidbody>().velocity.magnitude;
    _audioSource.PlayOneShot(s_throwFX[Random.Range(0, NUM_THROW_FX - 1)], Mathf.Lerp(0F, 0.7F, (throwSpeed - 1.5F) / 15F));
  }

  #endregion

}

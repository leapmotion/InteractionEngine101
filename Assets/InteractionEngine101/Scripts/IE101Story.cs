/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;

/// <summary>
/// Handles progression logic for the Interaction Engine 101.
/// </summary>
public class IE101Story : MonoBehaviour {

  #region PUBLIC FIELDS

  [Tooltip("The LMHeadMountedRig object that anchors the player's VR position. This is moved at the beginning of the scene to be appropriate.")]
  public Transform _headMountedRig;

  [Tooltip("The ToggleInteractionEngine object to be used to enable and disable the Interaction Engine at runtime.")]
  public InteractionEngineToggle _toggleIE;

  [Tooltip("The DialogueText text element used for the assistant to convey primary dialogue to the player.")]
  public GradualText _dialogueText;

  [Tooltip("The object whose colliders represent the ground floor of the scene. Used for example success detection logic.")]
  public GameObject _groundObj;

  [Header("Interfaces")]
  public Slideable[] introInterface;
  public Slideable[] primaryInterface;

  [Header("Neon IE Status Sign")]
  public Renderer _neonIEText;
  public Renderer _neonIEOnText;
  public Renderer _neonIEOffText;
  public Material _ieEnabledFontMaterial;
  public Material _ieDisabledFontMaterial;

  [Header("Lighting")]
  public bool _startWithLightsOn = false;
  public Light[] _primaryLights;
  [Tooltip("The material of the object that obstructs the view at the beginning of the scene, used to produce a fade-in effect.")]
  public Material _fadeImageMaterial;
  public MassEnableDisable _spotlightHalosEnableDisable;

  [Header("Audio")]
  public AudioSource _bgTrackAudioSource;
  public AudioClip _bgTrack;
  public AudioClip _lightsOnFX;
  public AudioSource _interfaceSlideAudioSource;
  public AudioClip _beginButtonSlideUpFX;
  public AudioClip _primaryInterfaceSlideDownFX;

  [Header("Spawning")]
  public BlockSpawner _blockSpawner;
  public XZRadiusBoundary _respawnBoundary;
  public Transform _ceilingSpawn;

  [Header("Examples")]
  public IE101Example[] _examples;
  public GradualText _exampleControlText;
  public PhysicalButton _nextExampleButton;
  public ToggleableLight _nextExampleButtonLight;
  public PhysicalButton _previousExampleButton;
  public ToggleableLight _previousExampleButtonLight;

  #endregion

  #region CONSTANTS

  private const float VR_WAIT_TO_MOVE_DURATION = 1F;     // wait X seconds before re-aligning the VR anchor based on the current HMD position
  private const float BEGINNING_FADE_IN_DURATION = 0.5F; // seconds to fade in
  private const float CEILING_SPAWN_RADIUS = 0.1F;       // spawn blocks randomly within this spherical radius

  #endregion

  #region PRIVATE FIELDS

  // Intro fading / camera placement
  private bool _prepVRAnchorOnStart = true;
  private bool _skipIntroDialogue = false;

  // Exercises
  private int _exampleIdx = -1;

  // Neon sign
  private bool _isIENeonTextVisible = false;

  #endregion

  #region PROPERTIES

  public int CurrentExampleIndex {
    get {
      return _exampleIdx;
    }
    private set {
      _exampleIdx = Mathf.Clamp(value, -1, _examples.Length - 1);
    }
  }

  public IE101Example CurrentExample {
    get {
      if (CurrentExampleIndex == -1) {
        return null;
      }
      return _examples[CurrentExampleIndex];
    }
  }

  public XZRadiusBoundary RespawnBoundary {
    get { return _respawnBoundary; }
  }

  #endregion

  #region UNITY CALLBACKS

  protected void Start() {
    BlackoutCamera();
    TurnLightsOff();
    if (_prepVRAnchorOnStart) {
      StartCoroutine(WaitThenResetVRAnchor());
    }
    else {
      InitPlayground();
    }
    CheckNextExerciseButtonState();
    CheckPreviousExerciseButtonState();
    IENeonTextUseEnabledColor();

    if (_bgTrackAudioSource != null && _bgTrack != null) {
      _bgTrackAudioSource.loop = true;
      _bgTrackAudioSource.clip = _bgTrack;
      _bgTrackAudioSource.Play();
    }
  }

  #endregion

  #region PRIVATE METHODS

  private void BlackoutCamera() {
    _fadeImageMaterial.color = new Color(8 / 255F, 8 / 255F, 8 / 255F, 1F);
  }

  private void InitPlayground() {
    if (_startWithLightsOn) {
      TurnLightsOn();
    }
    else {
      TurnLightsOff();
    }
    FadeIn();
  }

  private void FadeIn() {
    StartCoroutine(FadeInCoroutine());
  }

  private void CheckNextExerciseButtonState() {
    if (CurrentExampleIndex >= _examples.Length - 1) {
      DeactivateNextExampleButton();
    }
    else {
      ActivateNextExampleButton();
    }
  }

  private void CheckPreviousExerciseButtonState() {
    if (CurrentExampleIndex <= 0) {
      DeactivatePreviousExampleButton();
    }
    else {
      ActivatePreviousExampleButton();
    }
  }

  private void ActivateNextExampleButton() {
    _nextExampleButton.EnableButtonFunctionality();
    _nextExampleButtonLight.TurnLightOn();
  }

  private void DeactivateNextExampleButton() {
    _nextExampleButton.DisableButtonFunctionality();
    _nextExampleButtonLight.TurnLightOff();
  }

  private void ActivatePreviousExampleButton() {
    _previousExampleButton.EnableButtonFunctionality();
    _previousExampleButtonLight.TurnLightOn();
  }

  private void DeactivatePreviousExampleButton() {
    _previousExampleButton.DisableButtonFunctionality();
    _previousExampleButtonLight.TurnLightOff();
  }

  private void BeginCurrentExercise() {
    CurrentExample.enabled = true;
    if (CurrentExample.GetType().ToString() == "AllDone") {
      _exampleControlText.SetGradualText("That's it!");
    }
    else {
      _exampleControlText.SetGradualText("Current example:\n" + CurrentExample.ToString());
    }
  }

  private void EnsureIENeonTextVisible() {
    if (!_isIENeonTextVisible) {
      StartCoroutine(FlashIENeonText());
    }
  }

  private void TurnOffIENeonText() {
    _neonIEText.enabled = false;
    _neonIEOnText.enabled = false;
    _neonIEOffText.enabled = false;
  }

  private void TurnOnIENeonText() {
    _neonIEText.enabled = true;
    _neonIEOnText.enabled = true;
    _neonIEOffText.enabled = true;
  }

  #endregion

  #region STORY COROUTINES

  private IEnumerator FadeInCoroutine() {
    while (_fadeImageMaterial.color.a > 0.05F) {
      _fadeImageMaterial.color = Color.Lerp(_fadeImageMaterial.color, new Color(8 / 255F, 8 / 255F, 8 / 255F, 0F), Time.deltaTime / BEGINNING_FADE_IN_DURATION);
      yield return null;
    }
    _fadeImageMaterial.color = new Color(8 / 255F, 8 / 255F, 8 / 255F, 0F);
  }

  private IEnumerator WaitThenResetVRAnchor() {
    yield return new WaitForSecondsRealtime(VR_WAIT_TO_MOVE_DURATION);
    UnityEngine.VR.InputTracking.Recenter();
    InitPlayground();
  }

  private IEnumerator DoIntroDialogue() {
    string[] lines = new string[] {
      "Hi!",
      "Welcome to <b>Interaction Engine 101</b>!",
      "Here you can learn what the Interaction Engine does, and how it makes interacting with virtual objects a breeze!",
      "For each example, you can enable and disable the Interaction Engine at will, using the button on your right.",
      "When the Interaction Engine is <b>disabled</b>, your hands will simply consist of <b>kinematic rigidbodies</b>.",
      "Kinematic rigidbody hands may cause blocks to jitter or unexpectedly achieve explosive velocities.",
      "You'll find this makes interacting with objects more difficult! That's why we built the Interaction Engine.",
      "When you're ready to begin, hit Next on the panel to your left."
    };
    float[] lineTimes = new float[] {
      3F, 3F, 5F, 6F, 6F, 6F, 5F, 3F
    };
    for (int i = 0; i < lines.Length; i++) {

      if (i == 3) {
        EnsureIENeonTextVisible();
      }
      if (i == 4) {
        _toggleIE.DisableInteractionEngine();
      }
      if (i == 7) {
        _toggleIE.EnableInteractionEngine();
      }

      SetDialogueText(lines[i]);
      yield return new WaitForSecondsRealtime(lineTimes[i]);
      if (_skipIntroDialogue) {
        EnsureIENeonTextVisible();
        break;
      }
    }
  }

  private IEnumerator FlashIENeonText() {
    int numFlashes = 3;
    float offDuration = 1F;
    float onDuration = 1F;
    for (int i = 0; i < numFlashes; i++) {
      TurnOffIENeonText();
      yield return new WaitForSecondsRealtime(offDuration);
      TurnOnIENeonText();
      yield return new WaitForSecondsRealtime(onDuration);
    }
  }

  private IEnumerator PlayBeginButtonSlideUpFXAfterDelay(float seconds) {
    yield return new WaitForSecondsRealtime(seconds);
    if (_interfaceSlideAudioSource != null && _beginButtonSlideUpFX != null) {
      _interfaceSlideAudioSource.PlayOneShot(_beginButtonSlideUpFX);
    }
  }

  private IEnumerator PlayPrimaryInterfaceSlideDownFXAfterDelay(float seconds) {
    yield return new WaitForSecondsRealtime(seconds);
    if (_interfaceSlideAudioSource != null && _primaryInterfaceSlideDownFX != null) {
      _interfaceSlideAudioSource.PlayOneShot(_primaryInterfaceSlideDownFX);
    }
  }

  #endregion

  #region PUBLIC METHODS

  public void TurnLightsOff() {
    for (int i = 0; i < _primaryLights.Length; i++) {
      _primaryLights[i].enabled = false;
    }
    _spotlightHalosEnableDisable.DisableComponents();
  }

  public void TurnLightsOn() {
    for (int i = 0; i < _primaryLights.Length; i++) {
      _primaryLights[i].enabled = true;
    }
    _spotlightHalosEnableDisable.EnableComponents();

    _bgTrackAudioSource.PlayOneShot(_lightsOnFX);
  }

  public void BeginPlayground() {
    // Slide intro interface up.
    for (int i = 0; i < introInterface.Length; i++) {
      introInterface[i].StartSlide(introInterface[i].transform.position, introInterface[i].transform.position + Vector3.up * 6F, 4F);
    }
    StartCoroutine(PlayBeginButtonSlideUpFXAfterDelay(0.125F));

    // Slide main interface down.
    for (int i = 0; i < primaryInterface.Length; i++) {
      primaryInterface[i].StartSlide(primaryInterface[i].transform.position, primaryInterface[i].transform.position + Vector3.down * 6F, 4F);
    }
    StartCoroutine(DoIntroDialogue());
    StartCoroutine(PlayPrimaryInterfaceSlideDownFXAfterDelay(1F));
  }

  public void SetDialogueText(string text) {
    _dialogueText.SetGradualText(text);
  }

  public void ResetCurrentExample() {
    if (CurrentExampleIndex == -1) {
      NextExample();
    }
    else if (CurrentExample != null) {
      CurrentExample.Respawn();
    }
  }

  public void NextExample() {
    if (CurrentExample != null) {
      CurrentExample.enabled = false;
    }
    CurrentExampleIndex += 1;
    BeginCurrentExercise();

    CheckNextExerciseButtonState();
    CheckPreviousExerciseButtonState();
  }

  public void PreviousExample() {
    if (CurrentExample != null) {
      CurrentExample.enabled = false;
    }
    CurrentExampleIndex -= 1;
    BeginCurrentExercise();

    CheckNextExerciseButtonState();
    CheckPreviousExerciseButtonState();
  }

  public GameObject SpawnCeilingBlock() {
    GameObject block = _blockSpawner.Spawn();
    block.transform.position = _ceilingSpawn.position + Random.insideUnitSphere * CEILING_SPAWN_RADIUS;
    block.transform.rotation = Random.rotation;
    return block;
  }

  public void SkipIntroDialogue() {
    _skipIntroDialogue = true;
  }

  public void IENeonTextUseEnabledColor() {
    _neonIEOnText.gameObject.SetActive(true);
    _neonIEOffText.gameObject.SetActive(false);
    _neonIEText.material = _ieEnabledFontMaterial;
    _neonIEOnText.material = _ieEnabledFontMaterial;
  }

  public void IENeonTextUseDisabledColor() {
    _neonIEOnText.gameObject.SetActive(false);
    _neonIEOffText.gameObject.SetActive(true);
    _neonIEText.material = _ieDisabledFontMaterial;
    _neonIEOffText.material = _ieDisabledFontMaterial;
  }

  #endregion

}

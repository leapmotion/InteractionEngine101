/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System;

/// <summary>
/// Instead of displaying text all at once, this Component provides SetGradualText,
/// which will reveal the provided text argument one character at a time.
/// 
/// The input to SetGradualText supports <b>bold brackets</b>, but no other text markup
/// (e.g. italics or coloring).
/// </summary>
public class GradualText : Text {

  // For some reason this doesn't show up in the inspector
  // so use SetAudio() to set these on startup
  public AudioSource _perCharFXSource;
  public AudioClip _perCharOneShotFX;
  public void SetAudio(AudioSource perCharFXSource, AudioClip perCharOneShotFX) {
    _perCharFXSource = perCharFXSource;
    _perCharOneShotFX = perCharOneShotFX;
  }
  private int _maxCharsPerSFX = 3;
  private int _minCharsPerSFX = 4;
  private int _chosenCharsPerSFX = 3; // randomly chosen between max and min
  private int _charsThisSFX = 0;

  private string _fullText;
  private bool _updatingText = false;
  private float _timePerChar = 0.02F;
  private float _textDisplayDuration = 0F;
  private float _textDisplayTimer = 0F;
  private int _displayedCharsLastFrame = 0;

  // Handling bold brackets <b>...</b>
  private string _origText; // includes markup
  private List<int> _boldOpens = new List<int>();
  private List<int> _boldCloses = new List<int>();

  /// <summary>
  /// Tracks angle bracket markup using internal state, and returns the original text without markup removed.
  /// Only works with <b>bold</b> tags.
  /// </summary>
  private string ParseOutMarkup(string origText) {
    _origText = origText;
    _boldOpens.Clear();
    _boldCloses.Clear();
    StringBuilder sb = new StringBuilder();
    int idxSkipOffset = 0;
    for (int i = 0; i < origText.Length; i++) {
      if (origText[i] == '<') {
        if (origText.Substring(i, 3) == "<b>") {
          _boldOpens.Add(i - idxSkipOffset);
          idxSkipOffset += 3;
          i += 2;
          continue;
        }
        else if (origText.Substring(i, 4) == "</b>") {
          _boldCloses.Add(i - idxSkipOffset);
          idxSkipOffset += 4;
          i += 3;
          continue;
        }
      }
      else {
        sb.Append(origText[i]);
      }
    }
    return sb.ToString();
  }

  private bool IsIdxWithinBold(int textIdx) {
    for (int i = 0; i < _boldOpens.Count; i++) {
      if (i < _boldCloses.Count && textIdx >= _boldOpens[i] && textIdx < _boldCloses[i]) {
        return true;
      }
    }
    return false;
  }

  public void SetGradualText(string fullText) {
    _updatingText = true;
    _fullText = ParseOutMarkup(fullText);
    _textDisplayDuration = _timePerChar * fullText.Length;
    _textDisplayTimer = 0F;
    _displayedCharsLastFrame = 0;
    base.text = "";
  }

  protected virtual void Update() {
    if (_updatingText) {
      _textDisplayTimer += Time.deltaTime;

      float textFraction = _textDisplayTimer / _textDisplayDuration;
      int numCharsToDisplay = (int)Mathf.Min(Mathf.Round(_fullText.Length * textFraction), _fullText.Length);

      if (numCharsToDisplay > _displayedCharsLastFrame) {

        // Will be displaying at least one new character, so play the per-character sound effect as a one-shot
        UpdatePerCharSFX();

        StringBuilder sb = new StringBuilder();

        // bold markup handling
        bool _inBold = false;
        for (int i = 0; i < numCharsToDisplay; i++) {
          if (IsIdxWithinBold(i)) {
            if (!_inBold) {
              sb.Append("<b>");
              _inBold = true;
            }
          }
          else if (_inBold) {
            sb.Append("</b>");
            _inBold = false;
          }

          // Append text per-character
          sb.Append(_fullText[i]);
        }
        if (_inBold) {
          sb.Append("</b>"); // still append bold closer if we're in bold at the end of the display range
        }

        // Display the rest of the characters invisibly (so centered position doesn't change)
        sb.Append("<color=#00000000>");
        if (_fullText.Length > 0) {
          if (_inBold) {
            sb.Append("<b>");
          }
          for (int i = numCharsToDisplay; i < _fullText.Length; i++) {
            if (IsIdxWithinBold(i)) {
              if (!_inBold) {
                sb.Append("<b>");
                _inBold = true;
              }
            }
            else if (_inBold) {
              sb.Append("</b>");
              _inBold = false;
            }
            sb.Append(_fullText[i]);
          }
        }
        sb.Append("</color>");
        base.text = sb.ToString();

        _displayedCharsLastFrame = numCharsToDisplay;

        if (numCharsToDisplay >= _fullText.Length) {
          base.text = _origText;
          _updatingText = false;
          _textDisplayDuration = 0F;
          _textDisplayTimer = 0F;
          _displayedCharsLastFrame = 0;
        }
      }
    }
  }

  protected void UpdatePerCharSFX() {
    _charsThisSFX += 1;
    if (_charsThisSFX == _chosenCharsPerSFX) {
      _perCharFXSource.PlayOneShot(_perCharOneShotFX, 0.25F);
      _charsThisSFX = 0;
      _chosenCharsPerSFX = UnityEngine.Random.Range(_minCharsPerSFX, _maxCharsPerSFX + 1);
    }
  }

  private string GetColorHexString(Color color) {
    return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
  }

}

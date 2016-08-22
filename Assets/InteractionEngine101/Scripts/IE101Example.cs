/******************************************************************************\
* Copyright (C) 2012-2016 Leap Motion, Inc. All rights reserved.               *
* Leap Motion proprietary and confidential. Not for distribution.              *
* Use subject to the terms of the Leap Motion SDK Agreement available at       *
* https://developer.leapmotion.com/sdk_agreement, or another agreement         *
* between Leap Motion and you, your company or other organization.             *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Text;

public abstract class IE101Example : MonoBehaviour {

  #region PUBLIC FIELDS

  public IE101Story _story;

  #endregion

  #region PRIVATE FIELDS

  private bool _hasBeenCompletedWithIE = false;
  private bool _hasBeenCompletedWithoutIE = false;

  #endregion

  #region PROPERTIES

  public bool IsExampleActive {
    get { return isActiveAndEnabled; }
  }

  public bool HasBeenCompletedWithIE {
    get { return _hasBeenCompletedWithIE; }
  }

  public bool HasBeenCompletedWithoutIE {
    get { return _hasBeenCompletedWithoutIE; }
  }

  #endregion

  #region EXAMPLE INTERFACE

  protected virtual void OnEnable() {
    _hasBeenCompletedWithIE = false;
    _hasBeenCompletedWithoutIE = false;
  }

  protected void Update() { }

  protected void FixedUpdate() { }

  public virtual void Respawn() {
    _hasBeenCompletedWithIE = false;
    _hasBeenCompletedWithoutIE = false;
  }

  protected virtual void OnDisable() { }

  protected virtual void OnComplete() {
    if (_story._toggleIE.IsInteractionEngineEnabled) {
      _hasBeenCompletedWithIE = true;
    }
    else {
      _hasBeenCompletedWithoutIE = true;
    }
  }

  #endregion

  #region PUBLIC METHODS

  public override string ToString() {
    return NameFromTypeName(this.GetType().Name);
  }

  #endregion

  #region PRIVATE METHODS

  /// <summary> e.g. "CamelCase" -> "Camel Case" </summary>
  private string NameFromTypeName(string typeName) {
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < typeName.Length; i++) {
      if (char.IsUpper(typeName[i]) || (i > 0 && !char.IsDigit(typeName[i - 1]) && char.IsDigit(typeName[i]))) {
        sb.Append(" ");
      }
      sb.Append(typeName[i]);
    }
    return sb.ToString();
  }

  #endregion

}

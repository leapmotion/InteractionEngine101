using UnityEngine;
using System.Collections;

public class PhysicalButtonCollisionCallback : MonoBehaviour {

  public PhysicalButton toCall;

  void OnCollisionEnter(Collision coll) {
    toCall.ReceiveCollisionEnter(coll, GetComponentInChildren<Collider>());
  }

}

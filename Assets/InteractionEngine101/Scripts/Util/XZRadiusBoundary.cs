using UnityEngine;
using System.Collections;

public class XZRadiusBoundary : MonoBehaviour {

  public float _radius = 0.45F;

  public bool IsInBounds(Vector3 worldPosition) {
    return (new Vector3(worldPosition.x, 0F, worldPosition.z) - new Vector3(this.transform.position.x, 0F, this.transform.position.z)).sqrMagnitude < (_radius * _radius);
  }

  // Draw a rough circle as a gizmo to indicate approximately what the respawn radius is in the scene.
  protected void OnDrawGizmos() {
    Gizmos.color = Color.magenta;
    float numSegments = 32;
    Vector3 center = this.transform.position;
    float radius = _radius;
    for (int i = 0; i < numSegments; i++) {
      Gizmos.DrawLine(center + Quaternion.AngleAxis((360F / numSegments) * i, Vector3.up) * Vector3.right * radius,
                      center + Quaternion.AngleAxis((360F / numSegments) * (i+1), Vector3.up) * Vector3.right * radius);
    }
  }

}

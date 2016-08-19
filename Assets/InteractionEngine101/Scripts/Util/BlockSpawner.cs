using UnityEngine;
using System.Collections.Generic;

public class BlockSpawner : MonoBehaviour {

  [Tooltip("A spawned block is instantiated randomly from this list.")]
  public GameObject[] _blockPrefabs;

  public float DefaultBlockMass { get { return 0.5F; } }

  private List<GameObject> _blocks = new List<GameObject>();

  public GameObject Spawn() {
    GameObject block = Instantiate(_blockPrefabs[Random.Range(0, _blockPrefabs.Length-1)]);
    var body = block.GetComponent<Rigidbody>();
    if (body != null) {
      body.mass = DefaultBlockMass;
    }
    _blocks.Add(block);
    return block;
  }

  public void DestroyAll() {
    for (int i = 0; i < _blocks.Count; i++) {
      GameObject toDestroy = _blocks[i];
      _blocks.RemoveAt(i);
      Destroy(toDestroy);
    }
  }

}

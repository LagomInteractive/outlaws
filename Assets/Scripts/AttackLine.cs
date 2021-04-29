using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackLine : MonoBehaviour {
    LineRenderer line;
    public LayerMask layermask;
    public Vector3 offset;
    void Start() {
        line = GetComponent<LineRenderer>();
    }

    public void Draw(Vector3 start, Vector3 end) {
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    public void StopDrawing() {
        for (int i = 0; i < 2; i++) line.SetPosition(i, Vector3.zero);
    }

    void Update() {

    }
}

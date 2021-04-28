using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class CardTitleText : MonoBehaviour {

    public TMP_Text text;
    public AnimationCurve curve;

    void Start() {

    }


    void Update() {

        text.ForceMeshUpdate();
        TMP_TextInfo info = text.textInfo;

        float start = 3f;
        float end = 481f - start;
        float dist = end - start;

        foreach (TMP_CharacterInfo character in info.characterInfo) {
            if (!character.isVisible) continue;

            Vector3[] verts = info.meshInfo[character.materialReferenceIndex].vertices;
            float y = curve.Evaluate((verts[character.vertexIndex].x - start) / dist);
            for (int i = 0; i < 4; i++) {
                verts[character.vertexIndex + i] = verts[character.vertexIndex + i] + new Vector3(0, y * 100, 0);
            }
        }
        for (int i = 0; i < info.meshInfo.Length; i++) {
            TMP_MeshInfo meshInfo = info.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            text.UpdateGeometry(meshInfo.mesh, i);
        }

    }
}

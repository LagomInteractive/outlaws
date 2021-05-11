using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour {
    bool extended = false;
    Vector3 originalPosition;




    public void OnPointerEnter() {
        if (!extended) {
            extended = true;
            StartCoroutine(MoveOverSeconds(originalPosition + new Vector3(30f, 0, 0), .1f));
        }
    }


    public void OnPointerExit() {
        extended = false;
        StartCoroutine(MoveOverSeconds(originalPosition, .1f));
    }

    public IEnumerator MoveOverSeconds(Vector3 end, float seconds) {
        float elapsedTime = 0;
        Vector3 startingPos = transform.localPosition;
        while (elapsedTime < seconds) {
            transform.localPosition = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    void Start() {
        originalPosition = transform.localPosition;
    }
}

using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XPUpdatePackage {
    public float xpFrom, xpTo, xpRange, level, xpGained;
}

public class XPDisplay : MonoBehaviour {

    public Text currentLevel, nextLevel, xpGained;
    public Slider xpBar;

    XPUpdatePackage package;
    float animationTimeSeconds = 1;

    /// <summary>
    /// Used for on connection lost
    /// </summary>
    public void ShowNoXpGain() {
        xpBar.value = 0;
        xpGained.text = "0+ xp";
        nextLevel.text = "";
        currentLevel.text = "";
    }

    public void XpUpdate(string json) {
        package = JsonConvert.DeserializeObject<XPUpdatePackage>(json);
        currentLevel.text = "Level " + package.level;
        nextLevel.text = "Level " + (package.level + 1);

        StartCoroutine(AnimateProgress());
    }

    public void SetGainedXp(float percentage) {
        xpGained.text = $"{Mathf.CeilToInt(package.xpGained * percentage)}+ xp";
        xpBar.value = Mathf.Lerp(package.xpFrom / package.xpRange, package.xpTo / package.xpRange, (percentage));
    }

    public IEnumerator AnimateProgress() {
        float elapsedTime = 0;

        while (elapsedTime <= animationTimeSeconds) {
            SetGainedXp(elapsedTime / animationTimeSeconds);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        SetGainedXp(1);
    }

}

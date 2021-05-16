using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Tips : MonoBehaviour {

    public CosmicAPI api;
    public Text body, title, category, number;
    public Slider progressBar;

    public VideoPlayer videoPlayer;

    Tip[] tips;

    float slideTime = 20; // Seconds

    public int activeTip = 0;
    float progress = 0;

    void Start() {
        tips = api.GetTips();
        LoadTip();
    }



    public void CycleTip(int amount) {
        progress = 0;

        activeTip += amount;
        if (activeTip < 0) activeTip = tips.Length - 1;
        activeTip = activeTip % tips.Length;

        videoPlayer.time = 0;

        LoadTip();
    }

    void LoadTip() {
        Tip tip = tips[activeTip];
        videoPlayer.clip = Resources.Load<VideoClip>("tips/" + tip.content);
        videoPlayer.time = 0;
        videoPlayer.Play();

        title.text = tip.title;
        body.text = tip.body;
        category.text = tip.category;
        number.text = $"Tip #{tip.number}";
    }

    void Update() {
        progress += Time.deltaTime;
        if (progress > slideTime) {
            CycleTip(1);
        }
        progressBar.value = progress / slideTime;
    }
}

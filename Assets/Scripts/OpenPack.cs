using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenPack : MonoBehaviour {

    public CosmicAPI api;
    public Color32 activePackDisabled;

    public RawImage activePackImage;
    public RenderTexture devPackVideo;
    public Button openPack, next, previous;
    public Text packName;

    int activePack = 0;

    private void OnEnable() {
        if (api.IsLoggedIn()) ViewPack();
    }

    private void Start() {
        next.onClick.AddListener(() => CyclePack(1));
        previous.onClick.AddListener(() => CyclePack(-1));
    }

    void CyclePack(int cycleAmount) {
        int totalPacks = api.GetPacks().Length;
        activePack += cycleAmount;
        if (activePack < 0) activePack = totalPacks - 1;
        activePack = activePack % totalPacks;
        ViewPack();
    }

    void ViewPack() {
        Pack pack = api.GetPacks()[activePack];
        Profile me = api.GetProfile();
        int inventoryOfPack = me.packs.ContainsKey(pack.id) ? me.packs[pack.id] : 0;
        bool canOpenPack = inventoryOfPack > 0;

        if (pack.id != "7LjVkr2TS0baaZkJ_xmAy") {
            activePackImage.texture = Resources.Load<Texture>("packs/" + pack.id);
        } else {
            // This pack is the Dev pack, this one is special because of the
            // transparent video that playes instead of the pack image.
            activePackImage.texture = devPackVideo;
        }

        openPack.interactable = canOpenPack;
        activePackImage.color = canOpenPack ? new Color32(255, 255, 255, 255) : activePackDisabled;
        packName.text = $"{inventoryOfPack}x {pack.name}";
    }

    void Update() {

    }
}

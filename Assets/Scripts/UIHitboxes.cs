using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Hitbox {
    Player, Opponent, Sacrifice, None
}

public class UIHitboxes : MonoBehaviour {

    public Hitbox active;
    public Hitbox last;

    public Hitbox GetActiveUIHitbox() {
        return active;
    }

    public Hitbox GetLastHitbox() {
        return last;
    }

    public void EnterHitbox(string name) {
        switch (name) {
            case "player":
                active = Hitbox.Player;
                break;
            case "opponent":
                active = Hitbox.Opponent;
                break;
            case "sacrifice":
                active = Hitbox.Sacrifice;
                break;
        }
        last = active;
    }

    public void ExitHitbox(string name) {
        active = Hitbox.None;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : PoolableObject
{
    [SerializeField] private SpriteRenderer sr;

    public void ChangeColour(string tag)
    {
        // Red = Enemy
        // Blue = Friendly
        // Yellow = Collectable
        // Green = Interactable/Objective
        // Purple = Upgrades
        // Orange = 
        // Cyan = Energy (HP)

        switch (tag)
        {
            case "Enemy": sr.color = Color.red; break;
            case "Friendly": sr.color = Color.blue; break;
            case "Collectable": sr.color = Color.yellow; break;
            case "Interactable": sr.color = Color.green; break;
            case "Upgrade": sr.color = Color.magenta; break;         
            case "Health": sr.color = Color.cyan; break;
            default: sr.color = Color.white; break;
        }
    }
}

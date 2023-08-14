using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int hp = 100;
    public int maxHP = 100;
    public bool canMove = true;
    public bool isDead = false;

    [Header("UI")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI maxHealthText;
    public Slider hpSlider;

    public Transform GetTransform()
    {
        return transform;
    }

    public void Heal(int heal)
    {
        hp += heal;

        if (hp >= maxHP)
        {
            hp = maxHP;
        }
    }

    public void TakeDamage(int damage)
    {
        //Debug.Log("OOF! - " + damage);

        hp -= Mathf.CeilToInt(damage);
        if(hp <= 0)
        {
            hp = 0;
            canMove = false;
            isDead = true;
            //gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        canMove = true;
        isDead = false;
    }

    public void Update()
    {
        // Draw UI
        //DrawUI();

        if (isDead)
            return;
    
    }

    public void DrawUI()
    {
        hpSlider.value = hp;
        hpSlider.maxValue = maxHP;
        healthText.text = hp.ToString();
        maxHealthText.text = maxHP.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMagicSystem : MonoBehaviour
{
    /*[Header("References")]
    public PlayerStats playerStats;
    public Camera cam;
    public Transform attackPoint;
    public Transform meleePoint;
    public Rigidbody playerRb;

    [Header("Spells")]
    public Spell spell;
    public SpellScriptableObject[] primarySpellList = new SpellScriptableObject[10];
    public ObjectPool[] primarySpellPoolList = new ObjectPool[10];
    public SpellScriptableObject[] secondarySpellList = new SpellScriptableObject[10];
    public ObjectPool[] secondarySpellPoolList = new ObjectPool[10];
    public int spellIndex;
    public bool castingMagic = false;

    Vector3 viewPoint = new Vector3(0.5f, 0.5f, 0);
    Vector3 targetPoint;
    Vector3 directionWithoutSpread;
    Vector3 directionWithSpread;
    RaycastHit hit;

    [Header("MP")]
    public float manaRechargeRate = 0.02f;
    [SerializeField] private float[] allMaxMP = new float[11];
    [SerializeField] private float[] allMP = new float[11];

    [Header("Input")]
    public KeyCode primaryFireKey = KeyCode.Mouse0;
    public KeyCode secondaryFireKey = KeyCode.Mouse1;
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode[] numKeys = new KeyCode[10];

    // Start is called before the first frame update
    void Start()
    {
        FillMP();
        CreateSpellPool();
        SwapSpells(0);  // Use 1st spell in slot
    }

    // Update is called once per frame
    void Update()
    {
        // Stop particles (if any)
        // Do when either key is held up OR if MP runs out OR if player cannot move (stunned or dead)
        if ((Input.GetKeyUp(primaryFireKey) || !HasMP(element1, manaCost1) || !playerStats.canMove) && fireParticles1 != null && fireParticles1.isPlaying)
            fireParticles1.Stop();
        if ((Input.GetKeyUp(secondaryFireKey) || !HasMP(element2, manaCost2) || !playerStats.canMove) && fireParticles2 != null && fireParticles2.isPlaying)
            fireParticles2.Stop();

        if (!playerStats.canMove)
            return;

        // Primary Fire
        if(!castingMagic && Input.GetKey(primaryFireKey) && HasMP(element1, manaCost1) && playerStats.canMove)
        {
            castingMagic = true;
            CastSpell(primarySpellPoolList, element1, manaCost1, spellSpread1, spellSpeed1, upwardForce1, hitBoxScale1, fireParticles1);
            Invoke(nameof(ResetCast), timeBetweenCasts1);
        }

        // Secondary Fire
        if (!castingMagic && Input.GetKey(secondaryFireKey) && HasMP(element2, manaCost2) && playerStats.canMove)
        {
            castingMagic = true;
            CastSpell(secondarySpellPoolList, element2, manaCost2, spellSpread2, spellSpeed2, upwardForce2, hitBoxScale2, fireParticles2);
            Invoke(nameof(ResetCast), timeBetweenCasts2);
        }

        // Swap between Spells
        for (int i = 0; i < numKeys.Length; i++)
        {
            if (Input.GetKeyUp(numKeys[i]))
            {
                SwapSpells(i);
                break;
            }
                
        }

        RefillMP();
    }

    public void FillMP()
    {
        for(int i = 0; i < allMaxMP.Length; i++)
        {
            allMP[i] = allMaxMP[i] = 100f;
        }
    }

    public void RefillMP()
    {
        for (int i = 0; i < allMaxMP.Length; i++)
        {
            allMP[i] += manaRechargeRate * Time.deltaTime;

            if (allMP[i] > allMaxMP[i])
                allMP[i] = allMaxMP[i];
        }
    }

    public void CreateSpellPool()
    {
        for (int i = 0; i < primarySpellPoolList.Length; i++)
        {
            // Ignore empty spells
            if (primarySpellList[i] == null)
                continue;

            Spell s = primarySpellList[i].SetUpSpell(gameObject);
            if (primarySpellPoolList[i] == null)
                primarySpellPoolList[i] = ObjectPool.CreateInstance(s, (int)(s.lifeTime / s.timeBetweenCasts * 2));
        }

        for (int i = 0; i < secondarySpellPoolList.Length; i++)
        {
            // Ignore empty spells
            if (secondarySpellList[i] == null)
                continue;

            Spell s = secondarySpellList[i].SetUpSpell(gameObject);
            if (secondarySpellPoolList[i] == null)
                secondarySpellPoolList[i] = ObjectPool.CreateInstance(s, (int)(s.lifeTime / s.timeBetweenCasts * 2));
        }
    }

    public void CastSpell(ObjectPool[] spellPoolList, ElementTypes element, int manaCost, float spellSpread, float spellSpeed, float upwardForce, Vector3 hitBoxScale, ParticleSystem particles)
    {
        // If there's no spell at spellIndex, ignore
        if(spellPoolList[spellIndex] == null)
        {
            Debug.Log("NO SPELL TO USE");
            castingMagic = false;
            return;
        }

        // Instantiate Bullet from Pool
        PoolableObject po = spellPoolList[spellIndex].GetObject();
        if (po != null)
        {
            spell = po.GetComponent<Spell>();
            spell.ApplyEffectsOnUse();
            spell.transform.localScale = hitBoxScale;
        }

        // Ranged
        if(spell.spellType == SpellTypes.ranged)
        {
            // Find exact hit point
            Ray ray = cam.ViewportPointToRay(viewPoint);

            /*if (Physics.Raycast(ray, out hit))
                targetPoint = hit.point;
            else
                targetPoint = ray.GetPoint(75); ///

            targetPoint = ray.GetPoint(75);

            // Get direction between viewPoint and attackPoint;
            directionWithoutSpread = targetPoint - attackPoint.position;

            // Calculate spread (if any)
            directionWithSpread = directionWithoutSpread + new Vector3(Random.Range(-spellSpread, spellSpread), Random.Range(-spellSpread, spellSpread), 0);

            spell.transform.position = attackPoint.position;
            spell.transform.forward = directionWithSpread;
            spell.damageMultiplier = playerStats.GetTrueAttack();

            // Launch bullet
            spell.GetComponent<Rigidbody>().AddForce(directionWithoutSpread.normalized * spellSpeed, ForceMode.VelocityChange);
            if (upwardForce != 0)
                spell.rb.AddForce(cam.transform.up * upwardForce, ForceMode.Impulse);
        }
        // Melee
        else if(spell.spellType == SpellTypes.melee)
        {
            //Vector3 aaa = meleePoint.position + new Vector3(1, 1, hitBoxScale.z);
            meleePoint.localPosition = new Vector3(0, 0.2f, hitBoxScale.z);
            spell.transform.position = meleePoint.position;
            spell.damageMultiplier = playerStats.GetTrueAttack();
            spell.Attack();
        }
        // Spray/Flamethrower
        else if (spell.spellType == SpellTypes.spray)
        {
            //meleePoint.localPosition = new Vector3(0, 0.2f, hitBoxScale.z);
            //spell.transform.position = meleePoint.position;
            spell.transform.position = attackPoint.position;
            spell.damageMultiplier = playerStats.GetTrueAttack();
            spell.Attack();

            if(!particles.isPlaying)
                particles.Play();
        }
        // AOE
        else
        {
            spell.transform.position = transform.position;
            spell.damageMultiplier = playerStats.GetTrueAttack();
            spell.Explode();
        }     

        allMP[(int)element] -= manaCost;
    }

    private void ResetCast()
    {
        castingMagic = false;
    }

    public void SwapSpells(int index)
    {
        if(fireParticles1 != null && fireParticles1.isPlaying)
            fireParticles1.Stop();
        if (fireParticles2 != null && fireParticles2.isPlaying)
            fireParticles2.Stop();

        spellIndex = index;

        if (primarySpellList[index] != null)
        {
            SpellScriptableObject sso1 = primarySpellList[spellIndex];
            element1 = sso1.element;
            timeBetweenCasts1 = sso1.timeBetweenCasts;
            spellSpread1 = sso1.spread;
            spellSpeed1 = sso1.speed;
            manaCost1 = sso1.manaCost;
            upwardForce1 = sso1.upwardForce;
            hitBoxScale1 = sso1.hitBoxScale;
            if (sso1.fireParticles != null)
                fireParticles1.GetComponent<ParticleSystemRenderer>().material.SetTexture("_MainTex", sso1.fireParticles);
        }

        if (secondarySpellList[index] != null)
        {
            SpellScriptableObject sso2 = secondarySpellList[spellIndex];
            element2 = sso2.element;
            timeBetweenCasts2 = sso2.timeBetweenCasts;
            spellSpread2 = sso2.spread;
            spellSpeed2 = sso2.speed;
            manaCost2 = sso2.manaCost;
            upwardForce2 = sso2.upwardForce;
            hitBoxScale2 = sso2.hitBoxScale;
            
            if(sso2.fireParticles != null)
                fireParticles2.GetComponent<ParticleSystemRenderer>().material.SetTexture("_MainTex", sso2.fireParticles);
        }
            
    }

    public bool HasMP(ElementTypes element, int manaCost)
    {
        return (allMP[(int)element] > manaCost);
    }*/
}

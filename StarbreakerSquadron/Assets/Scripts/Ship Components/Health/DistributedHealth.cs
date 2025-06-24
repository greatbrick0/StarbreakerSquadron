using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class DistributedHealth : SmallHealth
{
    [SerializeField]
    private List<SmallHealth> distributionTargets = new List<SmallHealth>();
    private int distributionCount = 0;
    [SerializeField]
    private int leftOverDamage = 0;

    private void Start()
    {
        distributionCount = distributionTargets.Count;
        foreach (SmallHealth ii in distributionTargets)
        {
            ii.deathEvent.AddListener(UpdateDistributionCount);
        }

        if (IsServer) currentHealth.Value = maxHealth;
    }

    public override void TakeDamage(int amount)
    {
        if (!IsServer) return;
        if(!isAlive) return;
        if (amount < 0) return;

        amount += leftOverDamage;

        if(distributionCount <= 0)
        {
            leftOverDamage = 0;
            currentHealth.Value -= amount;
        }
        else
        {
            foreach(SmallHealth ii in distributionTargets)
            {
                ii.TakeDamage(Mathf.FloorToInt(amount / distributionCount));
            }
            if (distributionCount > 0) leftOverDamage = amount % distributionCount;
            else leftOverDamage = 0;
        }

        print(GetTotalHealth());
        timeSinceLastDamage = 0f;
    }

    public void UpdateDistributionCount()
    {
        distributionCount -= 1;
    }

    public int GetTotalHealth()
    {
        int output = GetHealth();
        foreach(SmallHealth ii in distributionTargets) output += ii.GetHealth();
        return output;
    }
}

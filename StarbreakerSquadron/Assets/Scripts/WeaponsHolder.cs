using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponsHolder : MonoBehaviour
{
    [Serializable]
    public class WeaponSlot
    {
        [SerializeField]
        public float remainingCooldown = 0f;
        [SerializeField]
        public UnityEvent<WeaponSlot> activateEvent;

        public void Activate()
        {
            activateEvent.Invoke(this);
        }

        public void SetCooldown(float newTime)
        {
            remainingCooldown = Math.Max(remainingCooldown, newTime);
        }
    }

    public byte inputActives = 0;
    [SerializeField]
    private List<WeaponSlot> slots = new List<WeaponSlot>(4);

    private void Update()
    {
        for(int ii = 0; ii < slots.Count; ii++)
        {
            if (slots[ii].remainingCooldown > 0f) slots[ii].remainingCooldown -= Time.deltaTime;
            else if ((inputActives & (1 << ii)) > 0) slots[ii].Activate();
        }
    }
}

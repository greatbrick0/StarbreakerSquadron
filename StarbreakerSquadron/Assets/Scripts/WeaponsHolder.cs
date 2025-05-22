using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class WeaponsHolder : MonoBehaviour
{
    [Serializable]
    public class WeaponSlot
    {
        [Display]
        public float remainingCooldown = 0f;
        [SerializeField]
        public List<GameObject> activesObjs = new List<GameObject>();
        private List<IActivatable> actives = new List<IActivatable>();

        public void Activate()
        {
            foreach (IActivatable weapon in actives) weapon.Activate();
            SetCooldown();
        }

        private void SetCooldown()
        {
            remainingCooldown = actives.Max(weapon => weapon.GetCooldown());
        }

        public void InitializeActives()
        {
            foreach (GameObject obj in activesObjs)
            {
                actives.Add(obj.GetComponent<IActivatable>());
            }
        }
    }

    [Display]
    public byte inputActives = 0;
    [SerializeField]
    private List<WeaponSlot> slots = new List<WeaponSlot>(4);

    private void Awake()
    {
        foreach (WeaponSlot weapon in slots)
        {
            weapon.InitializeActives();
        }
    }

    private void Update()
    {
        for(int ii = 0; ii < slots.Count; ii++)
        {
            if (slots[ii].remainingCooldown > 0f) slots[ii].remainingCooldown -= Time.deltaTime;
            else if ((inputActives & (1 << ii)) > 0) slots[ii].Activate();
        }
    }
}

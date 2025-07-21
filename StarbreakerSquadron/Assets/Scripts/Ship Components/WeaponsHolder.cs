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
        [field: SerializeField]
        public bool showCooldown { get; private set; } = false;
        [field: SerializeField]
        public float shownMaximum { get; private set; } = 0f;
        [SerializeField]
        private bool cycle = false;
        private int cycleIndex = 0;
        [SerializeField]
        public List<GameObject> activesObjs = new List<GameObject>();
        private List<IActivatable> actives = new List<IActivatable>();

        public void Activate()
        {
            if (cycle)
            {
                actives[cycleIndex].Activate();
                SetCooldown();
                cycleIndex += 1;
                cycleIndex %= actives.Count;
            }
            else
            {
                foreach (IActivatable weapon in actives) weapon.Activate();
                SetCooldown();
            }
        }

        private void SetCooldown()
        {
            if(cycle) remainingCooldown = actives[cycleIndex].GetCooldown();
            else remainingCooldown = actives.Max(weapon => weapon.GetCooldown());
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
            if (slots[ii].activesObjs.Count == 0) continue;
            else if (slots[ii].remainingCooldown > 0f) slots[ii].remainingCooldown -= Time.deltaTime;
            else if ((inputActives & (1 << ii)) > 0) slots[ii].Activate();
        }
    }
}

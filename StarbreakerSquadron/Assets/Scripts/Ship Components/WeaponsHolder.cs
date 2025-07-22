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
        public delegate void OnActivated(float newCooldown);
        public OnActivated onActivated;

        [Display]
        public float remainingCooldown = 0f;
        [field: SerializeField]
        public bool showCooldown { get; private set; } = false;
        [field: SerializeField]
        public float shownMaximum { get; private set; } = 0f;
        [SerializeField]
        private Color cooldownColour = Color.cyan;
        [SerializeField]
        private bool cycle = false;
        private int cycleIndex = 0;
        [SerializeField]
        public List<GameObject> activesObjs = new List<GameObject>();
        private List<IActivatable> actives = new List<IActivatable>();

        public void Activate()
        {
            if (onActivated != null) onActivated(GetCooldown());

            if (cycle)
            {
                actives[cycleIndex].Activate();
                remainingCooldown = GetCooldown();
                cycleIndex += 1;
                cycleIndex %= actives.Count;
            }
            else
            {
                foreach (IActivatable weapon in actives) weapon.Activate();
                remainingCooldown = GetCooldown();
            }
        }

        public float GetCooldown()
        {
            if (cycle) return actives[cycleIndex].GetCooldown();
            else return actives.Max(weapon => weapon.GetCooldown());
        }

        public void InitializeActives()
        {
            foreach (GameObject obj in activesObjs)
            {
                actives.Add(obj.GetComponent<IActivatable>());
            }
        }

        public Color GetCooldownColour()
        {
            return cooldownColour;
        }
    }

    [Display]
    public byte inputActives = 0;
    [field: SerializeField]
    public List<WeaponSlot> slots { get; private set; } = new List<WeaponSlot>(4);

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

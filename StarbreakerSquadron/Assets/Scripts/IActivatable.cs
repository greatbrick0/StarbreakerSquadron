using System;
using UnityEngine;

public interface IActivatable
{
    public void Activate();

    public float GetCooldown();
}

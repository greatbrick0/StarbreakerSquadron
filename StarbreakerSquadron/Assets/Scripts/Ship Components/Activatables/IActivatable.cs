using System;
using UnityEngine;

public interface IActivatable
{
    public void Activate();

    public void Preview()
    {

    }

    public float GetCooldown();
}

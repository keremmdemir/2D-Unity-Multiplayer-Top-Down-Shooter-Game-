using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShootable
{
    void TakeDamage(int damag, Vector2 hitPosition,Vector2 hitNormal, uint killerNetId);
}

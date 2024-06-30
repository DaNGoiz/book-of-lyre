using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ground check for player and trigger landing event;
/// </summary>
public class PlayerFeet : OnGroundSensor
{
    public override bool IsOnGround()
    {
        Debug.DrawRay(transform.position, new Vector2(0f, -checkRadius), Color.red);
        otherColliders = Physics2D.OverlapCircleAll(transform.position, checkRadius, LayerMask.GetMask(DataBase.LayerName.mainLayerName));
        foreach (Collider2D otherCollider in otherColliders)
        {
            //owner.StandingOnPlatform = otherCollider.GetComponent<Platform>();
            if (!Physics2D.GetIgnoreCollision(owner.GetComponent<Collider2D>(), otherCollider))
            {
                Ground g = otherCollider.GetComponent<Ground>();
                owner.mFricFact += g == null ? 0f : g.extraFric;//Only add friction to owner when the ground has "Ground" script
                (owner as PlayerController).isLockJumping = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        //owner.StandingOnPlatform = null;
        return false;
    }
}

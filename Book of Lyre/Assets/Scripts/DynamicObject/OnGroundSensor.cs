using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ground check for all dynamic objects
/// </summary>
public class OnGroundSensor : MonoBehaviour
{
    public DynamicObject owner;
    protected const float checkOffset = 0.65f;
    protected const float checkRadius = 0.25f;
    protected Collider2D[] otherColliders;

    public virtual bool IsOnGround()
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

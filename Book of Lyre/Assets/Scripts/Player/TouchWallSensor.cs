using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchWallSensor : MonoBehaviour
{
    public PlayerController owner;
    protected const float checkDistance = 0.7f;
    public bool IsPushingWall()
    {
        Debug.DrawRay(transform.position, new Vector2(owner.mOrientation * checkDistance, 0f), Color.green);

        LayerMask layer = LayerMask.GetMask(DataBase.LayerName.mainLayerName);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(owner.mOrientation, 0f), checkDistance, layer);
        Vector2 perp = Vector2.Perpendicular(hit.normal);
        owner.wallNormalPerp = perp;

        return owner.GetComponent<Collider2D>().IsTouchingLayers(layer) && hit;
    }
}

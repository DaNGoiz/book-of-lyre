using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    public Collider2D collider;
    public float elasticForce;
    public void OnCollisionEnter2D(Collision2D collision)
    {
        DynamicObject obj = collision.gameObject.GetComponent<DynamicObject>();
        if (obj && collision.transform.position.y > ((Vector2)collider.transform.position + collider.offset).y)
        {
            obj.SetSpeed(y: 0f);
            obj.Accelerate("Spring", new Vector2(0f, elasticForce), Physics.Speed.Limitation.YOnly, elasticForce);
        }
    }
}

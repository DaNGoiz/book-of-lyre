using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Physics;
using static Physics.Speed;

/// <summary>
/// Base for all movable objects
/// </summary>
public class DynamicObject : MonoBehaviour
{
    protected const float SlopeCheckDistance = 1f;

    //Status
    public Speed mSpeed;
    public Speed mOldSpeed;
    public Vector2 mScale;
    public Vector2 mOldScale;
    public List<Acceleration> mAccelerations;
    public List<ObjectAction> mActions;

    #region Position state
    public bool mIsGrounded;
    [HideInInspector]
    public bool mWasGrounded;
    [HideInInspector]
    public bool mWasAtCeiling;
    [HideInInspector]
    public bool mIsAtCeiling;
    #endregion

    public Rigidbody2D mRig;
    public Collider2D mBody;
    public PlayerFeet mFeet;
    public TouchWallSensor mHand;

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        //Set X speed to 0 if hitting wall
        if (!mIsGrounded &&
            1 << collision.gameObject.layer == LayerMask.GetMask(DataBase.LayerName.mainLayerName))
        {
            mSpeed.Set(0f, mSpeed.value.y);
        }
    }

    #region Physics
    [HideInInspector]
    //The friction acceleration provided by the ground
    public float mFricFact;

    //public Transform center;

    [HideInInspector]
    //Horizontal moving direction(return 1 When moving rightward)
    public float mHorDirection;
    [HideInInspector]
    //Vertical moving direction(return 1 When moving upward)
    public float mVerDirection;

    protected float slopeMaxAngle = 45f;
    [HideInInspector]
    public Vector2 slopeNormalPerp;
    [HideInInspector]
    public Vector2 slopeNormal;
    [HideInInspector]
    public float slopeDownAngle;
    [HideInInspector]
    public float slopeSideAngle;
    [HideInInspector]
    public float slopeDownAngleOld;
    [HideInInspector]
    public float slopeSideAngleOld;
    [HideInInspector]
    public bool mWasOnSlope;
    [HideInInspector]
    public bool mCanWalkOnSlope;
    public bool mIsOnSlope;
    #endregion

    protected virtual void DefaultInit()
    {
        mSpeed = new Speed();
        mAccelerations = new List<Acceleration>();
        mActions = new List<ObjectAction>();
    }
    /// <summary>
    /// Update dynamicObjects' physics and status; calculating frame speed and initialize accelerations; applying to transform
    /// </summary>
    protected virtual void UpdatePhysics()
    {
        mOldSpeed = mSpeed;
        mOldScale = mScale;
        mWasGrounded = mIsGrounded;
        mWasOnSlope = mIsOnSlope;
        mWasAtCeiling = mIsAtCeiling;

        //Initialize accelerations
        mAccelerations = new List<Acceleration>();

        GetHorizontalDirection();
        GetVerticalDirection();
        mIsGrounded = mFeet.IsOnGround();
        SlopeCheck();

        //Gravity
        Accelerate("Gravity", new Vector2(0f, -Constants.cGravity), Limitation.YOnly, MaxSpeed.Gravity);
    }
    /// <summary>
    /// Used after updating objects' behavior
    /// </summary>
    protected virtual void ApplyPhysics()
    {
        //Calculate the friction acceleration separately
        float fricAccHorizontal = Mathf.Min(mFricFact, 1f) * -mHorDirection;
        float fricAccVertical = Mathf.Min(mFricFact, 1f) * -mVerDirection;

        //CollisionCast();

        //Apply to Transform and RigidBody2D
        transform.position += (Vector3)mSpeed.value * Time.fixedDeltaTime;
        transform.localScale = new Vector3(mScale.x, mScale.y, 1.0f);
    }
    /// <summary>
    /// Update object's invoking coroutine
    /// </summary>
    protected virtual void UpdateActions()
    {
        for (int i = 0; i < mActions.Count; i++)
        {
            ObjectAction action = mActions[i];
            if (action.isActive)
            {
                action.Update();
            }
        }
    }
    /// <summary>
    /// Check slope
    /// </summary>
    public void SlopeCheck()
    {
        Vector2 checkPos = mFeet.transform.position;
        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }
    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, Vector2.right, SlopeCheckDistance, LayerMask.GetMask(DataBase.LayerName.mainLayerName));
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -Vector2.right, SlopeCheckDistance, LayerMask.GetMask(DataBase.LayerName.mainLayerName));
        Debug.DrawRay(checkPos, new Vector2(SlopeCheckDistance, 0f), Color.blue);

        if (slopeHitFront)
        {
            mIsOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        }
        else if (slopeHitBack)
        {
            mIsOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0.0f;
            mIsOnSlope = false;
        }
    }
    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, SlopeCheckDistance, LayerMask.GetMask(DataBase.LayerName.mainLayerName));

        if (hit)
        {
            slopeNormal = hit.normal;
            slopeNormalPerp = Vector2.Perpendicular(slopeNormal).normalized;
            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);
            Debug.DrawRay(checkPos, slopeNormalPerp, Color.green);
            if (slopeDownAngle != slopeDownAngleOld)
            {
                mIsOnSlope = true;
            }

            slopeDownAngleOld = slopeDownAngle;
            if (slopeDownAngle > slopeMaxAngle || slopeSideAngle > slopeMaxAngle)
            {
                mIsOnSlope = false;
            }
        }
    }
    public void GetHorizontalDirection()
    {
        if (mSpeed.value.x == 0f)
            mHorDirection = 0f;
        else if (mSpeed.value.x > 0f)
            mHorDirection = 1f;
        else if (mSpeed.value.x < 0f)
            mHorDirection = -1f;
    }
    public void GetVerticalDirection()
    {
        if (mSpeed.value.y == 0f)
            mVerDirection = 0f;
        else if (mSpeed.value.y > 0f)
            mVerDirection = 1f;
        else if (mSpeed.value.y < 0f)
            mVerDirection = -1f;
    }
    /// <summary>
    /// Increase speed 
    /// </summary>
    /// <param name="source">The source of the acceleration</param>
    /// <param name="acceleration">Increment</param>
    /// <param name="limitation">Can be found in class "Physics.Speed"</param>
    /// <param name="maxSpeed">The max speed of this speed component, the overspeed part will be ignored. You can also refer to the constants in the struct MaxSpeed directly</param>
    public void Accelerate(string source, Vector2 acceleration, Func<Speed, Acceleration, float, Vector2> limitation, float maxSpeed = Mathf.Infinity)
    {
        Acceleration acc = new Acceleration(source, acceleration);
        mSpeed.Accelerate(limitation, acc, maxSpeed);
        mAccelerations.Add(acc);
    }
    public void SetSpeed(float x = 0f, float y = 0f)
    {
        mSpeed.Set(x, y);
    }
    public void SetSpeed(Vector2 speed)
    {
        mSpeed.Set(speed);
    }
}

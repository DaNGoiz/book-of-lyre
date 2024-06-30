using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Physics;
using static Physics.Constants;
using static Physics.PlayerConstants;
using static Physics.Speed;

/// <summary>
/// Player's logic and movment controller
/// </summary>
public class PlayerController : DynamicObject
{
    #region Player input
    /// <summary>
    /// Key effect
    /// </summary>
    public enum KeyInput
    {
        GoUp = 0,
        GoDown,
        GoLeft,
        GoRight,
        Jump,
        Count
    }

    //Get keys at the current and last frame
    public bool[] mInputs;
    public bool[] mPrevInputs;

    #region Check keys
    public bool Released(KeyInput key)
    {
        return !mInputs[(int)key] && mPrevInputs[(int)key];
    }

    public bool KeyState(KeyInput key)
    {
        return mInputs[(int)key];
    }

    public bool Pressed(KeyInput key)
    {
        return mInputs[(int)key] && !mPrevInputs[(int)key];
    }
    #endregion
    #endregion

    /// <summary>
    /// Player's Main states
    /// </summary>
    private enum MainState
    {
        Stand,
        Walk,
        Jump,
        ClimbWall,
        Slide,
        Delaying,
    }
    /// <summary>
    /// Player's Jumping states
    /// </summary>
    private enum JumpingState
    {
        None,
        Climb,
        HangTime,
        Fall
    }

    //Player's status
    [SerializeField]
    private MainState mCurrentMainState = MainState.Stand;
    [SerializeField]
    private JumpingState mCurrentJumpingState = JumpingState.None;
    [SerializeField]
    private Animator mAnimator;
    //public MainCamera mainCamara;

    [HideInInspector]
    public float mOrientation;
    private float climbDir;
    private float climbTime;

    private float mWalkAcc;
    private float mBrakeAcc;
    private float mJumpAcc;

    private bool isMoving;
    public bool isLockMoving;
    public bool isLockJumping;
    public bool isLockOrientation;

    public bool mIsPushingWall;
    [HideInInspector]
    public bool mWasPushingWall;
    [HideInInspector]
    public Vector2 wallNormalPerp;

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        //Set X speed to 0 if hitting wall
        if (!mIsGrounded &&
            1 << collision.gameObject.layer == LayerMask.GetMask(DataBase.LayerName.mainLayerName))
        {
            SetSpeed(0f, mSpeed.value.y);
        }
    }

    #region Physics
    /// <summary>
    /// Add acceleration according to ground state and player's orientation
    /// </summary>
    private void Move()
    {
        if (!isLockMoving)
        {
            if (mOrientation == 0f)
            {
                isMoving = false;
            }
            else
            {
                isMoving = true;
            }
        }
    }
    private void ClimbWall()
    {
        if (!isLockMoving)
        {
            climbDir = 0f;
            if (KeyState(KeyInput.GoUp))
            {
                climbDir = 1f;
                climbTime += Time.fixedDeltaTime;
            }
            else
            {
                if (KeyState(KeyInput.GoDown))
                {
                    climbDir = -1f;
                    climbTime += Time.fixedDeltaTime;
                }
                else
                {
                    climbTime = 0f;
                }
            }
        }
    }
    /*private void GrabWall()
    {
        mActions.Add(new ObjectAction(Grab(), true));
        IEnumerable Grab()
        {
            yield return
        }
    }*/
    private void Jump()
    {
        Accelerate("Jump", new Vector2(0f, mJumpAcc), Limitation.YOnly);
    }
    private void WallJump()
    {
        Accelerate("Jump", new Vector2(0f, mJumpAcc), Limitation.YOnly);
    }
    #endregion

    /// <summary>
    /// Update loop.Player's state determines what actions will be detected
    /// </summary>
    public void CharacterUpdate()
    {
        UpdatePhysics();
        UpdateActions();
        UpdateMainState();
        ApplyPhysics();
    }
    protected override void UpdatePhysics()
    {
        mOldSpeed = mSpeed;
        mOldScale = mScale;
        mWasGrounded = mIsGrounded;
        mWasOnSlope = mIsOnSlope;
        mWasAtCeiling = mIsAtCeiling;
        mWasPushingWall = mIsPushingWall;

        GetOrientation();
        GetHorizontalDirection();
        GetVerticalDirection();
        mIsGrounded = mFeet.IsOnGround();
        SlopeCheck();

        //Gravity
        if (!mIsGrounded)
        {
            Accelerate("Gravity", new Vector2(0f, -cGravity), Limitation.YOnly, MaxSpeed.Gravity);
        }
    }
    protected override void ApplyPhysics()
    {
        ApplyAcc();

        //CollisionCast();

        //Apply to Transform and RigidBody2D
        mRig.velocity = mSpeed.value;
        transform.localScale = new Vector3(mScale.x, mScale.y, 1.0f);

        //Initialize accelerations
        mFricFact = 1f;
        mAccelerations = new List<Acceleration>();

        speedX = mSpeed.value.x;
        speedY = mSpeed.value.y;

        void ApplyAcc()
        {
            Vector2 acc = new Vector2();
            foreach (Acceleration ac in mAccelerations)
            {
                acc += ac.value;
            }
            if (mIsGrounded)
            {
                if (mIsOnSlope)
                {
                    Vector2 v = acc * slopeNormal;
                    if (Vector2.SignedAngle(-slopeNormalPerp, v) > 0f)//Judge whether the resultant force is above the slope
                    {
                        mIsGrounded = false;
                        InTheAir();
                    }
                    else
                    {
                        OnSlope(acc);
                    }
                }
                else
                {
                    OnFlatGround(acc);
                }
            }
            else
            {
                if (mCurrentMainState == MainState.ClimbWall)
                {
                    ClimbWall();
                }
                else
                {
                    InTheAir();
                }
            }
            void InTheAir()
            {
                if (isMoving)
                {
                    Accelerate("Move", new Vector2(mWalkAcc * mOrientation, 0f), Limitation.XOnly, MaxSpeed.MoveMent);//还要考虑风的影响
                }
                else
                {
                    float newSpdX = mSpeed.value.x + (-mHorDirection * cAirFric);
                    if (mSpeed.value.x * newSpdX > 0f)
                    {
                        Accelerate("Friction", new Vector2(-mHorDirection * cAirFric, 0f), Limitation.NoLimitation, Mathf.Infinity);
                    }
                    else
                    {
                        SetSpeed(0f, mSpeed.value.y);
                    }
                }
            }
            void OnSlope(Vector2 acc)
            {
                if (!mWasGrounded)
                {
                    //改，去掉减速BUG；保持x速度不变，使新速度的x分量等于原本的x分量
                    SetSpeed(Mathf.Abs(mSpeed.value.x) * -mHorDirection * slopeNormalPerp);
                }
                Vector2 fricAcc = mBrakeAcc * Mathf.Min(mFricFact, 1f) * mHorDirection * slopeNormalPerp;
                if (isMoving)
                {
                    Accelerate("Move", -mOrientation * mWalkAcc * Mathf.Min(mFricFact, 1f) * slopeNormalPerp, Limitation.SpDirection, MaxSpeed.MoveMent);
                }
                else
                {
                    Vector2 newSpd = mSpeed.value + fricAcc;
                    if (Vector2.Angle(newSpd, mSpeed.value) != 0f)
                    {
                        SetSpeed(new Vector2());
                    }
                    else
                    {
                        Accelerate("Friction", fricAcc, Limitation.NoLimitation);
                    }
                }
            }
            void OnFlatGround(Vector2 acc)
            {
                SetSpeed(mSpeed.value.x, 0f);
                if (isMoving)
                {
                    Accelerate("Move", new Vector2(mWalkAcc * Mathf.Min(mFricFact, 1f) * mOrientation, 0f), Limitation.XOnly, MaxSpeed.MoveMent);
                }
                else
                {
                    float fricAcc = -mHorDirection * mBrakeAcc * Mathf.Min(mFricFact, 1f); 
                    float newSpdX = mSpeed.value.x + fricAcc;
                    if (newSpdX * mSpeed.value.x > 0f)
                    {
                        Accelerate("Friction", new Vector2(fricAcc, 0f), Limitation.NoLimitation, Mathf.Infinity);
                    }
                    else
                    {
                        SetSpeed(0f, mSpeed.value.y);
                    }
                }
            }
            void ClimbWall()
            {
                SetSpeed(new Vector2());
                if (climbDir != 0f)
                {
                    float climbSpeed = cCSpdFact * Mathf.Cos(climbTime * cCSpdChangeRate * Mathf.Rad2Deg) + cCSpdFact;
                    SetSpeed(Utility.Project(new Vector2(0f, climbDir * climbSpeed), wallNormalPerp));
                }
            }
        }
    }
    /// <summary>
    /// Update Player behaviour according to its main state
    /// </summary>
    private void UpdateMainState()
    {
        switch (mCurrentMainState)
        {
            case MainState.Stand:
                CheckMoving();
                if (CheckJumping()) goto case MainState.Jump;
                if (CheckClimbingWall()) break; 
                if (CheckFalling()) goto case MainState.Jump;
                Move();
                break;

            case MainState.Walk:
                if (CheckJumping()) goto case MainState.Jump;
                if (CheckClimbingWall()) goto case MainState.ClimbWall;
                if (CheckFalling()) goto case MainState.Jump;
                CheckStanding();
                Move();
                break;

            case MainState.Jump:
                CheckLanding(); 
                if (CheckClimbingWall()) break;
                UpdateJumingState();
                Move();
                break;

            case MainState.ClimbWall:
                if (CheckOffWall())
                {
                    if (mIsGrounded)
                    {
                        SwitchToMain(MainState.Stand);
                        goto case MainState.Stand;
                    }
                    else
                    {
                        SwitchToMain(MainState.Jump);
                        goto case MainState.Jump;
                    }
                }
                if (CheckWallJumping()) goto case MainState.Jump;
                ClimbWall();

                break;

            case MainState.Slide:
                break;

            case MainState.Delaying:
                break;

            default:
                throw new UnityException("Can't find the specific state");
        }
    }
    /// <summary>
    /// Switch main state to another one
    /// </summary>
    /// <param name="state">target state</param>
    private void SwitchToMain(MainState state)
    {
        switch (mCurrentMainState)
        {
            case MainState.Stand:
                break;
            case MainState.Walk:
                break;
            case MainState.Jump:
                mCurrentJumpingState = JumpingState.None;
                break;

            case MainState.ClimbWall:
                break;
            case MainState.Slide:
                isLockMoving = false;
                break;

            case MainState.Delaying:
                isLockMoving = false;
                break;
        }
        switch (state)
        {
            case MainState.Stand:

                mCurrentMainState = MainState.Stand;
                break;

            case MainState.Walk:

                mCurrentMainState = MainState.Walk;
                break;

            case MainState.Jump:

                mCurrentMainState = MainState.Jump;
                break;

            case MainState.ClimbWall:
                SetSpeed(new Vector2());
                isMoving = false;
                climbTime = 0f;
                climbDir = 0f;

                mCurrentMainState = MainState.ClimbWall;
                break;

            case MainState.Slide:
                isMoving = false;
                isLockMoving = true;

                mCurrentMainState = MainState.Slide;
                break;

            case MainState.Delaying:
                isMoving = false;
                isLockMoving = true;

                mCurrentMainState = MainState.Delaying;
                break;

            default:
                throw new UnityException($"Can't find state \"{state}\" ");
        }
    }

    #region Check actions
    private bool CheckStanding()
    {
        if (mSpeed.value.x == 0)
        {
            SwitchToMain(MainState.Stand);
            return true;
        }
        return false;
    }
    private bool CheckMoving()
    {
        if (mOrientation != 0)
        {
            SwitchToMain(MainState.Walk);
            return true;
        }
        return false;
    }
    private bool CheckJumping()
    {
        if (Pressed(KeyInput.Jump) && !isLockJumping)
        {
            SwitchToMain(MainState.Jump);
            mCurrentJumpingState = JumpingState.Climb;
            mIsGrounded = false;
            isLockJumping = true;
            Jump();
            return true;
        }
        return false;
    }
    private bool CheckWallJumping()
    {
        if (Pressed(KeyInput.Jump) && !isLockJumping)
        {
            SwitchToMain(MainState.Jump);
            mCurrentJumpingState = JumpingState.Climb;
            mIsGrounded = false;
            isLockJumping = true;
            WallJump();
            return true;
        }
        return false;
    }
    private bool CheckOffWall()
    {
        if (!mHand.IsPushingWall())
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private bool CheckFalling()
    {
        if (!mIsGrounded)
        {
            if (mSpeed.value.y < -18f)
            {
                SwitchToMain(MainState.Jump);
                isLockJumping = true;
                mCurrentJumpingState = JumpingState.Fall;
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Add accelerations to player according to its jumping state
    /// </summary>
    private void UpdateJumingState()
    {
        switch (mCurrentJumpingState)
        {
            case JumpingState.Climb:
                if (!(mSpeed.value.y > cCritSpdToHang))
                {
                    mCurrentJumpingState = JumpingState.HangTime;
                    goto case JumpingState.HangTime;
                }
                Accelerate("Climb adjustment", new Vector2(0f, cClimbAcc), Limitation.YOnly, Mathf.Infinity);
                break;

            case JumpingState.HangTime:
                if (!(mSpeed.value.y > cCritSpdToFall))
                {
                    mCurrentJumpingState = JumpingState.Fall;
                }
                Accelerate("Hang time adjustment", new Vector2(0f, cHangTimeAcc), Limitation.YOnly, Mathf.Infinity);
                break;

            default:
                break;
        }
    }
    private bool CheckLanding()
    {
        if (mIsGrounded)
        {
            SwitchToMain(MainState.Stand);
            mCurrentJumpingState = JumpingState.None;
            return true;
        }
        return false;
    }
    private bool CheckClimbingWall()
    {
        if (mIsPushingWall = mHand.IsPushingWall())
        {
            if (mIsGrounded)
            {
                Debug.Log("站立转爬墙");
            }
            SwitchToMain(MainState.ClimbWall);
            return true;
        }
        return false;
    }
    #endregion

    /// <summary>
    /// Player's Orientation， return 1 when pressing the key "Go Right"
    /// </summary>
    public void GetOrientation()
    {
        if (!isLockOrientation)
        {
            if (KeyState(KeyInput.GoRight) == KeyState(KeyInput.GoLeft))
                mOrientation = 0f;
            else if (KeyState(KeyInput.GoRight))
                mOrientation = 1f;
            else
                mOrientation = -1f;
        }
    }
    /// <summary>
    /// Update inputs at the last frame
    /// </summary>
    public void UpdatePrevInputs()
    {
        var count = (byte)KeyInput.Count;

        for (byte i = 0; i < count; ++i)
            mPrevInputs[i] = mInputs[i];
    }
    /// <summary>
    /// Initialize player
    /// </summary>
    /// <param name="inputs">inputs at the current frame</param>
    /// <param name="prevInputs">inputs at the last frame</param>
    public void CharacterInit(bool[] inputs, bool[] prevInputs)
    {
        //Set initial position

        //Apply inputs
        mInputs = inputs;
        mPrevInputs = prevInputs;

        //Set initial acceleration
        mWalkAcc = cWalkAcc;
        mBrakeAcc = cBrakeAcc;
        mJumpAcc = cJumpAcc;

        mSpeed = new Speed();
        mAccelerations = new List<Acceleration>();
        mActions = new List<ObjectAction>();

        //mScale = Vector2.one; 
        mScale = new Vector2(1f, 4f);
    }

    #region Debug
    public float speedX;
    public float speedY;
    #endregion
}

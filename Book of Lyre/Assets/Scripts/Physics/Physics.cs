using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DataBase;

/// <summary>
/// Store most constants for physic effect;
/// </summary>
public class Physics : MonoBehaviour
{
    public partial struct Constants
    {
        /// <summary>
        /// Default friction factor
        /// </summary>
        public const float cAirFric = 0.2f;
        public const float cGravity = 9f;
        public const float cWalkAcc = 4f;
        public const float cBrakeAcc = 2f;
        /// <summary>
        /// Jump instant force
        /// </summary>
        public const float cJumpAcc = 100f;
        /// <summary>
        /// Control Y speed when climbing after a jump
        /// </summary>
        public const float cClimbAcc = 4f;
        /// <summary>
        /// Control Y speed when hovering
        /// </summary>
        public const float cHangTimeAcc = 5f;
    }
    public partial struct Constants
    {
        /// <summary>
        /// Control speed amplitude
        /// </summary>
        public const float cCSpdFact = 7f;
        /// <summary>
        /// Control speed fluctuant rate
        /// </summary>
        public const float cCSpdChangeRate = 0.25f;
    }
    public struct PlayerConstants
    {
        public const float cMaxWalkingSpd = 24f;
        public const float cMaxFallingSpd = 60f;
        /// <summary>
        /// Critical speed between climbing and hang time
        /// </summary>
        public const float cCritSpdToHang = 50f;
        /// <summary>
        /// Critical speed between hang time and fall
        /// </summary>
        public const float cCritSpdToFall = -50f;
    }
    public class Acceleration
    {
        public string source;
        public Vector2 value;
        public Acceleration(string source, float x = 0f, float y = 0f)
        {
            this.source = source;
            value = new Vector2(x, y);
        }
        public Acceleration(string source, Vector2 value)
        {
            this.source = source;
            this.value = value;
        }
        public override string ToString()
        {
            return "Source: " + source + "  Value: " + value;
        }
    }
    public partial class Speed
    {
        /// <summary>
        /// Store constants of max speeds of different speed types
        /// </summary>
        public struct MaxSpeed
        {
            public const float Gravity = 60f;
            public const float MoveMent = 24f;
        }
        public struct Limitation
        {
            /// <summary>
            /// No limitation
            /// </summary>
            /// <param name="speed"></param>
            /// <param name="acc"></param>
            /// <param name="max"></param>
            /// <returns></returns>
            public static Vector2 NoLimitation(Speed speed, Acceleration acc, float max)
            {
                return speed.value + acc.value;
            }
            /// <summary>
            /// Used for Limitation. Only confine the speed when its project on a specified direction overspeeding.
            /// </summary>
            /// <param name="speed">Reference to Speed</param>
            /// <param name="acc">Acceleration</param>
            /// <param name="max">Maximum speed</param>
            /// <returns>New speed vector</returns>
            public static Vector2 SpDirection(Speed speed, Acceleration acc, float max)
            {
                //v1, v2 represent "speed" and "acc". Now the consultant speed's project on v2 is greater than "max".
                //v3 represent the modified speed, whose project on v2 equal to "max". The calculation is as follow.
                //kv2 + v1 = v3;
                //v3 * v2/|v2| = max
                //k * |v2| + v1 * v2/|v2| = max
                //k = (max - v1 * v2/|v2|) / |v2|
                //This is the better choice for computer to get the result faster
                float dot = Vector2.Dot(speed.value, acc.value.normalized);
                if (Mathf.Abs(dot) > max)
                {
                    decimal k = (decimal)(max - dot);
                    decimal mag = (decimal)acc.value.magnitude;
                    k /= mag;
                    return speed.value + (float)k * acc.value;
                }
                else
                    return speed.value + acc.value;
            }
            /// <summary>
            /// Used for Limitation. Only change the vector's direction rather than magnitude when overspeeding.
            /// </summary>
            /// <param name="speed">Reference to Speed</param>
            /// <param name="acc">Acceleration</param>
            /// <param name="max">Maximum speed</param>
            /// <returns>New speed vector</returns>
            public static Vector2 AllDirection(Speed speed, Acceleration acc, float max)
            {
                Vector2 newSpd = speed.value + acc.value;
                Vector2 maxSpd = acc.value.normalized * max;
                if (newSpd.sqrMagnitude > speed.value.sqrMagnitude)
                {
                    if (newSpd.sqrMagnitude > maxSpd.sqrMagnitude)
                    {
                        return newSpd.normalized * speed.value.magnitude;
                    }
                    else
                    {
                        return newSpd;
                    }
                }
                else
                {
                    return newSpd;
                }
            }
            /// <summary>
            /// Used for Limitation. Only confine X speed when overspeeding.
            /// </summary>
            /// <param name="speed">Reference to Speed</param>
            /// <param name="acc">Acceleration</param>
            /// <param name="max">Maximum speed on X-axis.</param>
            /// <returns>New speed vector</returns>
            public static Vector2 XOnly(Speed speed, Acceleration acc, float max)
            {
                Vector2 newSpd = speed.value + acc.value;
                if (newSpd.x * speed.value.x < 0f)
                {
                    if (Mathf.Abs(newSpd.x) > max)
                    {
                        return new Vector2(newSpd.x / Mathf.Abs(newSpd.x) * max, newSpd.y);
                    }
                    else
                    {
                        return newSpd;
                    }
                }
                else
                {
                    if (Mathf.Abs(newSpd.x) <= Mathf.Abs(speed.value.x))
                    {
                        return newSpd;
                    }
                    else
                    {
                        if (Mathf.Abs(newSpd.x) > max)
                        {
                            return new Vector2(newSpd.x / Mathf.Abs(newSpd.x) * max, newSpd.y);
                        }
                        else
                        {
                            return newSpd;
                        }
                    }
                }
            }
            /// <summary>
            /// Used for Limitation Only confine Y speed when overspeeding.
            /// </summary>
            /// <param name="speed">Reference to Speed</param>
            /// <param name="acc">Acceleration</param>
            /// <param name="max">Maximum speed on Y-axis.</param>
            /// <returns>New speed vector</returns>
            public static Vector2 YOnly(Speed speed, Acceleration acc, float max)
            {
                Vector2 newSpd = speed.value + acc.value;
                if (newSpd.y * speed.value.y < 0f)
                {
                    if (Mathf.Abs(newSpd.y) > max)
                    {
                        return new Vector2(newSpd.x, newSpd.y / Mathf.Abs(newSpd.y) * max);
                    }
                    else
                    {
                        return newSpd;
                    }
                }
                else
                {
                    if (Mathf.Abs(newSpd.y) <= Mathf.Abs(speed.value.y))
                    { 
                        return newSpd;
                    }
                    else
                    {
                        if (Mathf.Abs(newSpd.y) > max)
                        {
                            return new Vector2(newSpd.x, newSpd.y / Mathf.Abs(newSpd.y) * max);
                        }
                        else
                        {
                            return newSpd;
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// Speed applied to dynamic objects
    /// </summary>
    public partial class Speed
    {
        public Vector2 value;
        public Speed() { }
        public Speed(Vector2 initValue)
        {
            value = initValue;
        }
        /// <summary>
        /// Increase speed 
        /// </summary>
        /// <param name="speed">Increment</param>
        /// <param name="max">The max speed of this speed component, the overspeed part will be ignored.You can refer to the constants in the struct MaxSpeed directly</param>
        public void Accelerate(Func<Speed, Acceleration, float, Vector2> limitation, Acceleration acc, float max)
        {
            value = limitation.Invoke(this, acc, max);
        }
        public void Set(Vector2 speed)
        {
            value = speed;
        }
        public void Set(float x = 0f, float y = 0f)
        {
            value = new Vector2(x, y);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ObjectAction
{
    private readonly IEnumerator enumerator;
    public bool isActive;
    public bool canBeInterrupted;
    public ObjectAction(IEnumerable action, bool canBeInterrupted, bool isActive = true)
    {
        this.isActive = isActive;
        this.canBeInterrupted = canBeInterrupted;
        enumerator = action.GetEnumerator();
    }
    public bool Update()
    {
        if (isActive)
        {
            if (!enumerator.MoveNext())
            {
                return false;
            }
        }
        return true;
    }
}

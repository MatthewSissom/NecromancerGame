using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputManager : MonoBehaviour
{

    //holds 1 bone moving touch, which will be enabled and disabled as needed
    private BoneMovingTouch movingTouch;
    //holds 1 rotation proxy, which will be enabled and disabled as needed
    private RotationTouch rotationTouch;

    //holds a list of active touches
    private List<TouchProxy> activeTouches;
    private List<TouchProxy> toDeactivate;

    TouchProxy FindNearestActive(Vector3 pos)
    {
        float distance, currentDist;
        distance = (activeTouches[0].transform.position - pos).magnitude;
        TouchProxy toReturn = activeTouches[0];

        for(int i = 1; i < activeTouches.Count; i++)
        {
            currentDist = (activeTouches[i].transform.position - pos).magnitude;
            if (currentDist < distance)
            {
                toReturn = activeTouches[i];
                distance = currentDist;
            }
        }

        return toReturn;
    }

    public void DisableTouch(TouchProxy toDisable)
    {
        if (!toDisable.isActiveAndEnabled) return;

        if (toDisable == movingTouch&&rotationTouch)
            DisableTouch(rotationTouch);

        toDisable.gameObject.SetActive(false);
        realTouchCount--;
        if (activeTouches.Count == 1)
        {
            activeTouches.Clear();
            return;
        }
        activeTouches.Remove(toDisable);
        
    }

    void DisableTouch(Vector3 position)
    {
        DisableTouch(FindNearestActive(position));
    }

    TouchProxy NewMoveTouch(Vector3 position, int id)
    {
        BoneMovingTouch mTouch = movingTouch;

        activeTouches.Add(mTouch);
        mTouch.Move(position, 0);
        mTouch.gameObject.SetActive(true);
        mTouch.id = id;

        return mTouch;
    }

    TouchProxy NewRotationTouch(Vector3 position,int id, BoneMovingTouch parent)
    {
        RotationTouch mTouch = rotationTouch;

        activeTouches.Add(mTouch);

        mTouch.Parent = parent;
        mTouch.ResetTouch(position, 0);
        mTouch.gameObject.SetActive(true);
        mTouch.id = id;

        return mTouch;
    }

    void ArrayInit()
    {
        activeTouches = new List<TouchProxy>();
        movingTouch = new BoneMovingTouch();
        rotationTouch = new RotationTouch();
        toDeactivate = null;
        

        
        movingTouch = Instantiate(moveTouchPref).GetComponent<BoneMovingTouch>();
        movingTouch.gameObject.SetActive(false);

        rotationTouch = Instantiate(rotationTouchPref).GetComponent<RotationTouch>();
        rotationTouch.gameObject.SetActive(false);
        
    }

    private void ChangeHandedness(bool isRightHanded)
    {
        for (int i = 0; i < 1; i++)
        {
            // TEMP - make better offset options
            if (isRightHanded)
                movingTouch.offset.x *= -1;
        }
    }
}


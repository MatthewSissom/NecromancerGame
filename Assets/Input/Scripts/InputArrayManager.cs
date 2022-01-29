using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputManager : MonoBehaviour
{

    //holds 5 bone moving touches, which will be enabled and disabled as needed
    private int moveIndex;
    private BoneMovingTouch[] movingTouches;
    //holds 5 rotation proxies, which will be enabled and disabled as needed
    private int rotationIndex;
    private RotationTouch[] rotationTouches;

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

        if (toDisable is BoneMovingTouch)
        {
            moveIndex--;
        }
        else
        {
            rotationIndex--;
        }
        toDisable.gameObject.SetActive(false);

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
        BoneMovingTouch mTouch = movingTouches[moveIndex];
        moveIndex++;

        activeTouches.Add(mTouch);
        mTouch.Move(position, 0);
        mTouch.gameObject.SetActive(true);
        mTouch.id = id;

        return mTouch;
    }

    TouchProxy NewRotationTouch(Vector3 position,int id, BoneMovingTouch parent)
    {
        RotationTouch mTouch = rotationTouches[rotationIndex];
        rotationIndex++;

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
        movingTouches = new BoneMovingTouch[5];
        rotationTouches = new RotationTouch[5];
        toDeactivate = null;
        moveIndex = 0;
        rotationIndex = 0;

        for(int i = 0; i < 5; i++)
        {
            movingTouches[i] = Instantiate(moveTouchPref).GetComponent<BoneMovingTouch>();
            movingTouches[i].gameObject.SetActive(false);

            rotationTouches[i] = Instantiate(rotationTouchPref).GetComponent<RotationTouch>();
            rotationTouches[i].gameObject.SetActive(false);
        }
    }

    private void ChangeHandedness(bool isRightHanded)
    {
        for (int i = 0; i < 5; i++)
        {
            // TEMP - make better offset options
            if (isRightHanded)
                movingTouches[i].offset.x *= -1;
        }
    }
}


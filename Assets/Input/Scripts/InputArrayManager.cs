using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputManager : MonoBehaviour
{

    //holds a bone moving touch, which will be enabled and disabled as needed
    private BoneMovingTouch movingTouch;
    //holds a rotation proxy, which will be enabled and disabled as needed
    private RotationTouch rotationTouch;
    //holds a list of active touches
    private List<TouchProxy> activeTouches;
    private List<TouchProxy> toDeactivate;   

    TouchProxy FindNearestActive(Vector3 pos)
    {
        float distance, currentDist;
        distance = float.MaxValue;
        TouchProxy toReturn = null;

        for(int i = 0; i < activeTouches.Count; i++)
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

        if (toDisable == movingTouch && rotationTouch)
            DisableTouch(rotationTouch);

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
        BoneMovingTouch mTouch = movingTouch;

        activeTouches.Add(mTouch);
        mTouch.Move(position, 0);
        mTouch.gameObject.SetActive(true);
        mTouch.id = id;

        return movingTouch;
    }

    TouchProxy NewRotationTouch(Vector3 position,int id, BoneMovingTouch parent)
    {

        RotationTouch mTouch = rotationTouch;

        activeTouches.Add(mTouch);

        mTouch.Parent = parent;
        mTouch.ResetTouch(position, 0);
        mTouch.gameObject.SetActive(true);
        mTouch.id = id;
        mTouch.realUp = Camera.main.transform.forward;
        return rotationTouch;
    }

    void ArrayInit()
    {
        activeTouches = new List<TouchProxy>();
        toDeactivate = null;

        movingTouch = Instantiate(moveTouchPref).GetComponent<BoneMovingTouch>();
        movingTouch.gameObject.SetActive(false);

        rotationTouch = Instantiate(rotationTouchPref).GetComponent<RotationTouch>();
        rotationTouch.gameObject.SetActive(false);   
    }

    private void ChangeHandedness(bool isRightHanded)
    {
        
        // TEMP - make better offset options
        if (isRightHanded)
             movingTouch.offset.x *= -1;
        
#if UNITY_STANDALONE_WIN
        for (int i = 0; i < 5; i++)
        {
            movingTouch.offset = new Vector3(.001f,.001f,0);
        }
#endif
    }

    // Used for tutorial
    public bool Rotating() { return rotationTouch != null; }
}


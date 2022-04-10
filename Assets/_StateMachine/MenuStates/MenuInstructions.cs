using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInstructions : State
{
    [System.Serializable]
    class MenuItemTranslationData
    {
        public bool enabled = false;
        public Vector2 targetPos = default;
        public float duration = default;
        [HideInInspector]
        public Vector2 initalPos;
    } 
    
    [System.Serializable]
    class MenuItemRotationData
    {
        public bool enabled;
        public float finalRotation;
        public float duration;
        [HideInInspector]
        public float initalRotation;
    }

    [System.Serializable]
    class HandData
    {
        public RectTransform transform;
        public MenuItemTranslationData movement;
        public MenuItemRotationData rotation;

        public void Init()
        {
            rotation.initalRotation = transform.rotation.z;
            movement.initalPos = transform.anchoredPosition;
        }
    }

    public static MenuInstructions Instance { get; private set; } = null;

    public GameObject canvas;
    [SerializeField]
    private List<GameObject> insturctionObjects;
    [SerializeField]
    private GameObject failureObject;
    [SerializeField]
    private List<HandData> handMovementData;
    private bool exit;

    int instructionIndex;
    float handTime;

    public override IEnumerator Routine()
    {
        Begin();

        ResetVars();
        exit = false;
        while (!exit)
        {
            handTime += Time.deltaTime;
            AnimateHands();
            yield return null;
        }

        End();
        yield break;
    }

    public void ButtonPressed(string name)
    {
        MenuManager.Instance.GoToMenu("Main");
        exit = true;
    }

    public void GoToNextState()
    {
        exit = true;
    }

    public void ShowTutorialFailureScreen()
    {
        if (instructionIndex < insturctionObjects.Count - 1)
        {
            insturctionObjects[instructionIndex].SetActive(false);
        }
        failureObject.SetActive(true);
    }

    public void ShowInstruction(int index)
    {
        if (failureObject.activeInHierarchy)
        {
            failureObject.SetActive(true);
        }
        if(index < insturctionObjects.Count)
        {
            insturctionObjects[instructionIndex].SetActive(false);
            instructionIndex = index;
            insturctionObjects[instructionIndex].SetActive(true);
        }
    }

    //public void PreviousInstruction()
    //{
    //    if (instructionIndex > 0)
    //    {
    //        insturctionObjects[instructionIndex].SetActive(false);
    //        instructionIndex--;
    //        insturctionObjects[instructionIndex].SetActive(true);
    //    }
    //}

    private void AnimateHands()
    {
        foreach (HandData data in handMovementData)
        {
            // translate hand based on time if needed
            if (data.movement.enabled)
            {
                data.transform.anchoredPosition = Vector2.Lerp(
                    data.movement.initalPos,
                    data.movement.targetPos,
                    Mathf.PingPong(handTime / data.movement.duration, 1));
            }
            // rotate hand if needed
            if (data.rotation.enabled)
            { 
                data.transform.rotation = Quaternion.Euler(
                    new Vector3(
                        0,
                        0,
                        Mathf.PingPong(handTime / data.rotation.duration * Mathf.Abs(data.rotation.finalRotation), Mathf.Abs(data.rotation.finalRotation)) * Mathf.Sign(data.rotation.finalRotation)));
            }
        }
    }

    private void ResetVars()
    {
        handTime = 0;
    }

    private void Start()
    {
        if (Instance != null)
            Destroy(this);
        Instance = this;

        canvas.SetActive(false);

        //events
        MenuManager.Instance.AddEventMethod(typeof(MenuInstructions), "begin", () => { canvas.SetActive(true); });
        MenuManager.Instance.AddEventMethod(typeof(MenuMain), "begin", () => { canvas.SetActive(false); });
        GameManager.Instance.AddEventMethod(typeof(GameCleanUp), "end", () => { canvas.SetActive(false); });

        // array init
        for (int i = 0; i < insturctionObjects.Count; i++)
        {
            insturctionObjects[i].SetActive(false);
        }
        for(int i = 0; i < handMovementData.Count; i++)
        {
            handMovementData[i].Init();
        }
    }
}

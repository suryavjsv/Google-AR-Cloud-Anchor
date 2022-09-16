using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tyre : MonoBehaviour
{
    public GameObject tyreInfoUI;
    public Animator tyreAnimator;
    public Animator brakeAnimator;
    bool brakeTrigger = false;
    bool infoTrigger = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyBrakes()
    {
        if (!brakeTrigger)
        {
            brakeAnimator.Play("ApplyingBrake");
            tyreAnimator.Play("TyreRotationSpeed");
            brakeTrigger = true;

        }
        else if (brakeTrigger)
        {
            brakeAnimator.Play("ReleasingBrake");
            tyreAnimator.Play("TyreRotation");
            brakeTrigger = false;
        }

    }

    public void TyreInfo()
    {
        if (!infoTrigger)
        {
            tyreInfoUI.SetActive(true);
            infoTrigger = true;
        }
        else if(infoTrigger)
        {
            tyreInfoUI.SetActive(false);
            infoTrigger = false;
        }
    }
}

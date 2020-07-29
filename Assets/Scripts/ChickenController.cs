using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenController : MonoBehaviour
{
    [SerializeField]
    private Animator chickenAnimator;

    private Camera target;

    private bool IsGrazing;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        IsGrazing = true;
    }

    /*
     * Controller for chicken, it will always grazing unless a player get in a range, 
     * then it will chase player around until the player goes too far.
     */
    void Update()
    {
        if (IsGrazing)
        {
            chickenAnimator.Play("eat");
            IsGrazing = false;
        }
        if (chickenAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && !IsGrazing)
        {
            if (Vector3.Distance(chickenAnimator.transform.position, target.transform.position) < 12)
            {
                transform.LookAt(target.transform);
                if (Vector3.Distance(chickenAnimator.transform.position, target.transform.position) > 5)
                {
                    chickenAnimator.Play("run");
                    transform.position += transform.forward * 3 * Time.deltaTime;
                }
                else if (Vector3.Distance(chickenAnimator.transform.position, target.transform.position) > 2)
                {
                    chickenAnimator.Play("walk");
                    transform.position += transform.forward * 1 * Time.deltaTime;
                }
                else
                {
                    chickenAnimator.Play("idle");
                }
            }
            else
            {
                IsGrazing = true;
            }
        }
    }
}

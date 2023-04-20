using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PTestAnimatorManager : Singleton<PTestAnimatorManager>
{

    public Coroutine StartImageAnimation(Transform transform)
    {
       return StartCoroutine(ImageAnimation(transform));
    }


    public Coroutine StarImageTestingAnimation(Image imgTesting)
    {
       return StartCoroutine(ImageTestingAnimation(imgTesting));
    }

    public IEnumerator ImageAnimation(Transform transform)
    {
        while (true)
        {
            //imgStatus.transform.Rotate(new Vector3(90, 0, 0) * Time.deltaTime);
            transform.Rotate(0, 0, 60 * Time.deltaTime, Space.World);
            yield return new WaitForFixedUpdate();
        }

    }


    public IEnumerator ImageTestingAnimation(Image imgTesting)
    {
        float time = 0;
        while (true)
        {
            imgTesting.fillAmount = time;
            yield return new WaitForSecondsRealtime(0.1f);
            time += 0.1f;
            if (time >= 1)
            {
                time = 0;
            }
        }

    }
}

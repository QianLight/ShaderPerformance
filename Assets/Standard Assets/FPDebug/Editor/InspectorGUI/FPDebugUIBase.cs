using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface FPDebugUIBase
{
    // Start is called before the first frame update
    void OnEnable();

    // Update is called once per frame
    void OnDisable();

    bool OnInspectorGUI();
}

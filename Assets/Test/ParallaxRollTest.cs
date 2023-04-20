using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CFUI;
using UnityEngine.CFEventSystems;

public class XUIFlow : XParallaxRollItemBase
{

    //private CFText m_label = null;

    public override void SetIndex(int index)
    {
        base.SetIndex(index);
        // m_label = trans.Find("label").GetComponent<CFText>();
        // m_label.SetText(m_index.ToString());
        // trans.name = index.ToString();
    }

    public override void UnSelect()
    {
        base.UnSelect();
        // CFImage image = UIBehaviour.Get<CFImage>(trans);
        // image.SetColor(Color.black);
    }

    public override void Select()
    {
        base.Select();
        // CFImage image = UIBehaviour.Get<CFImage>(trans);
        // image.SetColor(Color.red);
    }
}
public class ParallaxRollTest : MonoBehaviour
{
    public XParallaxRoll m_flow = null;
    public int min = 0;
    public int max = 10;
    public int startIndex = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        if(m_flow == null)
        {
            m_flow = GetComponent<XParallaxRoll>();
        }

        if(m_flow != null)
        {
            m_flow.Register(startIndex, max , OnCreate);
        }
    
    }

    private XParallaxRollItemBase OnCreate(){
        return new XUIFlow();
    }




    // Update is called once per frame
    void Update()
    {
        
    }
}

#if UNITY_EDITOR
using EditorEcs;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XEcsGamePlay;

public class XEcsScriptEntity
{
    private static XEditorFacility _fcy;
    private static Dictionary<ulong, XEcsScriptEntity> _map = new Dictionary<ulong, XEcsScriptEntity>();

    private ulong _id;

    // Use this for initialization
    public static void Start ()
    {
        _fcy = new XEditorFacility();
        
        Xuthus_VirtualServer.beginSirius(_fcy);
        if(!XEcs.singleton.Init(new XEcsInterface()))
        {
            EditorUtility.DisplayDialog(
                "Error!",
                "The Ecs version DO NOT match with " + XEcs.singleton.Version() + "(c++) and " + (int)XVersion2Csp.Csp_Version + "(c#)",
                "Quit");

            EditorApplication.ExecuteMenuItem("Edit/Play");
        }
    }

    public static void Reload(ulong id, uint hash)
    {
        Xuthus_VirtualServer.reload(hash);
        XEcs.singleton.Reload(hash);
    }

    public static void Create(ulong id, uint skill = 0, float face = 0, float x = 0, float y = 0, float z = 0)
    {
        Xuthus_VirtualServer.create(id, 0, face, x, y, z);

        XEcs.singleton.Create(id, Vector3.zero, 0, 0, true);
        XEcs.singleton.OnCreated(id);

        _map.Add(id, new XEcsScriptEntity());
        _map[id]._id = id;

        if(skill > 0)
        {
            Xuthus_VirtualServer.bind_offslotskill(id, skill);
        }
    }

    public static void BindSkill(ulong id,uint skill)
    {
        Xuthus_VirtualServer.bind_offslotskill(id, skill);
    }

    public static void CreatePuppet(ulong id, float face = 0, float x = 0, float y = 0, float z = 0)
    {
        Xuthus_VirtualServer.create(id, 0, face, x, y, z);

        XEcs.singleton.Create(id, new Vector3(x, y, z), face, 0, true);
        XEcs.singleton.OnCreated(id);

        _map.Add(id, new XEcsScriptEntity());
        _map[id]._id = id;
    }

    public static void SetActionRatio(ulong id ,float ratio)
    {
        Xuthus_VirtualServer.setActionRatio(id, ratio);
    }

    public static void BindHit(ulong id, int slot, uint hit)
    {
        Xuthus_VirtualServer.bindhit(id, slot, hit);
    }

    public static void Destroy(ulong id)
    {
        Xuthus_VirtualServer.destroy(id);
        XEcs.singleton.Destroy(id);

        _map.Remove(id);
    }

    // Update is called once per frame
    public static void Update (float delta)
    {
        Xuthus_VirtualServer.update(delta);

        foreach (ulong id in _map.Keys)
        {
            _map[id].Sync();
        }

        XVirtualNet.singleton.Transfer();
        XVirtualNet.singleton.Receive();

        XEcs.singleton.Update(delta);
	}

    public static void PostUpdate()
    {
        XEcs.singleton.PostUpdate(Time.deltaTime);
    }

    public static void SetDebug()
    {
        XEcs.singleton.setDebug(true);
    }

    public static void Quit()
    {
        _fcy.Dispose();

        Xuthus_VirtualServer.endSirius();
        XEcs.singleton.Uninit();

        _map.Clear();
    }

    public static void SetDebug(bool flag)
    {
        XEcs.singleton.setDebug(flag);
    }

    public static void OnMove(ulong id, float forward, Vector3 ori)
    {
        Xuthus_VirtualServer.drive2dest(id, forward, ori.x, ori.y, ori.z);
        XEcs.singleton.Move(id, forward);
    }

    public static void OnStop(ulong id, float forward, Vector3 p)
    {
        Xuthus_VirtualServer.drive2stand(id, forward, p.x, p.y, p.z);
        XEcs.singleton.Idled(id);
    }

    public static void OnSkill(ulong id, ulong target = ulong.MaxValue)
    {
        Xuthus_VirtualServer.slot2skill(id, target, EditorEcs.XInputSlot.Normal_Slot, EditorEcs.XSkillType.ButtonDown);
    }

    public static void EndSkill(ulong id)
    {
        Xuthus_VirtualServer.endskill(id);
    }

    public void Sync()
    {
        if(Xuthus_VirtualServer.needsync(_id))
        {
            using (EditorEcs.XNet net = Xuthus_VirtualServer.sync(_id))
            {
                XVirtualNet.singleton.Send(_id, net);
            }
        }
    }
}
#endif
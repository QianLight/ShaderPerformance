#if UNITY_EDITOR
using CFUtilPoolLib;
using EditorEcs;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XEcsGamePlay;

public class XVirtualNet : XSingleton<XVirtualNet>
{
    public Queue<XNet> sender = new Queue<XNet>();
    public Queue<XNet> receiver = new Queue<XNet>();

    public override bool Init()
    {
        sender.Clear();
        receiver.Clear();
        return true;
    }

    public void Send(ulong id, EditorEcs.XNet net)
    {
        XNet data = new XNet();
        {
            data.id = id;
            data.lag = Time.time + GetLag();
            data.face = net.face;
            data.interpolation = net.interpolation;
            data.old_sequence = net.old_sequence;
            data.x = net.x;
            data.y = net.y;
            data.z = net.z;
            data.script_id = net.script_id;
            data.percentage = net.percentage;
            data.state = (XStateType)net.state;
            data.sync_sequence = net.sync_sequence;
            data.velocity = net.velocity;
            data.conditionseq = net.condition_seq;
            data.switchseq = net.switch_seq;
            data.whileseq = net.while_seq;
            data.skill_code = net.skill_code;
            data.action_ratio = net.action_ratio;
            data.hit_from = net.from_entt;
            data.hit_from_hash = net.from_hash;
            data.hit_from_index = net.from_index;
            data.hit_from_dir = net.from_dir;
            data.target = net.target;
            data.passive = net.passive;

            sender.Enqueue(data);
        }
    }

    public void Receive()
    {
        int count = 0;
        while(receiver.Count > 0)
        {
            XEcs.singleton.Sync(receiver.Dequeue());
            count++;

            if (count >= 50) break;
        }
    }

    // Update is called once per frame
    public void Transfer()
    {
        while (sender.Count > 0)
        {
            if(Time.time >= sender.Peek().lag)
            {
                receiver.Enqueue(sender.Dequeue());
            }
            else
            {
                break;
            }
        }
    }

    public float GetLag()
    {
        float lag = VirtualSkill.SkillHoster.GetHoster.Lag;
        float fluc = VirtualSkill.SkillHoster.GetHoster.Fluctuations;

        float flag = XCommon.singleton.RandomPercentage() > 0.5f ? 1 : -1;
        return lag + flag * XCommon.singleton.RandomPercentage() * fluc;
    }
}
#endif
#if UNITY_EDITOR
using CFEngine;
using CFUtilPoolLib;
using System.IO;
using UnityEngine;
using VirtualSkill;
using XEcsGamePlay;

public class XEcsInterface : XIEcsInterface
{
    public uint fetch_delay()
    {
        return (uint)(XVirtualNet.singleton.GetLag() * 1000);
    }

    public float fetchspeed (ulong id)
    {
        if (SkillHoster.GetHoster.moveSpeed != 0) return SkillHoster.GetHoster.moveSpeed;
        return SkillHoster.GetHoster.EntityDic[id].statisticsData == null ? 7 : SkillHoster.GetHoster.EntityDic[id].statisticsData.RunSpeed;
    }

    public float fetchrotatespeed (ulong id)
    {
        if (SkillHoster.GetHoster.rotateSpeed != 0) return SkillHoster.GetHoster.rotateSpeed;
        return SkillHoster.GetHoster.EntityDic[id].statisticsData == null ? 10 : SkillHoster.GetHoster.EntityDic[id].statisticsData.RotateSpeed;
    }

    public float fetchheight (ulong id)
    {
        return SkillHoster.GetHoster.EntityDic[id].presentData.BoundHeight * SkillHoster.GetHoster.EntityDic[id].presentData.Scale;
    }

    public float fetchradius (ulong id)
    {
        return SkillHoster.GetHoster.EntityDic[id].presentData.BoundRadius * SkillHoster.GetHoster.EntityDic[id].presentData.Scale;
    }

    public float fetch_cd (ulong id, uint skill)
    {
        return 0;
    }

    public int fetch_phase_count(ulong id, uint skill)
    {
        return 1;
    }

    public void push_skill_stringify(ulong id, uint hash)
    {
        string path = SkillHoster.GetHoster.GetScriptPath(hash);

        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            byte[] bytes = SimpleTools.FileStream2Bytes(fs);
            SimpleTools.Unlock(ref bytes, 0, (int)fs.Length);
            SimpleTools.Bytes2String(ref bytes, 0, (int)fs.Length);
            Xuthus.cacheSkillScript(hash, bytes);
        }
    }

    public void push_hit_stringify(ulong id, uint hash)
    {
        string path = SkillHoster.GetHoster.GetHitScriptPath(hash);

        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            byte[] bytes = SimpleTools.FileStream2Bytes(fs);
            SimpleTools.Unlock(ref bytes, 0, (int)fs.Length);
            SimpleTools.Bytes2String(ref bytes, 0, (int)fs.Length);
            Xuthus.cacheHitScript(hash, bytes);
        }
    }

    public bool is_spot_entt (ulong id)
    {
        return true;
    }

    public void onSync (ulong id, Vector3 pos, float face, bool force)
    {
        SkillHoster.GetHoster.SyncPos (id, pos, face);
    }

    public void on_node_change (ulong id, uint hash, int index)
    {
        SkillHoster.GetHoster.OnNodeChange (id, hash, index);
    }

    public void on_skill_begin (ulong id, uint hash)
    {
        SkillHoster.GetHoster.OnSkillBegin(id);
    }

    public void on_svr_skill_begin (ulong id, uint hash)
    {
        SkillHoster.GetHoster.Targets.Clear();
    }

    public void on_svr_skill_end(ulong id, uint hash, uint idx)
    {
        XEcs.singleton.OnSvrSkillEnd(id, hash, idx);
    }

    public void on_skill_end (ulong id)
    {
        SkillHoster.GetHoster.OnSkillEnd (id);
    }

    public void overrideAnimClip (ulong id, string motion, uint hash, int index)
    {
        SkillHoster.GetHoster.OverrideAnimClip (id, motion, hash, index);
    }

    public void playAu (ulong id, uint hash, int index)
    {
        SkillHoster.GetHoster.PlayAudio (id, hash, index);
    }

    public void playCameraLayerMask (ulong id, uint hash, int index)
    {
        SkillHoster.GetHoster.playCameraLayerMask (id, hash, index);
    }

    public void playCameraMotion (ulong id, uint hash, int index)
    {
        SkillHoster.GetHoster.playCameraMotion (id, hash, index);
    }

    public void playCameraPostEffect (ulong id, uint hash, int index)
    {
        SkillHoster.GetHoster.playCameraPostEffect (id, hash, index);
    }

    public void playCameraShake (ulong id, uint hash, int index)
    {
        SkillHoster.GetHoster.playCameraShake (id, hash, index);
    }

    public void playCameraStretch (ulong id, uint hash, int index)
    {
        SkillHoster.GetHoster.playCameraStretch (id, hash, index);
    }

    // public XFx playFx (ulong id, uint hash, int index, float hitDirection)
    // {
    //     return null; //SkillHoster.GetHoster.PlayFx (id, hash, index, hitDirection);
    // }
    public SFX playFx (ulong id, uint hash, int index, float hitDirection)
    {
        return SkillHoster.GetHoster.PlayFx (id, hash, index, hitDirection);
    }

    public SFX playFx(ulong id, uint hash, int index, Vector3 pos, float dir)
    {
        return SkillHoster.GetHoster.PlayFx(id, hash, index, pos, dir);
    }

    public void playShaderEffect (ulong id, uint hash, int index)
    {
        SkillHoster.GetHoster.playShaderEffect (id, hash, index);
    }

    public void prepareHit (ulong id)
    {

    }

    public void prepareIdle (ulong id)
    {
        SkillHoster.GetHoster.PrepareAnim (id, "idle", "Idle");
    }

    public void prepareMove (ulong id)
    {
        SkillHoster.GetHoster.PrepareAnim (id, "run", "Run");
    }

    public void prepareDeath (ulong id)
    {

    }

    public void prepareSkill (ulong id)
    {

    }

    // public XFx projectBullet (ulong id, uint hash, int index, Vector3 pos, float face)
    // {
    //     return null; //SkillHoster.GetHoster.ProjectBullet (id, hash, index, pos, face);
    // }

    public SFX projectBullet (ulong id, ulong bullet, uint hash, int index, Vector3 pos, float face)
    {
        return SkillHoster.GetHoster.ProjectBullet(id, bullet, hash, index, pos, face);
    }

    public void endBullet (uint hash, int index, Vector3 end)
    {

    }

    public void setAnimSpeed (ulong id, float speed)
    {
        SkillHoster.GetHoster.SetAnimSpeed (id, speed);
    }

    public void setAnimTrigger (ulong id, string trigger, int layer)
    {
        if (SkillHoster.GetHoster.GetAnimSpeed (id) < 0.618f && trigger == "EndSkill")
        {
            SkillHoster.GetHoster.SetAnimTrigger (id, "SubEndSkill");
        }
        else
            SkillHoster.GetHoster.SetAnimTrigger (id, trigger);
    }

    public bool has_buff (ulong uid, uint id)
    {
        return false;
    }

    public bool has_buff_state (ulong uid, uint state)
    {
        return false;
    }

    public void playWarning (ulong id, uint hash, int index, float face, float x, float y, float z)
    {
        SkillHoster.GetHoster.ProjectWarning(id, hash, index, new Vector3(x, y, z), face);
    }

    public void on_state_change (ulong id, int olds, int news)
    {
        //Debug.Log("change: " + id + " , " + state);
    }

    public void on_state_renew (ulong id, int state)
    {
        //Debug.Log("renew: " + id + " , " + state);
    }

    public void playSpecialAction (ulong id, uint hash, int index)
    {
        SkillHoster.GetHoster.playSpecialAction (id, hash, index);
    }

    public void on_action_ratio_changed (ulong id, float ratio)
    {
        SkillHoster.GetHoster.OnActionRatioChanged (id, ratio);
    }

    public bool fetch_height_at (ulong id, float x, ref float y, float z)
    {
        y = SkillHoster.GetHoster.GroundHeight;
        return true;
    }

    public bool verify_pos_at (
        ulong id,
        ref float x,
        ref float y,
        ref float z,
        float rx,
        float ry,
        float rz,
        XStateType type)
    {
        if (y < 0) y = 0;
        return true;
    }
    //return in radian
    public float fetchpitchattr (ulong id)
    {
        Vector3 forward = SkillHoster.GetHoster.cameraComponent.CameraObject.transform.forward;
        Vector3 face = forward;
        face.y = 0; face.Normalize();
        Vector3 roleFace = SkillHoster.GetHoster.EntityDic[SkillHoster.PlayerIndex].obj.transform.forward;
        roleFace.y = 0f; roleFace.Normalize();
        float pitch = Vector3.Angle(forward, face) * Mathf.Deg2Rad;

        return forward.y >= 0f ? pitch : -pitch;
    }

    public double get_attr (ulong id, uint attr)
    {
        return 0;
    }

    public double get_run_speed_percent(ulong id)
    {
        return 1;
    }

    public float client_min_move_dist ()
    {
        return 0.01f;
    }

    public bool on_script_version_check (uint script, int version)
    {
        return true;
    }

    public void on_script_version_not_match (ulong id, uint script)
    {

    }

    public ulong push_target (ulong id)
    {
        ulong target = 0;

        if (SkillHoster.GetHoster.Targets.Count > 0)
        {
            target = SkillHoster.GetHoster.Targets.Dequeue();
        }

        return target;
    }

    public void push_targets(ulong id)
    {
        ulong target = 0;

        do
        {
            target = 0;
            if (SkillHoster.GetHoster.Targets.Count > 0)
            {
                target = SkillHoster.GetHoster.Targets.Dequeue();
            }
            if (target > 0)
            {
                XEcs.singleton.SetBackTargets(id, target);
            }

        } while (target > 0);
        
    }

    public bool has_entity(ulong id)
    {
        return true;
    }

    public ulong get_associate_entity(ulong id)
    {
        return id;
    }

    public bool get_skill_condition(ulong id, uint hash)
    {
        return true;
    }

    public void on_svr_target_push(ulong id, ulong target)
    {
        SkillHoster.GetHoster.Targets.Enqueue(target);
    }

    public void on_target_selected(ulong id, ulong target)
    {

    }

    public void on_cdfixed_on(ulong id)
    {

    }

    public void on_cdfixed_off(ulong id)
    {

    }
}
#endif
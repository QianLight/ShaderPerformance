#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CFEngine;
using EditorEcs;
using UnityEngine;
using VirtualSkill;
using XEcsGamePlay;

public class XEditorFacility : EditorEcs.IFacility
{
    public override bool is_role(ulong id)
    {
        return SkillHoster.PlayerIndex == id;
    }

    public override ulong fetchtime()
    {
        return (ulong)(Time.realtimeSinceStartup * 1000.0f);
    }

    public override uint fetchframe()
    {
        return (uint)Time.frameCount;
    }

    public override uint fetch_delay(ulong id)
    {
        return (uint)(XVirtualNet.singleton.GetLag() * 1000);
    }

    public override float fetchheight(ulong id)
    {
        try
        {
            return SkillHoster.GetHoster.EntityDic[id].presentData.BoundHeight * SkillHoster.GetHoster.EntityDic[id].presentData.Scale;
        }
        catch (Exception e)
        {
            Debug.LogError("fetchheight error: " + e.Message);
            return 0;
        }
    }

    public override float fetchcd(ulong id, uint skill)
    {
        try
        {
            return 0;
        }
        catch (Exception e)
        {
            Debug.LogError("fetchcd error: " + e.Message);
            return 0;
        }
    }

    public override int fetchphasecount(ulong id, uint skill)
    {
        try
        {
            return 1;
        }
        catch (Exception e)
        {
            Debug.LogError("fetchcd error: " + e.Message);
            return 0;
        }
    }

    public override float fetchradius(ulong id)
    {
        try
        {
            return SkillHoster.GetHoster.EntityDic[id].presentData.BoundRadius * SkillHoster.GetHoster.EntityDic[id].presentData.Scale;
        }
        catch (Exception e)
        {
            Debug.LogError("fetchradius error: " + e.Message);
            return 0;
        }
    }

    public override float fetch_move_interpolation(ulong id)
    {
        try
        {
            float i = 0;
            //i = SkillHoster.GetHoster.EntityDic[id].presentData;
            if (i < float.Epsilon) i = 0.5f;
            if (i > 1) i = 1.0f;
            return i;
        }
        catch (Exception e)
        {
            Debug.LogError("fetch_move_interpolation error: " + e.Message);
            return 0.5f;
        }
    }

    public override float fetch_tsangle_factor()
    {
        return 0.05f;
    }

    public override bool fetch_smart_targetlock(ulong id)
    {
        return false;
    }

    public override int fetch_smart_targetlock_time()
    {
        return 3;
    }

    public override float fetch_rotatespeed(ulong id)
    {
        try
        {
            if (SkillHoster.GetHoster.rotateSpeed != 0) return SkillHoster.GetHoster.rotateSpeed;
            return SkillHoster.GetHoster.EntityDic[id].statisticsData == null ? 10 : SkillHoster.GetHoster.EntityDic[id].statisticsData.RotateSpeed;
        }
        catch (Exception e)
        {
            Debug.LogError("fetch_rotatespeed error: " + e.Message);
            return 0;
        }
    }

    public override float fetch_auto_rotatespeed(ulong id)
    {
        try
        {
            if (SkillHoster.GetHoster.rotateSpeed != 0) return SkillHoster.GetHoster.rotateSpeed;
            return 30;
        }
        catch (Exception e)
        {
            Debug.LogError("fetch_auto_rotatespeed error: " + e.Message);
            return 0;
        }
    }
    //return in radian
    public override float fetch_pitch_attr(ulong id)
    {
        Vector3 forward = SkillHoster.GetHoster.cameraComponent.CameraObject.transform.forward;
        Vector3 face = forward;
        face.y = 0; face.Normalize();
        Vector3 roleFace = SkillHoster.GetHoster.EntityDic[SkillHoster.PlayerIndex].obj.transform.forward;
        roleFace.y = 0f; roleFace.Normalize();
        float pitch = Vector3.Angle(forward, face) * Mathf.Deg2Rad;

        return forward.y >= 0f ? pitch : -pitch;

    }

    public override uint fetch_skill_probability_max_cumulation(ulong arg0)
    {
        return 1;
    }

    public override uint fetch_hit_header_hash(ulong id)
    {
        try
        {
            return SkillHoster.GetHoster.GetHitHeaderScriptHash(id);
        }
        catch (Exception e)
        {
            Debug.LogError("fetch_hit_header_hash error: " + e.Message);
            return 0;
        }
    }

    public override string fetch_hit_header_addr(ulong id)
    {
        try
        {
            return SkillHoster.GetHoster.GetHitHeaderScriptPath(id);
        }
        catch (Exception e)
        {
            Debug.LogError("fetch_hit_header_addr error: " + e.Message);
            return null;
        }
    }

    public override float fetchspeed(ulong id)
    {
        try
        {
            if (SkillHoster.GetHoster.moveSpeed != 0) return SkillHoster.GetHoster.moveSpeed;
            return SkillHoster.GetHoster.EntityDic[id].statisticsData == null ? 7 : SkillHoster.GetHoster.EntityDic[id].statisticsData.RunSpeed;
        }
        catch (Exception e)
        {
            Debug.LogError("fetchspeed error: " + e.Message);
            return 0;
        }
    }

    public override void push_skill_stringify(ulong id, uint hash)
    {
        string path = SkillHoster.GetHoster.GetScriptPath(hash);

        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            byte[] bytes = SimpleTools.FileStream2Bytes(fs);
            SimpleTools.Unlock(ref bytes, 0, (int)fs.Length);
            SimpleTools.Bytes2String(ref bytes, 0, (int)fs.Length);
            Xuthus_VirtualServer.cacheSkillScript(hash, bytes);
        }
    }

    public override void push_hit_stringify(ulong id, uint hash)
    {
        string path = SkillHoster.GetHoster.GetHitScriptPath(hash);

        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            byte[] bytes = SimpleTools.FileStream2Bytes(fs);
            SimpleTools.Unlock(ref bytes, 0, (int)fs.Length);
            SimpleTools.Bytes2String(ref bytes, 0, (int)fs.Length);
            Xuthus_VirtualServer.cacheHitScript(hash, bytes);
        }
    }

    public override ulong mob_required(ulong host, int templateid, float face, float x, float y, float z)
    {
        try
        {
            var presentid = XEntityStatisticsReader.GetPresentid((uint)templateid);
            return SkillHoster.GetHoster.CreatePuppet((int)presentid, face, x, y, z);
        }
        catch (Exception e)
        {
            Debug.LogError("mob_required error: " + e.Message);
            return 0;
        }
    }

    public override void mob_released(ulong id)
    {
        try
        {
            SkillHoster.GetHoster.DestoryEntity(id);
        }
        catch (Exception e)
        {
            Debug.LogError("mob_released error: " + e.Message);
        }
    }

    public override void on_skill_begin(ulong id, uint hash)
    {
        XEcs.singleton.Interface.on_svr_skill_begin(id, hash);
    }

    public override void on_skill_end(ulong id, uint hash, uint last_bullet, uint interrupt_return)
    {
        XEcs.singleton.Interface.on_svr_skill_end(id, hash, last_bullet);
    }

    public override void on_state_change(ulong id, int olds, int news)
    {

    }

    public override void on_state_renew(ulong id, int state)
    {

    }

    public override void on_framefixed_off(ulong id)
    {
        
    }

    public override void on_framefixed_on(ulong id, float ratio, bool stopres)
    {

    }

    public override void editor_bullet_sync(ulong id, ulong roleid, float face, float x, float y, float z)
    {
        try
        {
            SkillHoster.GetHoster.EditorBulletSync(id, roleid, face, x, y, z);
        }
        catch (Exception e)
        {
            Debug.LogError("editor_bullet_sync error: " + e.Message);
        }
    }

    public override ulong get_targets(ulong id)
    {
        try
        {
            return SkillHoster.GetHoster.Target;
        }
        catch (Exception e)
        {
            Debug.LogError("get_targets error: " + e.Message);
            return 0;
        }
    }

    public override float get_immortal_length()
    {
        try
        {
            return 0.5f;
        }
        catch (Exception e)
        {
            Debug.LogError("get_immortal_length error: " + e.Message);
            return 0;
        }
    }

    public override bool has_buff(ulong uid, uint id)
    {
        try
        {
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError("has_buff error: " + e.Message);
            return false;
        }
    }

    public override void on_target_selected(ulong id, ulong target)
    {
        try
        {
            XEcs.singleton.Interface.on_svr_target_push(id, target);
        }
        catch (Exception e)
        {
            Debug.LogError("on_target_change error: " + e.Message);

        }
    }

    public override void on_bullet_target_selected(ulong id, ulong bullet, ulong target)
    {
        XEcsGamePlay.XBulletMgr.singleton.SetTarget(
            bullet, 
            target);
    }

    public override void on_item_drop(ulong id, uint item)
    {

    }

    public override void on_space_time_lock(ulong arg0, float arg1, float arg2, int arg3, bool cd)
    {
        SkillHoster.GetHoster.OnSpaceTimeLock(arg0, arg1, arg2, arg3);
    }

    public override void on_trigger_qte_event(ulong arg0, int arg1)
    {

    }

    public override float fetch_height_at(ulong id, float x, float y, float z)
    {
        return SkillHoster.GetHoster.GroundHeight;
    }

    public override void on_bullet_clear(ulong id, ulong bullet, float x, float y, float z, bool imm)
    {
        XEcsGamePlay.XBulletMgr.singleton.ServerOrderKill(bullet, new Vector3(x, y, z));
    }

    public override int get_assist_role_count(ulong id)
    {
        return SkillHoster.GetHoster.AssistCount;
    }

    public override bool is_carried_player(ulong id)
    {
        return id == SkillHoster.PlayerIndex;
    }

    public override float fetch_floating_lower(ulong id)
    {
        return 0;
    }

    public override float fetch_floating_upper(ulong id)
    {
        return 5;
    }

    public override ulong get_associate_entity(ulong id)
    {
        return id;
    }

    public override void deliver_message(ulong id, string msg, int type)
    {
        switch((XMessageType)type)
        {
            case XMessageType.WALL:
                {

                }break;
        }
    }

    public override bool verify_pos(ulong id, float x, float y, float z, float rx, float ry, float rz)
    {
        try
        {
            SkillHoster.GetHoster.VerifyPos(id, ref x, ref y, ref z, rx, ry, rz);
            Xuthus_VirtualServer.setPosition(id, x, y, z);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("verify_pos error: " + e.Message);
            return false;
        }
    }

    public override void on_cdfixed_on(ulong id)
    {
        
    }

    public override void on_cdfixed_off(ulong id)
    {
        
    }

    public override void report_script_parse_error(string message)
    {
        Debug.LogError(message);
    }

    public override void report_error(string message)
    {
        Debug.LogError(message);
    }

    public override void on_wholelife_bullet_project(ulong id)
    {
        
    }
}
#endif
using CFUtilPoolLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public class EditorDirectorBindingData
{

    public DirectorBindingData data
    {
        get
        {
            return DirectorBindingData.Data;
        }
    }

    Dictionary<string, DirectorAnimBinding> animBingdings =
        new Dictionary<string, DirectorAnimBinding> ();

    Dictionary<string, DirectorActiveBinding> ActiveBindings =
        new Dictionary<string, DirectorActiveBinding> ();

    Dictionary<string, DirectorControlBinding> controlBindings =
        new Dictionary<string, DirectorControlBinding> ();

    /// <summary>
    /// checked this on save to disk
    /// </summary>
    public void CheckRmved(PlayableDirector director)
    {
        if (director != null && director.playableAsset != null)
        {
            var list = director.playableAsset.outputs.Select(x => x.streamName);
            var to_remove = animBingdings.Where(x => !list.Contains(x.Key)).Select(x => x.Key).ToArray();
            foreach (var it in to_remove)   animBingdings.Remove(it);

            to_remove = ActiveBindings.Where(x => !list.Contains(x.Key)).Select(x => x.Key).ToArray();
            foreach (var it in to_remove) if (!string.IsNullOrEmpty(it)) ActiveBindings.Remove(it);

            to_remove = controlBindings.Where(x => !list.Contains(x.Key)).Select(x => x.Key).ToArray();
            foreach (var it in to_remove) if (!string.IsNullOrEmpty(it)) controlBindings.Remove(it);
        }
    }

    // public DirectorAnimBinding GetAnimBinding (string name)
    // {
    //     if (animBingdings.ContainsKey (name))
    //     {
    //         return animBingdings[name];
    //     }
    //     return DirectorAnimBinding.Null;
    // }

    // public DirectorActiveBinding GetActiveBinding (string name)
    // {
    //     if (ActiveBindings.ContainsKey (name))
    //     {
    //         return ActiveBindings[name];
    //     }
    //     return DirectorActiveBinding.Null;
    // }

    // public DirectorControlBinding GetControlBinding (string name)
    // {
    //     if (controlBindings.ContainsKey (name))
    //     {
    //         return controlBindings[name];
    //     }
    //     return DirectorControlBinding.Null;
    // }

    public DirectorAnimBinding GetNewAnimBinding (string name)
    {
        if (animBingdings.ContainsKey (name))
        {
            return animBingdings[name];
        }
        DirectorAnimBinding node = new DirectorAnimBinding ();
        node.streamName = name;
        node.type = PlayableAnimType.Entity;
        node.val = 0;
        node.scale = Vector3.one;
        animBingdings.Add (name, node);
        return node;
    }

    public DirectorActiveBinding GetNewActiveBinding (string name)
    {
        if (ActiveBindings.ContainsKey (name))
        {
            return ActiveBindings[name];
        }
        DirectorActiveBinding node = new DirectorActiveBinding ();
        node.streamName = name;
        node.type = PlayerableActiveType.Player;
        node.val = "0";
        ActiveBindings.Add (name, node);
        return node;
    }

    public DirectorControlBinding GetNewControlBinding (string name)
    {
        if (controlBindings.ContainsKey (name))
        {
            return controlBindings[name];
        }
        DirectorControlBinding node = new DirectorControlBinding ();
        node.streamName = name;
        node.nodes = new ControlNode[0];
        controlBindings.Add (name, node);
        return node;
    }
    public void Set (string name, ref DirectorAnimBinding bind)
    {
        animBingdings[name] = bind;
    }
    public void Set (string name, ref DirectorActiveBinding bind)
    {
        ActiveBindings[name] = bind;
    }
    public void Set (string name, ref DirectorControlBinding bind)
    {
        controlBindings[name] = bind;
    }

    public void WriteToFile (PlayableDirector director, BinaryWriter write)
    {
        OnLoad (director);
        DirectorDataWrite.WriteTo (write, data);
    }
    public void OnLoad (PlayableDirector director)
    {
        CheckRmved (director);
        data.animList = new DirectorAnimBinding[animBingdings.Count];
        data.animBindingCount = animBingdings.Count;
        data.ActiveList = new DirectorActiveBinding[ActiveBindings.Count];
        data.activeBindingCount = ActiveBindings.Count;
        data.controlList = new DirectorControlBinding[controlBindings.Count];
        data.controlBindingCount = controlBindings.Count;
        animBingdings.Values.CopyTo (data.animList, 0);
        ActiveBindings.Values.CopyTo (data.ActiveList, 0);
        controlBindings.Values.CopyTo (data.controlList, 0);
    }
    public void OnParse ()
    {
        animBingdings.Clear ();
        ActiveBindings.Clear ();
        controlBindings.Clear ();

        if (data.animList != null)
        {
            foreach (var it in data.animList)
            {
                animBingdings.Add (it.streamName, it);
            }
        }
        if (data.ActiveList != null)
        {
            foreach (var it in data.ActiveList)
            {
                ActiveBindings.Add (it.streamName, it);
            }
        }
        if (data.controlList != null)
        {
            foreach (var it in data.controlList)
            {
                controlBindings.Add (it.streamName, it);
            }
        }
    }

}
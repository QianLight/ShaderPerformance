using Blueprint.Actor;
using UnityEngine;
using System;

namespace Blueprint.Asset
{
    public partial interface IAssetLoader
    {
        T SpawnActor<T>(string path) where T : ActorBase;

        T SpawnActor<T>(string path, Transform parent = null) where T : ActorBase;

        ActorBase SpawnActor(string path);

        ActorBase SpawnActor(string path, Transform parent = null);

        bool Inited();

        T LoadAsset<T>(string path) where T : UnityEngine.Object;

        T LoadAsset<T>(string path, Action release) where T : UnityEngine.Object;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Fusion;
using Mono.Cecil.Cil;
using UnityEngine;

public class ObjectPoolingManager : MonoBehaviour, INetworkObjectPool
{
    private Dictionary<NetworkObject, List<NetworkObject>> prefabsThatHadBeenInstantiated = new();

    public void Start()
    {
        if (GlobalManagers.Instance != null)
        {
            GlobalManagers.Instance.objectPoolingManager = this;
        }
    }

    // Called everytime Runner.Spawn() is called all the peers too
    public NetworkObject AcquireInstance(NetworkRunner runner, NetworkPrefabInfo info)
    {
        NetworkObject networkObject = null;
        NetworkProjectConfig.Global.PrefabTable.TryGetPrefab(info.Prefab, out var prefab);
        prefabsThatHadBeenInstantiated.TryGetValue(prefab, out var networkObjects);

        // We have spawned this object before and potentially can do a recycling
        bool foundMatch = false;
        if (networkObjects?.Count > 0)
        {
            foreach (var item in networkObjects)
            {
                if (item != null && item.gameObject.activeSelf == false)
                {
                    // You want to do recycling now
                    // todo do object pool ing
                    networkObject = item;
                    foundMatch = true;
                    break;
                }
            }
        }

        if (foundMatch == false)
        {
            // Create a new object (Spawning)
            // Add to dictionary
            networkObject = CreateObjectInstance(prefab);
        }

        return networkObject;
    }

    private NetworkObject CreateObjectInstance(NetworkObject prefab)
    {
        var obj = Instantiate(prefab);

        if (prefabsThatHadBeenInstantiated.TryGetValue(prefab, out var instanceData))
        {
            // Already spawned this type of prefab in the past
            // Update only the list not the key
            instanceData.Add(obj);
        }
        else
        {
            // Completely new type of prefab
            // Create a new key in the dictionary
            var list = new List<NetworkObject> { obj };
            prefabsThatHadBeenInstantiated.Add(prefab, list);
        }

        return obj;
    }

    // Called everytime Runner.Despawn() is called all the peers too
    public void ReleaseInstance(NetworkRunner runner, NetworkObject instance, bool isSceneObject)
    {
        instance.gameObject.SetActive(false);
    }

    public void RemoveNetworkObjectFromDict(NetworkObject obj)
    {
        if (prefabsThatHadBeenInstantiated.Count > 0)
        {
            foreach (var item in prefabsThatHadBeenInstantiated)
            {
                foreach (var networkObject in item.Value)
                {
                    if (networkObject == obj)
                    {
                        item.Value.Remove(networkObject);
                        break;
                    }
                }
            }
        }
    }
}

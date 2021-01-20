
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

namespace VRtist
{
    public enum PrefabID
    {
        SunLight,
        SpotLight,
        PointLight,
        Camera,
        Locator
    }

    public enum MaterialID
    {
        ObjectOpaque,
        ObjectTransparent,
        Paint,
        GreasePencil
    }

    public class ResourceManager : MonoBehaviour
    {
        Dictionary<PrefabID, string> prefabsMap = new Dictionary<PrefabID, string>();
        Dictionary<PrefabID, GameObject> prefabs = new Dictionary<PrefabID, GameObject>();

        Dictionary<MaterialID, string> materialsMap = new Dictionary<MaterialID, string>();
        Dictionary<MaterialID, Material> materials = new Dictionary<MaterialID, Material>();

        private static ResourceManager _instance;
        public static ResourceManager Instance
        {
            get { return _instance; }
        }

        private void Awake()
        {
            if (null == _instance)
            {
                _instance = this;

                prefabsMap.Add(PrefabID.SunLight, "Prefabs/Sun");
                prefabsMap.Add(PrefabID.SpotLight, "Prefabs/Spot");
                prefabsMap.Add(PrefabID.PointLight, "Prefabs/Point");
                prefabsMap.Add(PrefabID.Camera, "Prefabs/Camera");
                prefabsMap.Add(PrefabID.Locator, "Prefabs/Primitives/Axis_locator");

                materialsMap.Add(MaterialID.ObjectOpaque, "Materials/ObjectOpaque");
                materialsMap.Add(MaterialID.ObjectTransparent, "Materials/ObjectTransparent");
                materialsMap.Add(MaterialID.Paint, "Materials/Paint");
                materialsMap.Add(MaterialID.GreasePencil, "Materials/GreasePencilMat");
            }
        }

        public static GameObject GetPrefab(PrefabID resource)
        {
            if (!Instance.prefabs.TryGetValue(resource, out GameObject prefab))
            {
                Assert.IsTrue(Instance.prefabsMap.ContainsKey(resource));
                prefab = Resources.Load<GameObject>(Instance.prefabsMap[resource]);
                Instance.prefabs.Add(resource, prefab);
            }
            return prefab;
        }

        public static Material GetMaterial(MaterialID resource)
        {
            if (!Instance.materials.TryGetValue(resource, out Material material))
            {
                Assert.IsTrue(Instance.materialsMap.ContainsKey(resource));
                material = Resources.Load<Material>(Instance.materialsMap[resource]);
                Instance.materials.Add(resource, material);
            }
            return material;
        }
    }
}

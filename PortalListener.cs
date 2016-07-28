using UnityEngine;
using System.Collections.Generic;
using SLua;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BnH {
    [CustomLuaClass]
    public class PortalListener : MonoBehaviour {

        [DoNotToLua]
        public List<GameObject> objs;

        public Vector3 CenterPos;
        public float Radius;

        void Start() {
#if UNITY_EDITOR
            if (LuaService.isStarted) {
                LuaService.instance.LuaAddSceneNode(this);
            }
#else
            LuaService.instance.LuaAddSceneNode(this);
#endif
            BoxCollider collider = GetComponent<BoxCollider>();
            if (collider != null) {
                Destroy(collider);
            }
        }

        public void SetActive(bool isShow) {
            for (int i = 0; i < objs.Count; ++i) {
                objs[i].SetActive(isShow);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos() {
            Gizmos.DrawWireSphere(CenterPos, Radius);
        }

        Bounds bound;

        bool Contains(Bounds a, Bounds b) {
            return a.Contains(b.max) && a.Contains(b.min);
        }

        void RecursiveTestBound(Bounds bound, Transform trans, List<GameObject> inBoundObjs, Vector3 offset) {
            Renderer renderer = trans.GetComponent<Renderer>();
            bound.center -= offset;
            if (renderer != null) {
                if (Contains(bound, renderer.bounds)) {
                    inBoundObjs.Add(trans.gameObject);
                }
                return;
            }
            for (int i = 0; i < trans.childCount; ++i) {
                Transform child = trans.GetChild(i);
                RecursiveTestBound(bound, child, inBoundObjs, Vector3.zero);
            }
        }

        [ContextMenu("更新遮挡")]
        void UpdateBound() {
            bound = GetComponent<Collider>().bounds;

            CenterPos = bound.center;
            Radius = Radius == 0 ? 10 : Radius;

            objs = new List<GameObject>();
            Vector3 boundOffset = transform.localPosition;
            RecursiveTestBound(bound, transform, objs, Vector3.zero);
        }

        [ContextMenu("显示")]
        void Open() {
            SetActive(true);
        }
        [ContextMenu("隐藏")]
        void Hide() {
            SetActive(false);
        }
#endif

    }
}
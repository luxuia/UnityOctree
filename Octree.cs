using UnityEngine;
using System.Collections.Generic;
using SLua;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace BnH {
    [CustomLuaClass]
    public class Octree {
        public OctreeNode Root;

        public float width, length, height;
        public Vector3 center;

        public int MAX_DEPTH = 10;

        public Octree(float width, float length, float height, float centerx, float centery, float centerz) {
            Root = new OctreeNode(this, null, new Vector3(centerx, centery, centerz), new Vector3(width, height, length));
        }

        public void AddSceneObj(SceneObj obj) {
            AddSceneObj(obj, Root, 0);
        }

        public void RemoveSceneObj(SceneObj obj) {
            if( obj.Parent != null) {
                obj.Parent.RemoveSceneObj(obj);
            }
        }


        void FillNodeInCollide(Frustum collide, List<OctreeNode> toFill, OctreeNode node) {
            var side = OctreeNode.IsInSide(ref collide, ref node.AABB);
            if (side == OctreeNode.Side.In) {
                toFill.Add(node);
                return;
            } else if (side == OctreeNode.Side.Cross) {
                if (node.Childs != null) {
                    for (int i =0; i < 8; ++i) {
                        if (node.Childs[i] != null && node.Childs[i].TotalSceneObjs > 0) {
                            FillNodeInCollide(collide, toFill, node.Childs[i]);
                        }
                    }
                }
                if (node.SceneObjs != null && node.SceneObjs.Count > 0) {
                    Color old = Gizmos.color;
                    Gizmos.color = Color.red;

                    for (int i = 0; i < node.SceneObjs.Count; ++i) {
                        if (OctreeNode.IsInSide(ref collide, ref node.SceneObjs[i].AABB) != OctreeNode.Side.Out) {
                            Gizmos.DrawWireCube(node.SceneObjs[i].AABB.center, node.SceneObjs[i].AABB.size);
                        }
                    }
                    Gizmos.color = old;
                }
            }
        }

        public void GetQuery(Frustum collide, List<OctreeNode> toFill) {
            toFill.Clear();

            FillNodeInCollide(collide, toFill, Root);
        }

        public void AddScene() {
            var objs = GameObject.FindObjectsOfType<Renderer>();
            for (int i = 0; i < objs.Length; ++i) {
                var obj = objs[i];
                var sceneObj = new SceneObj();
                sceneObj.AABB = obj.bounds;
                sceneObj.Obj = obj.gameObject;
                AddSceneObj(sceneObj);
            }
        }

        public void DrawNodes(OctreeNode node, Color color) {
            if (node != null) {
                Color old = Gizmos.color;
                Gizmos.color = color;
                Gizmos.DrawWireCube(node.AABB.center, node.AABB.size);
                Gizmos.color = old;
                if (node.Childs != null) {
                    for (int i = 0; i < node.Childs.Length; ++i) {
                        DrawNodes(node.Childs[i], color);
                    }
                }
            }
        }

        public void OnDrawGizmos() {
            DrawNodes(Root, Color.white);
        }

        public void AddSceneObj(SceneObj obj, OctreeNode node, int depth) {
            if (depth < MAX_DEPTH && node.isDoubleSize(obj.AABB)) {
                var fitNode = node.GetFitNode(obj.AABB);
                if (fitNode != node) {
                    AddSceneObj(obj, fitNode.GetFitNode(obj.AABB), ++depth);
                } else {
                    node.AddSceneObj(obj);
                }
            } else {
                node.AddSceneObj(obj);
            }
        }
    }
}
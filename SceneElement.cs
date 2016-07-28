using UnityEngine;
using System.Collections;
using SLua;


namespace BnH {
    [CustomLuaClass]
    public class SceneObj {

        public GameObject Obj;  // 场景元素
        public int LuaID; // lua中的逻辑对象

        public Bounds AABB;

        public OctreeNode Parent;
    }

}
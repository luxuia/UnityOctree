using UnityEngine;
using System.Collections;

public class Frustum {
    public Plane[] Planes = new Plane[6];

    public Frustum() {
        for (int i = 0; i < 6;++i) {
            Planes[i] = new Plane();
        }
    }

    public void Update() {
        var cam = Camera.main;
        var trans = cam.transform;

        //前后两个平面
        Planes[0].normal = -trans.forward;
        Planes[0].SetNormalAndPosition(-trans.forward, trans.position + trans.forward* cam.farClipPlane);

        Planes[1].normal = trans.forward;
        Planes[1].SetNormalAndPosition(trans.forward, trans.position + trans.forward * cam.nearClipPlane);

        // 两边两个平面
        float fov = cam.fieldOfView;
        var rotate = Quaternion.AngleAxis(fov/2, trans.up);
        Vector3 up = trans.up;
        Planes[2].SetNormalAndPosition(Vector3.Cross(rotate * trans.forward, trans.up), trans.position);
        Gizmos.DrawLine(trans.position, trans.position + rotate * trans.forward * 1000);

        rotate.Set(-rotate.x,- rotate.y, -rotate.z, rotate.w);
        Planes[3].SetNormalAndPosition(Vector3.Cross(trans.up, rotate * trans.forward), trans.position);
        Gizmos.DrawLine(trans.position, trans.position + rotate * trans.forward * 1000);
    }
}

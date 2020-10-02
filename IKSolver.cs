using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IKSolver)), CanEditMultipleObjects]
public class E_IKSolverV2 : Editor
{
    public override void OnInspectorGUI()
    {
        IKSolver solver = (IKSolver)target;
        if (solver.needResetOption)
        {
            GUI.enabled = false;
        }
        DrawDefaultInspector();
        if (solver.needResetOption)
        {
            GUI.enabled = true;
            if (GUILayout.Button("Reset Scene Hierarchy"))
            {
                solver.ResetHierarchy();
            }
        }
        else
        {
            if (GUILayout.Button("Start Tracking"))
            {
                solver.Initialize();
                solver.enable = true;
            }
        }
    }
}

[ExecuteInEditMode]
public class IKSolver : MonoBehaviour
{
    [Serializable]
    public class Bone
    {
        public Transform bone;
        public Transform pole;
        public Vector2 rollContraints;
        public Vector2 angleConstraints;

        [HideInInspector]
        public Transform endPoint;
        [HideInInspector]
        public float length;
        [HideInInspector]
        public Vector3 origPos;
        [HideInInspector]
        public Quaternion origRot;
    }

    [Header("Bones - Leaf to Root")]
    [Tooltip("Make sure you assign them in leaf to root order only...")]
    [SerializeField]
    public Bone[] bones;
    [Tooltip("The end point of the leaf bone positioned at tip of the chain to get its orientation...")]
    public Transform endPointOfLastBone;

    [Header("Settings")]
    [Tooltip("More precision...")]
    public int iterations;

    [Header("EditMode")]
    [HideInInspector]
    public bool enable;

    [HideInInspector]
    public bool needResetOption = false;

    private Vector3 lastTargetPosition;
    private bool editorInitialized = false;

    void Start()
    {
        lastTargetPosition = transform.position;
        if (Application.isPlaying && !editorInitialized)
        {
            Initialize();
        }
    }

    void Update()
    {
        if (lastTargetPosition != transform.position)
        {
            if (Application.isPlaying || (Application.isEditor && enable))
            {
                Solve();
            }
        }
    }

    public void Initialize()
    {
        bones[0].length = Vector3.Distance(endPointOfLastBone.position, bones[0].bone.position);
        bones[0].origPos = bones[0].bone.position;
        bones[0].origRot = bones[0].bone.rotation;
        bones[0].endPoint = endPointOfLastBone;
        for (int i = 1; i < bones.Length; i++)
        {
            bones[i].origPos = bones[i].bone.position;
            bones[i].origRot = bones[i].bone.rotation;
            bones[i].length = Vector3.Distance(bones[i - 1].bone.position, bones[i].bone.position);
            bones[i].endPoint = bones[i - 1].bone;
        }
        editorInitialized = true;
        needResetOption = true;
    }

    void Solve()
    {
        /*
         * Can be used to get consistent results at cost of computation...
         * May also be NOT used to get variations...
        for (int i = 0; i < bones.Length; i++)
        {
            bones[i].bone.position = bones[i].origPos;
            bones[i].bone.rotation = bones[i].origRot;
        }
        */

        Vector3 rootPoint = bones[bones.Length - 1].bone.position;
        Vector3 dir;
        Quaternion forwRot;

        // Poles
        for (int j = 1; j < bones.Length; j++)
        {
            Vector3 to = bones[j - 1].endPoint.position - bones[j].bone.position;
            Vector3 norm = Vector3.Cross(bones[j].pole.position - bones[j].bone.position, to);
            Vector3 cast = Vector3.Cross(bones[j - 1].endPoint.position - bones[j].bone.position, norm).normalized;
            float d = (bones[j - 1].endPoint.position - bones[j].bone.position).magnitude;
            float y = (bones[j].length * bones[j].length + d * d - bones[j - 1].length * bones[j - 1].length) / (2f * d);
            d = (float)Math.Sqrt(Mathf.Abs(bones[j].length * bones[j].length - y * y));
            Vector3 newPos = bones[j].bone.position + to.normalized * y + cast * d;
            dir = (newPos - bones[j].bone.position).normalized;
            forwRot = Quaternion.FromToRotation(bones[j].bone.up, dir);
            bones[j].bone.rotation = Quaternion.LookRotation(forwRot * bones[j].bone.forward, dir);
            bones[j - 1].bone.position = newPos;
            dir = (bones[j - 1].endPoint.position - bones[j - 1].bone.position).normalized;
            forwRot = Quaternion.FromToRotation(bones[j - 1].bone.up, dir);
            bones[j - 1].bone.rotation = Quaternion.LookRotation(forwRot * bones[j - 1].bone.forward, dir);
        }

        for (int i = 0; i < iterations; i++)
        {
            // Base
            dir = (transform.position - bones[0].bone.position).normalized;
            forwRot = Quaternion.FromToRotation(bones[0].bone.up, dir);
            bones[0].bone.rotation = Quaternion.LookRotation(forwRot * bones[0].bone.forward, dir);
            bones[0].bone.position = transform.position - (dir * bones[0].length);
            for (int j = 1; j < bones.Length; j++)
            {
                Quaternion childRotBackup = bones[j - 1].bone.rotation;
                Vector3 childPosBackup = bones[j - 1].bone.position;
                dir = (bones[j - 1].bone.position - bones[j].bone.position).normalized;
                forwRot = Quaternion.FromToRotation(bones[j].bone.up, dir);
                bones[j].bone.rotation = Quaternion.LookRotation(forwRot * bones[j].bone.forward, dir);
                bones[j].bone.position = childPosBackup - (dir * bones[j].length);
                bones[j - 1].bone.rotation = childRotBackup;
                bones[j - 1].bone.position = childPosBackup;
            }

            // Step Angle Constraints
            for (int j = 1; j < bones.Length; j++)
            {
                Vector3 endPoint = bones[j - 1].endPoint.position;
                Vector3 parentUp = bones[j].bone.up;
                Vector3 childUp = bones[j - 1].bone.up;
                Vector3 norm = Vector3.Cross(parentUp, childUp).normalized;
                float angle = Vector3.SignedAngle(parentUp, childUp, norm);
                angle = Mathf.Clamp(angle, bones[j - 1].angleConstraints.x, bones[j - 1].angleConstraints.y);
                forwRot = Quaternion.FromToRotation(bones[j - 1].bone.up, Quaternion.AngleAxis(angle, norm) * bones[j].bone.up);
                bones[j - 1].bone.rotation = Quaternion.LookRotation(forwRot * bones[j - 1].bone.forward, Quaternion.AngleAxis(angle, norm) * bones[j].bone.up);
                bones[j - 1].bone.position += endPoint - bones[j - 1].endPoint.position;
            }

            bones[bones.Length - 1].bone.position = rootPoint;
        }

        // Clamp roll - TBE
        for (int j = 0; j < bones.Length - 1; j++)
        {
            Quaternion ft = Quaternion.FromToRotation(bones[j].bone.up, bones[j + 1].bone.up);
            Quaternion rft = Quaternion.FromToRotation(bones[j + 1].bone.up, bones[j].bone.up);
            bones[j].bone.rotation = ft * bones[j].bone.rotation;
            Vector3 eulers = bones[j].bone.localRotation.eulerAngles;
            eulers.y = Mathf.Clamp(eulers.y, bones[j].rollContraints.x, bones[j].rollContraints.y);
            bones[j].bone.localRotation = Quaternion.Euler(eulers);
            bones[j].bone.rotation = rft * bones[j].bone.rotation;
        }

        // Final Angle Constraints
        for (int j = 1; j < bones.Length; j++)
        {
            Vector3 parentUp = bones[j].bone.up;
            Vector3 childUp = bones[j - 1].bone.up;
            Vector3 norm = Vector3.Cross(parentUp, childUp).normalized;
            float angle = Vector3.SignedAngle(parentUp, childUp, norm);
            angle = Mathf.Clamp(angle, bones[j - 1].angleConstraints.x, bones[j - 1].angleConstraints.y);
            forwRot = Quaternion.FromToRotation(bones[j - 1].bone.up, Quaternion.AngleAxis(angle, norm) * bones[j].bone.up);
            bones[j - 1].bone.rotation = Quaternion.LookRotation(forwRot * bones[j - 1].bone.forward, Quaternion.AngleAxis(angle, norm) * bones[j].bone.up);
        }

        lastTargetPosition = transform.position;
    }

    /// <summary>
    /// Do not ever call this in Play mode. It will mess up the IK system.
    /// </summary>
    public void ResetHierarchy()
    {
        for (int i = bones.Length - 1; i >= 0; i--)
        {
            bones[i].bone.position = bones[i].origPos;
            bones[i].bone.rotation = bones[i].origRot;
        }
        lastTargetPosition = Vector3.zero;
        enable = false;
        editorInitialized = false;
        needResetOption = false;
    }
}


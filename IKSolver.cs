using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[ExecuteInEditMode]
public class IKSolver : MonoBehaviour
{
    [Serializable]
    public class Bone
    {
        public Transform bone;
        public float length;
    }

    [Header("Bones - Leaf to Root")]
    [Tooltip("Make sure you assign them in leaf to root order only...")]
    [SerializeField]
    public Bone[] bones;

    [Header("Settings")]
    [Tooltip("Bend chain towards this target when target is closer than total chain length... Make sure you place it far than total chain length for better accuracy...")]
    public Transform poleTarget;
    [Tooltip("More precision...")]
    public int iterations;

    [Header("EditMode")]
    public bool enable;

    private Vector3 lastTargetPosition;

    void Start()
    {
        lastTargetPosition = transform.position;
    }

    void Update()
    {
        if (lastTargetPosition != transform.position)
        {
            if (Application.isPlaying || (Application.isEditor && enable))
            {
                Solve();
            }
            lastTargetPosition = transform.position;
        }
    }

    void Solve()
    {
        Vector3 rootPoint = bones[bones.Length - 1].bone.position;
        bones[bones.Length - 1].bone.up = -(poleTarget.position - bones[bones.Length - 1].bone.position);
        for (int i = bones.Length - 2; i >= 0; i--)
        {
            bones[i].bone.position = bones[i + 1].bone.position + (-bones[i + 1].bone.up * bones[i + 1].length);
            bones[i].bone.up = -(poleTarget.position - bones[i].bone.position);
        }
        for (int i = 0; i < iterations; i++)
        {
            bones[0].bone.up = -(transform.position - bones[0].bone.position);
            bones[0].bone.position = transform.position - (-bones[0].bone.up * bones[0].length);
            for (int j = 1; j < bones.Length; j++)
            {
                bones[j].bone.up = -(bones[j - 1].bone.position - bones[j].bone.position);
                bones[j].bone.position = bones[j - 1].bone.position - (-bones[j].bone.up * bones[j].length);
            }

            bones[bones.Length - 1].bone.position = rootPoint;
            for (int j = bones.Length - 2; j >= 0; j--)
            {
                bones[j].bone.position = bones[j + 1].bone.position + (-bones[j + 1].bone.up * bones[j + 1].length);
            }
        }
    }
}

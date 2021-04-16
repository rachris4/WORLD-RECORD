using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointBuilder : MonoBehaviour
{
    [SerializeField]
    public bool Mirror;
    [SerializeField]
    public string[] Bodyparts;
    [SerializeField]
    public float rotationMax;
    [SerializeField]
    public float rotationMin;
}

public class ControllerBuilder : MonoBehaviour
{
    [SerializeField]
    public string Type;
    [SerializeField]
    public float StrengthMod;
    [SerializeField]
    public float SpeedMod;
    [SerializeField]
    public float P;
    [SerializeField]
    public float I;
    [SerializeField]
    public float D;
    [SerializeField]
    public float Rotation;
    [SerializeField]
    public float Border;
    [SerializeField]
    public float SpeedLimit;
    [SerializeField]
    public int Wavelength;
    [SerializeField]
    public float Offset;
}

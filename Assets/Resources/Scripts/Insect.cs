using UnityEngine;
using System.Collections;
using System;

using Random = UnityEngine.Random;
using UnityEngine.UI;
using System.Collections.Generic;
public enum InsectPreset
{
    Custom, Mosquito, Fly, Wasp, DragonFly
}

[ExecuteInEditMode]
public class Insect : MonoBehaviour
{
    Vector3 InsectTransformPosition;
    Quaternion InsectTransformRotation = new Quaternion(0, 0, 0, 0);

    private GameObject PlayerObject;

    public InsectPreset Preset;
    public bool Swarm = false;
    public int SwarmSize = 0;
    public int SizeMod = 1;
    public bool Jittering = true;
    public float JitterTime = 1.0f;         //seconds
    public float JitterAmount = 1.0f;       //Unity units
    public bool Flying = true;
    public float FlyingTime = 1.0f;
    public float FlyingIntervalRandom = 0.5f;
    public float FlyingStrength = 2.0f;     //Unity units  
    public bool InitialBurstOfSpeed = true;
    public bool ChangesMind = false;
    public int ChangesMindChance = 50;
    public bool Migrating = false;
    public bool LinearInterpolation = false;
    public bool StickToRegion = true;
    public float RegionRadius = 5.0f;
    public bool Fleeing = false;
    public float FleeingDistance = 2.0f;
    public bool DebugMode = false;
    public bool CubeMode = true;
    public bool DistanceLerp = true;

    public Color DebugObjectColor = Color.white;

    private Vector3 InsectOrigin;

    private double JitterCounter = 0;
    private double FlyingCounter = 0;

    private List<GameObject> InsectSwarm;
    private float UsedRegionRadius = 0.0f;

    private bool InitialBurstOfSpeedMod;

    public bool Hidden = false;

    void Start()
    {
        InitialBurstOfSpeedMod = InitialBurstOfSpeed;

        PlayerObject = GameObject.FindGameObjectWithTag("Char");

        if (Application.isPlaying)
        {
            this.transform.localScale *= SizeMod;

            if (Swarm)
            {
                InsectSwarm = new List<GameObject>();
                for (int o = 0; o < SwarmSize; o++)
                {
                    GameObject i;
                    if (CubeMode)
                        i = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    else
                        i = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    i.name = this.name + "Swarm" + Preset.ToString() + o;

                    i.transform.position = this.transform.position;
                    i.transform.localScale = this.transform.localScale;

                    i.AddComponent<Insect>();

                    i.GetComponent<Insect>().PlayerObject = PlayerObject;
                    i.GetComponent<Insect>().CubeMode = CubeMode;
                    i.GetComponent<Insect>().Preset = Preset;
                    i.GetComponent<Insect>().Swarm = false;
                    i.GetComponent<Insect>().SwarmSize = 0;
                    i.GetComponent<Insect>().ChangesMind = ChangesMind;
                    i.GetComponent<Insect>().ChangesMindChance = ChangesMindChance;
                    i.GetComponent<Insect>().Jittering = Jittering;
                    i.GetComponent<Insect>().JitterTime = JitterTime;
                    i.GetComponent<Insect>().JitterAmount = JitterAmount;
                    i.GetComponent<Insect>().Fleeing = Fleeing;
                    i.GetComponent<Insect>().Flying = Flying;
                    i.GetComponent<Insect>().FlyingTime = FlyingTime - Random.Range(-FlyingIntervalRandom, FlyingIntervalRandom);
                    i.GetComponent<Insect>().FlyingIntervalRandom = FlyingIntervalRandom;
                    i.GetComponent<Insect>().FlyingStrength = FlyingStrength;
                    i.GetComponent<Insect>().StickToRegion = StickToRegion;
                    i.GetComponent<Insect>().RegionRadius = RegionRadius;
                    i.GetComponent<Insect>().DebugMode = DebugMode;
                    i.GetComponent<Insect>().DebugObjectColor = DebugObjectColor;
                    i.GetComponent<Insect>().LinearInterpolation = LinearInterpolation;
                    i.GetComponent<Insect>().Migrating = Migrating;
                    i.GetComponent<Insect>().UsedRegionRadius = UsedRegionRadius;
                    i.GetComponent<Insect>().InitialBurstOfSpeed = InitialBurstOfSpeed;

                    if (DistanceLerp)
                    {
                        Material newMat = Resources.Load("Materials/distanceLerp", typeof(Material)) as Material;
                        i.GetComponent<MeshRenderer>().material = newMat;
                    }

                    //i.AddComponent<Rigidbody>();
                    //i.GetComponent<Rigidbody>().useGravity = false;
                    //i.GetComponent<Rigidbody>().angularDrag = 1000;

                    i.tag = "Insect";
                    i.layer = gameObject.layer;

                    InsectSwarm.Add(i);
                }
            }

            Material nm = Resources.Load("Materials/distanceLerp", typeof(Material)) as Material;
            GetComponent<MeshRenderer>().material = nm;

            InsectOrigin = this.transform.position;

            InsectTransformPosition = this.transform.position;
            InsectTransformRotation = this.transform.rotation;

            JitterTime *= 1000; //naar ms
            FlyingTime *= 1000; //naar ms

            if (Migrating)
            {
                UsedRegionRadius = RegionRadius * 4;
            }
            else
                UsedRegionRadius = RegionRadius;
        }
    }

    void Update()
    {

        if (!Hidden)
        {
            if (Application.isPlaying)
            {
                if (StickToRegion && UsedRegionRadius < FlyingStrength) //usedRegion is altijd groter dan FlyingStrength als Migration aanstaat. Daarom deze rare check. 
                {
                    if (!Migrating)
                    {
                        Debug.LogError("Stick to Region is on while FlyingStrength is higher than DistanceFromOrigin, please make sure FlyingStrength is lower than DistanceFromOrigin");
                        return;
                    }
                }
                if (ChangesMindChance >= 100)
                {
                    Debug.LogError("ChangesMindChance is higher than 99. Please use a maximum of 99.");
                }

                if (Jittering)
                {
                    JitterCounter += Time.deltaTime * 1000;
                    if (JitterCounter >= JitterTime)
                    {


                        JitterCounter = 0;
                        Jitter();
                    }
                }

                if (Flying)
                {
                    if (!Migrating)
                    {
                        FlyingCounter += Time.deltaTime * 1000;
                        if (FlyingCounter >= (FlyingTime + Random.Range(-FlyingIntervalRandom, FlyingIntervalRandom)))
                        {
                            FlyingCounter = 0;
                            Fly();
                        }
                    }
                    else
                    {
                        //migratie, dus anim is afhankelijk of het insect in de buurt van de target is.                     
                        if (Vector3.Distance(transform.position, InsectTransformPosition) < 1.0f)
                            Fly();
                        else if (ChangesMind) //Of changesMind staat aan, dan is er een kleine kans dat er gewoon direct gevlogen moet worden.
                        {
                            float chance = Random.value * 100;
                            if (chance > ChangesMindChance)
                                Fly();
                        }
                    }
                }

                //fleeing overides the flying
                if (Vector3.Distance(this.transform.position, PlayerObject.transform.position) < FleeingDistance)
                    Flee();

                if (this.transform.position != InsectTransformPosition)
                {
                    if (!LinearInterpolation)
                        this.transform.position = Vector3.Slerp(this.transform.position, InsectTransformPosition, Time.deltaTime);
                    else
                        this.transform.position = Vector3.Lerp(this.transform.position, InsectTransformPosition, Time.deltaTime);
                }

                if (this.transform.rotation != InsectTransformRotation)
                {
                    if (!LinearInterpolation)
                        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, InsectTransformRotation, Time.deltaTime);
                    else
                        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, InsectTransformRotation, Time.deltaTime);
                }

                if (DebugObjectColor != Color.white)
                {
                    this.GetComponent<Renderer>().material.SetColor("_Color", DebugObjectColor);
                }
            }
            else
            {
                //presets...
                switch (Preset)
                {
                    case InsectPreset.Wasp:
                        Jittering = false;
                        JitterAmount = 3.0f;
                        JitterTime = 3.0f;
                        Flying = true;
                        FlyingIntervalRandom = 0.5f;
                        FlyingStrength = 1.5f;
                        FlyingTime = 1.0f;
                        DebugObjectColor = Color.yellow;
                        Fleeing = false;
                        FleeingDistance = 0.1f;
                        UsedRegionRadius = RegionRadius;

                        break;
                    case InsectPreset.Mosquito:
                        Jittering = true;
                        JitterAmount = 0.01f;
                        JitterTime = 0.1f;
                        Flying = true;
                        FlyingTime = 0.1f;
                        FlyingStrength = 0.1f;
                        DebugObjectColor = Color.red;

                        UsedRegionRadius = RegionRadius;

                        break;
                    case InsectPreset.Fly:
                        Jittering = true;
                        JitterAmount = 0.5f;
                        JitterTime = 0.3f;
                        Flying = true;
                        FlyingStrength = 1.0f;
                        FlyingIntervalRandom = 0.2f;
                        FlyingTime = 0.5f;
                        DebugObjectColor = Color.green;

                        UsedRegionRadius = RegionRadius;

                        break;
                    case InsectPreset.DragonFly:
                        Jittering = false;
                        JitterAmount = 0.0f;
                        JitterTime = 0.0f;
                        Flying = true;
                        FlyingStrength = 1.0f;
                        FlyingIntervalRandom = 0.2f;
                        FlyingTime = 0.5f;
                        Migrating = true;
                        LinearInterpolation = true;
                        DebugObjectColor = Color.blue;

                        UsedRegionRadius = RegionRadius * 4;

                        break;
                    case InsectPreset.Custom:

                        break;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (DebugMode && Application.isPlaying)
        {
            try
            {
                //teken wat gizmos.             
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(InsectOrigin, UsedRegionRadius);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(this.transform.position, InsectTransformPosition);

                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Mesh cu = go.GetComponent<MeshFilter>().sharedMesh;

                Gizmos.color = Color.blue;
                Gizmos.DrawWireMesh(cu, InsectTransformPosition, InsectTransformRotation, this.transform.localScale);

                DestroyImmediate(go);
            }
            catch (Exception E)
            { }
        }
    }
    void OnCollisionEnter(Collision e)
    {
        Debug.Log("TesT?");
    }


    void Jitter()
    {
        float x = Random.Range(-JitterAmount, JitterAmount);
        float y = Random.Range(-JitterAmount, JitterAmount);
        float z = Random.Range(-JitterAmount, JitterAmount);

        InsectTransformPosition += new Vector3(x, y, z);
    }

    void Fly()
    {
        //volg een vector met een bepaalde lengte.   
        float strength = 0.0f;

        if (InitialBurstOfSpeedMod)
        {
            strength = Random.Range(-RegionRadius, RegionRadius); // de ene of de andere kant op.         
            InitialBurstOfSpeedMod = false;
        }
        else
            strength = Random.Range(-FlyingStrength, FlyingStrength); // de ene of de andere kant op.         

        Quaternion rot = Random.rotation;
        Vector3 cp = InsectTransformPosition;

        Vector3 cprot = rot * cp; //rotated vector

        InsectTransformRotation = rot;

        //ff zien of de dist nog wel in de region zit
        if (StickToRegion)
        {
            if (!Migrating)
            {
                Vector3 prospected = InsectTransformPosition + (rot * Vector3.forward) * strength;
                float prospectedDist = Vector3.Distance(InsectOrigin, prospected);

                if (prospectedDist < UsedRegionRadius)
                    InsectTransformPosition += (rot * Vector3.forward) * strength;
                else
                {
                    //go back to origin
                    float angle = Vector3.Angle(this.transform.position, InsectOrigin);
                    float distance = Vector3.Distance(this.transform.position, InsectOrigin);

                    float fx = (InsectOrigin.x - this.transform.position.x) / distance;
                    float fy = (InsectOrigin.y - this.transform.position.y) / distance;
                    float fz = (InsectOrigin.z - this.transform.position.z) / distance;

                    Vector3 target = new Vector3(fx, fy, fz);

                    InsectTransformPosition += target;
                }
            }
            else
            {
                //dit word 1 keer geraakt.

                //migrate to point on sphere...
                Vector3 norm = Random.onUnitSphere + InsectOrigin;

                float distance = Vector3.Distance(InsectOrigin, norm);

                float fx = (InsectOrigin.x - norm.x) / distance;
                float fy = (InsectOrigin.y - norm.y) / distance;
                float fz = (InsectOrigin.z - norm.z) / distance;

                Vector3 target = new Vector3(fx, fy, fz);

                InsectTransformPosition = InsectOrigin + (target * UsedRegionRadius);
            }
        }
    }

    private void Flee()
    {
        float angle = Vector3.Angle(this.transform.position, PlayerObject.transform.position);
        InsectTransformRotation = Quaternion.AngleAxis(angle, this.transform.forward);

        float distance = Vector3.Distance(this.transform.position, PlayerObject.transform.position);

        float fx = (this.transform.position.x - PlayerObject.transform.position.x) / distance;
        float fy = (this.transform.position.y - PlayerObject.transform.position.y) / distance;
        float fz = (this.transform.position.z - PlayerObject.transform.position.z) / distance;

        Vector3 target = new Vector3(fx, fy, fz);

        InsectTransformPosition += target; //ga weg van speler. 
    }
}
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class NewInsect : MonoBehaviour {

    public bool Swarm = false;

    [HideInInspector()]
    public bool SwarmBoss = true;

    public bool Home = false;

    public int SwarmSize = 0;
    public bool GradualMassIncrease = false;
    public bool GradualDragDecrease = false;
    public int GradualMassIncreaseSize = 1;

    public bool FollowTarget = false;    
    public Transform FollowTargetTransform;

    public int Speed = 1;

    public bool GoToTarget = false;
    public float RetainDistanceToOrigin = 2.0f;

    public bool Flying = false;
    public float FlyingTime = 3.0f;

    public float RegionSize = 10.0f;
    private Vector3 RegionOrigin;

    public bool DebugMode = false;
    public Color DebugObjectColor = Color.white;

    public bool Attacks = false;
    public float AttackPower = 10.0f;
    
    private Vector3 InsectTarget;
    private Vector3 InsectOriginPosition;
    private Quaternion InsectOriginRotation;

    private float GameTimer = 0.0f;
    private float Health = 100.0f;
    [HideInInspector()]
    public List<GameObject> InsectSwarm;

    private bool Duplicate = false;
    private float UniqueTimeMod = 0.0f;

	// Use this for initialization
	void Start () {
        
        UniqueTimeMod = 2.0f / UnityEngine.Random.Range(0, 10);
        InsectOriginPosition = transform.position;
        RegionOrigin = InsectOriginPosition;
        InsectTarget = InsectOriginPosition;

        if (SwarmBoss)
            this.tag = "Swarmboss";

        if (Swarm)
        {
            InsectSwarm = new List<GameObject>();
            for (int o = 0; o < SwarmSize; o++)
            {
                GameObject i = GameObject.CreatePrimitive(PrimitiveType.Cube);

                i.name = this.name + "Swarm" +  o;

                i.transform.position = this.transform.position;
                i.transform.localScale = this.transform.localScale;

                i.AddComponent<NewInsect>();
                i.GetComponent<NewInsect>().Swarm = false; //anders krijg je combo explosie
                i.GetComponent<NewInsect>().SwarmSize = SwarmSize;
                i.GetComponent<NewInsect>().Speed = Speed;
                i.GetComponent<NewInsect>().GoToTarget = GoToTarget;
                i.GetComponent<NewInsect>().RetainDistanceToOrigin = RetainDistanceToOrigin;
                i.GetComponent<NewInsect>().Flying = Flying;
                i.GetComponent<NewInsect>().FlyingTime = FlyingTime;
                i.GetComponent<NewInsect>().RegionSize = RegionSize;
                i.GetComponent<NewInsect>().DebugMode = DebugMode; //ook hier, of niet echt, maar iets minder ram nodig
                i.GetComponent<NewInsect>().Duplicate = true;
                i.GetComponent<NewInsect>().FollowTarget = FollowTarget;
                i.GetComponent<NewInsect>().FollowTargetTransform = FollowTargetTransform;
                i.GetComponent<NewInsect>().DebugObjectColor = DebugObjectColor;
                i.GetComponent<NewInsect>().SwarmBoss = false;
                i.GetComponent<NewInsect>().Attacks = Attacks;
                
                i.AddComponent<Rigidbody>();

                if (GradualMassIncrease)
                {
                    i.GetComponent<Rigidbody>().mass = this.GetComponent<Rigidbody>().mass * (o * GradualMassIncreaseSize);

                    if (GradualDragDecrease)
                        i.GetComponent<Rigidbody>().drag = this.GetComponent<Rigidbody>().drag / (o * GradualMassIncreaseSize); //dit moet afnemen
                    else
                        i.GetComponent<Rigidbody>().drag = this.GetComponent<Rigidbody>().drag;
                }
                else
                {
                    i.GetComponent<Rigidbody>().mass = this.GetComponent<Rigidbody>().mass;
                    i.GetComponent<Rigidbody>().drag = this.GetComponent<Rigidbody>().drag;
                }

                i.GetComponent<Rigidbody>().useGravity = this.GetComponent<Rigidbody>().useGravity;
                
                i.GetComponent<Rigidbody>().angularDrag = this.GetComponent<Rigidbody>().angularDrag;

                InsectSwarm.Add(i);
            }

            //assign lijst aan alle instanties
            for (int u = 0; u < InsectSwarm.Count - 1; u++)
            {
                InsectSwarm[u].GetComponent<NewInsect>().InsectSwarm = InsectSwarm;
            }
        }

        if (DebugObjectColor != Color.white)
            this.GetComponent<Renderer>().material.SetColor("_Color", DebugObjectColor);        

        if(SwarmBoss)
            this.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
	}
	
	// Update is called once per frame
	void Update () 
    {      
        if (GoToTarget && Vector3.Distance(transform.position, InsectTarget) > RetainDistanceToOrigin)
        {                        
            transform.LookAt(InsectTarget);

            this.GetComponent<Rigidbody>().velocity += transform.forward * Speed;
        }

        if (Flying && (FlyingTime + UniqueTimeMod) < GameTimer)
        {
            Fly();
        }

        if (Attacks) //dit is de positie van de ANDERE boss
        {
            //verplaats de region, komen de insects er vanzelf achterna
            //vind de dichtsbijzijnde swarmboss en vlieg m aan
            GameObject[] bosses = GameObject.FindGameObjectsWithTag("Swarmboss");
            foreach (GameObject boss in bosses)
            {
                if (boss != gameObject) //niet achter jezelf aan
                {
                    //hier moet het meerdere swarms ding in komen
                    RegionOrigin = boss.transform.position;
                }
            }
        }
        else if(SwarmBoss)
        {            
            //eigen boss volgen
            foreach (GameObject item in InsectSwarm)
            {
                item.GetComponent<NewInsect>().RegionOrigin = transform.position;                    
            }            
        }
        

        GameTimer += Time.deltaTime;
	}

    private void Fly()
    {  
        if (!FollowTarget)
        {
            Quaternion rot = UnityEngine.Random.rotation;
            Vector3 rotnorm = rot * Vector3.forward;

            rotnorm *= UnityEngine.Random.Range(-RegionSize, RegionSize);
            rotnorm += RegionOrigin;

            InsectTarget = rotnorm;
        }
        else
        {                
            InsectTarget = FollowTargetTransform.position;
        }

        GameTimer = 0.0f;        
    }

    private void OnDrawGizmos()
    {        
        if (Application.isPlaying && DebugMode)
        {
            if (!Duplicate)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(RegionOrigin, RegionSize);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.transform.position, InsectTarget);

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh cu = go.GetComponent<MeshFilter>().sharedMesh;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireMesh(cu, InsectTarget, Quaternion.identity, this.transform.localScale);

            DestroyImmediate(go);
        }
    }
    
    private void OnCollisionEnter(Collision col)
    {
        try
        {
            if (!InsectSwarm.ToList().Contains(col.gameObject) && col.gameObject.GetComponent<NewInsect>() != null)
            {
                //subtract health
                col.gameObject.GetComponent<NewInsect>().Health -= AttackPower;

                if (col.gameObject.GetComponent<NewInsect>().Health < 0.0f)
                {
                    Debug.Log(name + " killed " + col.gameObject.name);

                    if (col.gameObject.GetComponent<NewInsect>().SwarmBoss)
                        col.gameObject.GetComponent<NewInsect>().FindNewSwarmboss();

                    //was dit de swarmboss? Moet de next in command in command komen

                    col.gameObject.SetActive(false);
                }
            }
            else { }
            //eigen doelpunt
        }
        catch (ArgumentNullException e)
        { 
            
        }
    }

    private void FindNewSwarmboss()
    {
        foreach (GameObject Insect in InsectSwarm)
        {
            if (Insect.activeSelf)
            {
                Debug.Log(Insect.name + " is now in charge of the swarm");

                Insect.GetComponent<NewInsect>().SwarmBoss = true;
                Insect.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                //zo, gevonden
                break;
            }
        }
    }
}
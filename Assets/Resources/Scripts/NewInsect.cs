using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class NewInsect : MonoBehaviour {

    [Tooltip("Copy this instance into a swarm.")]
    public bool Swarm = false;
    public int SwarmSize = 0;

    [HideInInspector()]
    public bool SwarmBoss = true;

    [Tooltip("Is this the home team?")]
    public bool HomeTeam = false;

    [Tooltip("Increases the mass of each new swarm instance. Impacts attack power.")] 
    public bool GradualMassIncrease = false;
    [Tooltip("Decreases the drag of each new swarm instance. Impacts maneuverability.")]
    public bool GradualDragDecrease = false;
    [Tooltip("Multiplication of the drag and mass increase and decrease.")]
    public int GradualMassIncreaseSize = 1;

    [Tooltip("Follow a specific target.")]
    public bool FollowTarget = false;
    [Tooltip("The target.")]
    public Transform FollowTargetTransform;

    [Tooltip("Speed of the insects.")]
    public int Speed = 1;

    public bool GoToTarget = false;
    public float RetainDistanceToOrigin = 2.0f;

    public bool Flying = false;
    public float FlyingTime = 3.0f;

    public float RegionSize = 10.0f;
    private Vector3 RegionOrigin;

    public bool Fleeing = false;
    public float FleeingDistance = 1.5f;

    [Tooltip("If you set this to true, it will not only flee from the attacker, but from anything not part of the swarm")]
    public bool FleeFromAnything = false;   

    public bool DebugMode = false;
    public Color DebugObjectColor = Color.white;

    public bool Attacks = false;
    public float AttackPower = 10.0f;

    [HideInInspector()]
    public GameObject AttackTarget;

    private Vector3 InsectTarget;
    private Vector3 InsectOriginPosition;
    private Quaternion InsectOriginRotation;

    private float GameTimer = 0.0f;
    private float Health = 100.0f;
    [HideInInspector()]
    public List<GameObject> InsectSwarm;

    private bool Duplicate = false;
    private float UniqueTimeMod = 0.0f;
    public bool Blocked = false;

    [HideInInspector()]
    public bool Initializer = true;

	// Use this for initialization
    private FlyingType FlyType;
    private enum FlyingType { 
        Normal,
        Attack,
        Fleeing,
        Idle
    }

	void Start () {
        
        UniqueTimeMod = 2.0f / UnityEngine.Random.Range(0, 10);
        InsectOriginPosition = transform.position;
        RegionOrigin = InsectOriginPosition;
        InsectTarget = InsectOriginPosition;

        AttackTarget = gameObject;

        if (SwarmBoss)
            this.tag = "Swarmboss";

        if (Swarm)
        {
            InsectSwarm = new List<GameObject>();
            for (int o = 0; o < SwarmSize; o++)
            {
                //plaats op een punt in de spere, anders colliden ze
                Vector3 pos = randomSpherePoint(transform.position.x, transform.position.y, transform.position.z, RegionSize );
                
                Vector3 NewInsectPosition = new Vector3(pos.x, pos.y, pos.z);

                GameObject i = GameObject.CreatePrimitive(PrimitiveType.Cube);

                i.name = this.name + "Swarm" +  o;

                i.transform.position = NewInsectPosition;
                i.transform.localScale = this.transform.localScale;

                i.AddComponent<NewInsect>();

                i.GetComponent<NewInsect>().Initializer = false;

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
                i.GetComponent<NewInsect>().Blocked = Blocked;
                i.GetComponent<NewInsect>().Fleeing = Fleeing;
                i.GetComponent<NewInsect>().FleeingDistance = FleeingDistance;
                i.GetComponent<NewInsect>().FleeFromAnything = FleeFromAnything;

               
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

            InsectSwarm.Add(this.gameObject);

            //assign lijst aan alle instanties
            for (int u = 0; u < InsectSwarm.Count - 1; u++)
            {
                InsectSwarm[u].GetComponent<NewInsect>().InsectSwarm = InsectSwarm;
            }
        }
        else if (Initializer && !Swarm)            
            InsectSwarm.Add(this.gameObject);

        if (DebugObjectColor != Color.white)
            this.GetComponent<Renderer>().material.SetColor("_Color", DebugObjectColor);        

        if(SwarmBoss)
            this.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (!Blocked)
        {
            OnKeyPress();

            if (GoToTarget && Vector3.Distance(transform.position, InsectTarget) > RetainDistanceToOrigin)
            {
                transform.LookAt(InsectTarget);

                this.GetComponent<Rigidbody>().velocity += transform.forward * Speed; //Flytowards target
            }
           
            if (Flying && (FlyingTime + UniqueTimeMod) < GameTimer)
            {                
                Fly();
            }
            else if (SwarmBoss)
            {
                //eigen boss volgen
                foreach (GameObject item in InsectSwarm)
                {
                    item.GetComponent<NewInsect>().RegionOrigin = transform.position;
                }
            }

            GameTimer += Time.deltaTime;
        }

        Debug.Log(FlyType);
	}

    private void Attack()
    {
        //verplaats de region, komen de insects er vanzelf achterna
        //vind de dichtsbijzijnde swarmboss en vlieg m aan
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Swarmboss");
        foreach (GameObject boss in bosses)
        {
            if (boss.name != gameObject.name) //niet achter jezelf aan
            {
                //hier moet het meerdere swarms ding in komen
                RegionOrigin = boss.transform.position;
            }
        }

        if (!AttackTarget.activeSelf || AttackTarget.name == gameObject.name)
        {
            //set new insect target to follow around
            //closest
            if (Swarm)
            {
                List<GameObject> ll = new List<GameObject>();

                GameObject[] boss = GameObject.FindGameObjectsWithTag("Swarmboss");
                foreach (GameObject b in boss)
                {
                    if (InsectSwarm.ToList().Count(t => t.name == b.name) == 0) //niet achter jezelf aan
                    {
                        ll = b.GetComponent<NewInsect>().InsectSwarm;
                        break;
                    }
                }

                Dictionary<float, GameObject> distDic = new Dictionary<float, GameObject>();

                foreach (GameObject l in ll)
                {
                    if (l.activeSelf)
                        distDic.Add(Vector3.Distance(this.transform.position, l.transform.position), l);
                }

                List<float> distances = distDic.Keys.ToList();
                distances.Sort();

                if (distDic.Count != 0)
                {
                    AttackTarget = distDic[distances[0]];
                }
            }
            else
            { 
                //geen swarm, dus maar 1 insect hier
                GameObject[] boss = GameObject.FindGameObjectsWithTag("Swarmboss");
                foreach (GameObject b in boss)
                {                    
                    if (b.name != name)
                        AttackTarget = b;
                }
            }
        }
    }

    private void Fly()
    {        
        if (Attacks)        
            Attack();        
        else if (Fleeing)        
            Flee(); 
        
        if (!FollowTarget && !Attacks && !Fleeing)
        {
            Quaternion rot = UnityEngine.Random.rotation;
            Vector3 rotnorm = rot * Vector3.forward;

            rotnorm *= UnityEngine.Random.Range(-RegionSize, RegionSize);
            rotnorm += RegionOrigin;

            InsectTarget = rotnorm;
        }
        else if (Attacks && AttackTarget.activeSelf)
        {
            InsectTarget = AttackTarget.transform.position;
            FlyType = FlyingType.Attack;
        }      
       
        GameTimer = 0.0f; 
    }       
    
    private void Flee()
    {
        //check if anything is attacking the insect
        GameObject Attacker = this.gameObject;

        List<GameObject> ll = new List<GameObject>();
        GameObject[] boss = GameObject.FindGameObjectsWithTag("Swarmboss");
     
        foreach (GameObject b in boss)
        {
            if (InsectSwarm.ToList().Count(t => t.name == b.name) == 0) //niet achter jezelf aan
            {
                ll = b.GetComponent<NewInsect>().InsectSwarm;                
                break;
            }
        }

        Dictionary<float, GameObject> distDic = new Dictionary<float, GameObject>();

        foreach (GameObject l in ll)
        {
            if (l.activeSelf)
                distDic.Add(Vector3.Distance(this.transform.position, l.transform.position), l);
        }

        List<float> distances = distDic.Keys.ToList();
        distances.Sort();    

        if (distances.Count != 0)
            Attacker = distDic[distances[0]];

        if (distances[0] < FleeingDistance)
        {            
            FlyType = FlyingType.Fleeing;
            this.GetComponent<Rigidbody>().velocity = Quaternion.AngleAxis(UnityEngine.Random.Range(-90, 90), new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value)) * (Attacker.GetComponent<Rigidbody>().velocity * 1);
        }        
    }

    private void FindNewSwarmboss()
    {
        foreach (GameObject Insect in InsectSwarm)
        {
            if (Insect.activeSelf && Insect.name != name) //niet zelf
            {
                Insect.GetComponent<NewInsect>().SwarmBoss = true;
                Insect.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                //zo, gevonden

                this.tag = "Insects";
                Insect.tag = "Swarmboss";

                break;
            }
        }
    }

    Vector3 randomSpherePoint(float x0, float y0, float z0, float radius)
    {
       float u = UnityEngine.Random.value;
       var v = UnityEngine.Random.value;

       var theta = 2 * Math.PI * u;
       var phi = Math.Acos(2 * v - 1);
       
       float x = x0 + (radius * (float)Math.Sin(phi) * (float)Math.Cos(theta));
       float y = y0 + (radius * (float)Math.Sin(phi) * (float)Math.Sin(theta));
       float z = z0 + (radius * (float)Math.Cos(phi));

       return new Vector3(x,y,z);
    }

    #region Handler functions

    private void OnKeyPress()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Attacks = true;
        }
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
            float localvelocity = GetComponent<Rigidbody>().velocity.magnitude;
            float remotevelocity = col.gameObject.GetComponent<Rigidbody>().velocity.magnitude;

            //compare vectors, als de ene van achteren komt heeft ie voorrang
            Vector3 localvector = GetComponent<Rigidbody>().velocity;
            Vector3 remotevector = col.gameObject.GetComponent<Rigidbody>().velocity;
            //todo

            bool localattack = true;
            if (localvelocity < remotevelocity) //first check the velocity, takes precedence
                localattack = false;

            if (FlyType == FlyingType.Fleeing)
                localattack = true;
            else if (col.gameObject.GetComponent<NewInsect>().FlyType == FlyingType.Fleeing)
                localattack = false;
            

            if (InsectSwarm.ToList().Count(tag => tag.name == col.gameObject.name) == 0 && col.gameObject.GetComponent<NewInsect>() != null)
            {                
                if (!localattack)
                    col.gameObject.GetComponent<NewInsect>().Health -= AttackPower;
                else
                    this.Health -= AttackPower;

                if (col.gameObject.GetComponent<NewInsect>().Health <= 0.0f)
                {
                    
                    if (!localattack)
                    {                        
                        if (col.gameObject.GetComponent<NewInsect>().SwarmBoss)                            
                            col.gameObject.GetComponent<NewInsect>().FindNewSwarmboss();
                        
                        col.gameObject.SetActive(false);
                    }
                    else
                    {                        
                        if (this.SwarmBoss)                     
                            this.FindNewSwarmboss();                        

                        this.gameObject.SetActive(false);
                    }       
                }
            }
        }
        catch (ArgumentNullException e)
        { 
            
        }
    }

    GameObject[] FindGameObjectsWithLayer(int layer) 
    { 
        GameObject[] goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        List<GameObject> goList = new List<GameObject>(); 
        for (int i = 0; i < goArray.Length; i++) 
        { 
            if (goArray[i].layer == layer) 
            { 
                goList.Add(goArray[i]); 
            } 
        } 
        if (goList.Count == 0) 
        { 
            return null; 
        } 
        return goList.ToArray(); 
    }
    #endregion
}
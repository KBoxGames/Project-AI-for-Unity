using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EntytiAI : MonoBehaviour


{

    private NavMeshAgent Agent;
    
    public  float Health;
    public int Hangry;
    public int HangryIntencity;

    public GameObject Target;

    private Condition State;
 
    private bool Alive = true;
    Rigidbody Mrigidbody;

    private bool TargetCapturet;
    private float TimerHangry;
    private float TimerLook;
    private float TimerFind;

    public bool FindComplete = false;
    public bool FindEmpty = true;
 

    private Vector3 TargetPosition;

    private GameObject temp;

    // ------------------------ зрение -------------------------------------------
    [Header ("Настройки горизонта")]
    [Range (30f,200f)]
    public float AngelEyeH; //Угол обзора по горизонтали
    [Range(0.1f,5f)]
    public float DistanceEye; // Дальность видимости
    [Range (1,50)]
    public int RayValue;  //количество лучей
    private float step; //расстояние между лучами
    private float StartAngel; // начальный угол
    [Header("Настройки вертикали")]
    [Range(-1f, 1f)]
    public float StartAngelelV;
    [Range(1, 5)]
    public int LayerValue;
    [Range(0.01f, 5f)]
    public float StepV;
    

    const float unit = 0.0174444444f; //один градус
    const float UnitPerCircle = 6.28f; //360 градусов
     // -------------------------------------------------------------------------

       

        




    void Start()
    {

         Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        Health = 100;
        Hangry = 0;
        TimerHangry = 0;
        HangryIntencity = 2;
        TargetCapturet = false;
        
    }

  

    void FixedUpdate()
    {
        
        if (Alive)
        {
            TheChoiceOfAcion();

            LookAT_();

            UpdateHangry();

            UpdateHealth();

            UpdateState();

            
        }

        

    }

    void TheChoiceOfAcion()
    {
         
        switch (State)
        {
            case Condition.Rest:
                if (!TargetCapturet)
                {
                 RandomTarget();
                    Walking();
                }
             
                Debug.Log("Отдых");
                break;

            case Condition.FinderFood:
                Find("Food");
                Debug.Log("Поиск пищи");
                break;

            case Condition.Walking:
                Walking();
                Debug.Log("Бег");
                break;

            case Condition.Dead:
                DeadEntity();
                Debug.Log("Конец");
                break;
            case Condition.test:
                Debug.Log("Тест");
                Test();
                break;
        }
    }

    //---------------------------------------Ж\Параметры------------------------------------------------

    void UpdateHangry()
    {
        TimerHangry += Time.deltaTime;
        if (TimerHangry > 1)
        {
            Hangry = Hangry < 100 ? Hangry + HangryIntencity : 100;
            TimerHangry = 0;
        }
    }

    void UpdateHealth()
    {
        if (Hangry == 100)
        {
            Health -= 0.1f;
        }

        if (Health <= 0)
        {
            Health = 0;
        }
    }
           
    void LookAT_()
    {

         Vector3 targetDirection = Target.transform.position - transform.position;
         Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, Time.deltaTime*2f, 0.0f);

         transform.rotation = Quaternion.LookRotation(newDirection);
                       
        TimerLook += Time.deltaTime;
        if (TargetCapturet && TimerLook > 2)
        {
            TimerLook = 0;
            TargetCapturet = false;
        }
    }

    //----------------------------------------рефлексы--------------------------------------------------

    void RandomTarget()
    {
        float RNDX = Random.Range(transform.position.x - 10, transform.position.x + 10);
        float RNDZ = Random.Range(transform.position.z-10,transform.position.z + 10);
        Target.transform.position = new Vector3(RNDX, 0, RNDZ);
        TargetCapturet = true;
    }

    void Find(string Tag)
    {
        Color CL = Color.red;
        float BX;
        float BZ;
        float BY = StartAngelelV;
        StartAngel = UnitPerCircle - (AngelEyeH / 2) * unit;
        step = (AngelEyeH * unit) / (RayValue - 1);
        float step_ = StartAngel;
        float DistanceToFood = 100;
        TimerFind += Time.deltaTime;
        
        
        if (!TargetCapturet) RandomTarget();


        // создание лучей
        for (int j = 0; j < LayerValue; j++)
        {

            for (int i = 0; i < RayValue; i++)
            {
                BX = DistanceEye * Mathf.Sin(step_);
                BZ = DistanceEye * Mathf.Cos(step_);
                CL = Color.red;

                RaycastHit HitFood;
                Ray ray = new Ray(transform.position, transform.TransformDirection(new Vector3(BX, BY, BZ)) * 10);
                Physics.Raycast(ray, out HitFood);

                if (HitFood.collider != null && HitFood.collider.gameObject.tag == Tag)
                {
                    if (HitFood.distance < DistanceToFood)
                    {
                       
                        Debug.Log(HitFood.collider.gameObject.tag + " Вижу");
                        CL = Color.green;
                        Target.transform.position = HitFood.collider.gameObject.transform.position;
                        FindComplete = true;
                       
                    }
                }
                

                Debug.DrawRay(transform.position, transform.TransformDirection(new Vector3(BX, BY, BZ)) * 10, CL);
                step_ = step_ + step;

            }

            step_ = StartAngel;
            BY = BY + StepV;
        }

    }

    void Walking()
    {
        Debug.Log("Иду к цели...");
        Agent.destination = Target.transform.position;
        
        
        
        
    }

    // ---------------------------------------Cознание---------------------------------------------------

    void UpdateState()
    {
        
        if (Hangry <= 5 )
        {
            State = Condition.Rest;
        } // если не голоден то гуляешь

        if (Hangry > 70 && TimerFind < 10)
        {
            State = Condition.FinderFood;
        } //если голоден и ищешь пищу менее 10 сек

        if (Hangry > 70 && TimerFind > 10)
        {
            State = Condition.Rest;
            TimerFind += Time.deltaTime;
        } //если голоден и не чего не нашел в течении 10 сек, то идешь на новое место

        if (Agent.pathStatus == NavMeshPathStatus.PathComplete && TimerFind > 20)
        {
            State = Condition.FinderFood;
            TimerFind = 0;
        } //  если поиск более 20 сек не дал результатов, и пришел на новое место то начинаешь искать с начала

        if (Health <= 0)
        {
            State = Condition.Dead;
        }

        if (Hangry > 30 && FindComplete)
        {
            State = Condition.Walking;
        }

        if (Agent.pathStatus == NavMeshPathStatus.PathComplete && Hangry < 70)
        {
            Debug.Log("Пришел в точку! Гуляю");
            State = Condition.Rest;
        }

        


    } 

    void DeadEntity()
    {

        transform.rotation = Quaternion.Euler(90f, transform.rotation.y, transform.rotation.z);
        Alive = false;
        Debug.Log("NOOOOOOOOOOOOOOOOOOT");
        Destroy(this.gameObject);
    }

    //------------------------------------------TEST-----------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Food" && Hangry > 50)
        {
            Hangry = Hangry - 50; 
            FindComplete = false;
            Destroy(other.gameObject);

        }
    }
   

    void Test()
    {
        Hangry -= HangryIntencity;

        if (Hangry < 10)
        {
            Destroy(temp);
            FindComplete = false;
        }
    }
}

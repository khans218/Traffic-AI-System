using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BikeControl : MonoBehaviour
{

    private AIVehicle AIVScript;

    // Wheels Setting /////////////////////////////////

    public BikeWheels bikeWheels;

    [System.Serializable]
    public class BikeWheels
    {
        public ConnectWheel wheels;
        public WheelSetting setting;
    }


    [System.Serializable]
    public class ConnectWheel
    {

        public Transform wheelFront; // connect to Front Right Wheel transform
        public Transform wheelBack; // connect to Front Left Wheel transform

        public Transform AxleFront; // connect to Back Right Wheel transform
        public Transform AxleBack; // connect to Back Left Wheel transform

    }

    [System.Serializable]
    public class WheelSetting
    {

        public float Radius = 0.3f; // the radius of the wheels
        public float Weight = 1000.0f; // the weight of a wheel
        public float Distance = 0.2f;

    }

    // Lights Setting /////////////////////////////////

    public BikeLights bikeLights;

    [System.Serializable]
    public class BikeLights
    {

        public Light[] brakeLights;

    }


    // Bike sounds /////////////////////////////////

    public BikeSounds bikeSounds;

    [System.Serializable]
    public class BikeSounds
    {
        public AudioSource IdleEngine;
        public AudioSource horn;

        public AudioSource crash;
        public AudioSource nitro;
    }

    // Bike Particle /////////////////////////////////

    public BikeParticles bikeParticles;
    private GameObject[] Particle = new GameObject[4];

    [System.Serializable]
    public class BikeParticles
    {
        public GameObject brakeParticlePrefab;
        public ParticleSystem shiftParticle1, shiftParticle2;
    }

    [System.Serializable]
    public class HitGround
    {
        public string tag = "street";
        public bool grounded = false;
        public AudioClip brakeSound;
        public AudioClip groundSound;
        public Color brakeColor;

    }


    // Bike Engine Setting /////////////////////////////////

    public BikeSetting bikeSetting;

    [System.Serializable]
    public class BikeSetting
    {

        public bool showNormalGizmos = false;

        public HitGround[] hitGround;

        public Transform MainBody;
        public Transform bikeSteer;


        public float maxWheelie = 40.0f;
        public float speedWheelie = 30.0f;

        public float slipBrake = 3.0f;


        public float springs = 35000.0f;
        public float dampers = 4000.0f;

        public float bikePower = 120;
        public float shiftPower = 150;
        public float brakePower = 8000;

        public Vector3 shiftCentre = new Vector3(0.0f, -0.6f, 0.0f); // offset of centre of mass

        public float maxSteerAngle = 30.0f; // max angle of steering wheels
        public float maxTurn = 1.5f;

        public float shiftDownRPM = 1500.0f; // rpm script will shift gear down
        public float shiftUpRPM = 4000.0f; // rpm script will shift gear up
        public float idleRPM = 700.0f; // idle rpm

        public float stiffness = 1.0f; // for wheels, determines slip

        public bool automaticGear = true;

        public float[] gears = { -10f, 9f, 6f, 4.5f, 3f, 2.5f }; // gear ratios (index 0 is reverse)

        public float LimitBackwardSpeed = 60.0f;
        public float LimitForwardSpeed = 220.0f;

    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private Quaternion SteerRotation;


    [HideInInspector]
    public bool grounded = true;

    private float MotorRotation;

    [HideInInspector]
    public bool crash;

    [HideInInspector]
    public float steer = 0; // steering -1.0 .. 1.0
    [HideInInspector]
    public bool brake;

    private float slip = 0.0f;

    [HideInInspector]
    public bool Backward = false;

    [HideInInspector]
    public float steer2;

    private float accel = 0.0f; // accelerating -1.0 .. 1.0

    private bool shifmotor;

    [HideInInspector]
    public float curTorque = 100f;

    [HideInInspector]
    public float powerShift = 100;

    [HideInInspector]
    public bool shift;

    private float flipRotate = 0.0f;

    [HideInInspector]
    public float speed = 0.0f;

    // table of efficiency at certain RPM, in tableStep RPM increases, 1.0f is 100% efficient
    // at the given RPM, current table has 100% at around 2000RPM
    private float[] efficiencyTable = { 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 1.0f, 1.0f, 0.95f, 0.80f, 0.70f, 0.60f, 0.5f, 0.45f, 0.40f, 0.36f, 0.33f, 0.30f, 0.20f, 0.10f, 0.05f };

    // the scale of the indices in table, so with 250f, 750RPM translates to efficiencyTable[3].
    private float efficiencyTableStep = 250.0f;

    private float shiftDelay = 0.0f;

    private float Pitch;
    private float PitchDelay;

    private float shiftTime = 0.0f;

    private bool bikeOff = true;

    [HideInInspector]
    public int currentGear = 0;
    [HideInInspector]
    public bool NeutralGear = true;
    [HideInInspector]
    public float motorRPM = 0.0f;

    private float wantedRPM = 0.0f;
    private float w_rotate;

    private Rigidbody myRigidbody;

    private bool shifting;

    private float Wheelie;
    private Quaternion deltaRotation1, deltaRotation2;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private WheelComponent[] wheels;
    private class WheelComponent
    {

        public Transform wheel;
        public Transform axle;
        public WheelCollider collider;
        public Vector3 startPos;
        public float rotation = 0.0f;
        public float maxSteer;
        public bool drive;
        public float pos_y = 0.0f;
    }

    private WheelComponent SetWheelComponent(Transform wheel, Transform axle, bool drive, float maxSteer, float pos_y)
    {

        WheelComponent result = new WheelComponent();
        GameObject wheelCol = new GameObject(wheel.name + "WheelCollider");



        wheelCol.transform.parent = transform;
        wheelCol.transform.position = wheel.position;
        wheelCol.transform.eulerAngles = transform.eulerAngles;
        pos_y = wheelCol.transform.localPosition.y;


       
        wheelCol.AddComponent(typeof(WheelCollider));

       
        result.drive = drive;
        result.wheel = wheel;
        result.axle = axle;
        result.collider = wheelCol.GetComponent<WheelCollider>();
        result.pos_y = pos_y;
        result.maxSteer = maxSteer;
        result.startPos = axle.transform.localPosition;

        return result;

    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void Awake()
    {

        if (bikeSetting.automaticGear) NeutralGear = false;

        AIVScript = transform.GetComponent<AIVehicle>();


        if (bikeSounds.horn)
            AIVScript.horn = bikeSounds.horn;


        if (AIVScript.vehicleStatus == VehicleStatus.EmptyOff)
        {

            bikeOff = true;

            foreach (Light brakeLight in bikeLights.brakeLights)
                brakeLight.enabled = false;
        }
        else
        {
            bikeOff = false;
        }


        myRigidbody = transform.GetComponent<Rigidbody>();

        SteerRotation = bikeSetting.bikeSteer.localRotation;
        wheels = new WheelComponent[2];

        wheels[0] = SetWheelComponent(bikeWheels.wheels.wheelFront, bikeWheels.wheels.AxleFront, false, bikeSetting.maxSteerAngle, bikeWheels.wheels.AxleFront.localPosition.y);
        wheels[1] = SetWheelComponent(bikeWheels.wheels.wheelBack, bikeWheels.wheels.AxleBack, true, 0, bikeWheels.wheels.AxleBack.localPosition.y);

        wheels[0].collider.transform.localPosition = new Vector3(0, wheels[0].collider.transform.localPosition.y, wheels[0].collider.transform.localPosition.z);
        wheels[1].collider.transform.localPosition = new Vector3(0, wheels[1].collider.transform.localPosition.y, wheels[1].collider.transform.localPosition.z);


        foreach (WheelComponent w in wheels)
        {


            WheelCollider col = w.collider;

            col.suspensionDistance = bikeWheels.setting.Distance;
            JointSpring js = col.suspensionSpring;

            js.spring = bikeSetting.springs;
            js.damper = bikeSetting.dampers;
            col.suspensionSpring = js;


            col.radius = bikeWheels.setting.Radius;


            col.mass = bikeWheels.setting.Weight;


            WheelFrictionCurve fc = col.forwardFriction;

            fc.asymptoteValue = 0.5f;
            fc.extremumSlip = 0.4f;
            fc.asymptoteSlip = 0.8f;
            fc.stiffness = bikeSetting.stiffness;
            col.forwardFriction = fc;
            fc = col.sidewaysFriction;
            fc.asymptoteValue = 0.75f;
            fc.extremumSlip = 0.2f;
            fc.asymptoteSlip = 0.5f;
            fc.stiffness = bikeSetting.stiffness;
            col.sidewaysFriction = fc;
        }
    }

    public void ShiftUp()
    {
           float now = Time.timeSinceLevelLoad;

          if (now < shiftDelay) return;


        if (currentGear < bikeSetting.gears.Length - 1)
        {

            if (!bikeSetting.automaticGear)
            {
                if (currentGear == 0)
                {
                    if (NeutralGear) { currentGear++; NeutralGear = false; }
                    else
                    { NeutralGear = true; }
                }
                else
                {
                    currentGear++;
                }
            }
            else
            {
                currentGear++;
            }

               shiftDelay = now + 1.0f;
               shiftTime = 1.0f;
        }
    }


    public void ShiftDown()
    {
           float now = Time.timeSinceLevelLoad;

           if (now < shiftDelay) return;

        if (currentGear > 0 || NeutralGear)
        {

            if (!bikeSetting.automaticGear)
            {

                if (currentGear == 1)
                {
                    if (!NeutralGear) { currentGear--; NeutralGear = true; }
                }
                else if (currentGear == 0) { NeutralGear = false; } else { currentGear--; }
            }
            else
            {
                currentGear--;
            }

                shiftDelay = now + 0.1f;
                shiftTime = 2.0f;
        }
    }





    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Water"))
            Application.LoadLevel(Application.loadedLevel);

        if (!bikeSounds.crash.isPlaying)
        {
            bikeSounds.crash.Play();
            bikeSounds.crash.volume = Mathf.Clamp(myRigidbody.velocity.magnitude / 10.0f, 0.1f, 1.0f);
        }
    }




    void Update()
    {

        if (AIVScript.vehicleStatus == VehicleStatus.EmptyOff)
        {

            bikeSounds.IdleEngine.mute = true;
            bikeSounds.nitro.mute = true;
        }
        else
        {
            if (bikeOff)
            {

                bikeSounds.IdleEngine.mute = false;
                bikeSounds.nitro.mute = false;

                bikeOff = false;
            }
        }

        if (AIVScript.vehicleStatus == VehicleStatus.AI)
        {
            AIVScript.AIActive = true;
        }
        else if (AIVScript.vehicleStatus == VehicleStatus.Player)
        {

            AIVScript.AIActive = false;

            AIVScript.automaticGear = bikeSetting.automaticGear;
            AIVScript.neutralGear = NeutralGear;
            AIVScript.currentGear = currentGear;
            AIVScript.motorRPM = motorRPM;
            AIVScript.powerShift = powerShift;

            if (!bikeSetting.automaticGear)
            {
                if (Input.GetKeyDown("page up"))
                    ShiftUp();
                
                if (Input.GetKeyDown("page down"))
                    ShiftDown();
            }
        }
        else
        {
            AIVScript.AIActive = false;
        }


        steer2 = Mathf.LerpAngle(steer2, steer * -bikeSetting.maxSteerAngle, Time.deltaTime * 10.0f);

        MotorRotation = Mathf.LerpAngle(MotorRotation, steer2 * bikeSetting.maxTurn * (Mathf.Clamp(speed / 100, 0.0f, 1.0f)), Time.deltaTime * 5.0f);

        if (bikeSetting.bikeSteer)
            bikeSetting.bikeSteer.localRotation = SteerRotation * Quaternion.Euler(0, wheels[0].collider.steerAngle, 0); // this is 90 degrees around y axis



        if (!crash)
        {
            flipRotate = (transform.eulerAngles.z > 90 && transform.eulerAngles.z < 270) ? 180.0f : 0.0f;

            Wheelie = Mathf.Clamp(Wheelie, 0, bikeSetting.maxWheelie);


            if (shifting)
            {
                Wheelie += bikeSetting.speedWheelie * Time.deltaTime / (speed / 50);
            }
            else
            {
                Wheelie = Mathf.MoveTowards(Wheelie, 0, (bikeSetting.speedWheelie * 2) * Time.deltaTime * 1.3f);
            }


            deltaRotation1 = Quaternion.Euler(-Wheelie, 0, flipRotate - transform.localEulerAngles.z + (MotorRotation));
            deltaRotation2 = Quaternion.Euler(0, 0, flipRotate - transform.localEulerAngles.z);


            myRigidbody.MoveRotation(myRigidbody.rotation * deltaRotation2);
            bikeSetting.MainBody.localRotation = deltaRotation1;


        }
        else
        {

            bikeSetting.MainBody.localRotation = Quaternion.identity;
            Wheelie = 0;
        }


    }

    void FixedUpdate()
    {

        speed = myRigidbody.velocity.magnitude * 2.7f;
        AIVScript.vehicleSpeed = speed;

        if (crash)
        {
            myRigidbody.constraints = RigidbodyConstraints.None;
            myRigidbody.centerOfMass = Vector3.zero;
        }
        else
        {
            myRigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;
            myRigidbody.centerOfMass = bikeSetting.shiftCentre;
        }


        switch (AIVScript.vehicleStatus)
        {
            case VehicleStatus.EmptyOff | VehicleStatus.EmptyOn:

                accel = 0.0f;
                steer = 0.0f;
                brake = false;
                shift = false;
                break;

            case VehicleStatus.AI:

                accel = AIVScript.AIAccel;
                steer = AIVScript.AISteer;
                brake = AIVScript.AIBrake;

                break;
            case VehicleStatus.Player:
                if (GameControl.manager.controlMode == ControlMode.simple)
                {
                    accel = 0.0f;
                    shift = false;
                    brake = false;

                    if (!crash)
                    {

                        steer = Mathf.MoveTowards(steer, Input.GetAxis("Horizontal"), 0.1f);
                        accel = Input.GetAxis("Vertical");
                        brake = Input.GetButton("Jump");
                        shift = Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift);
                    }
                    else
                    {
                        steer = 0;
                    }

                }
                else if (GameControl.manager.controlMode == ControlMode.touch)
                {
                    if (GameControl.accelFwd != 0) { accel = GameControl.accelFwd; } else { accel = GameControl.accelBack; }
                    steer = Mathf.MoveTowards(steer, GameControl.steerAmount, 0.07f);
                    brake = GameControl.brake;
                    shift = GameControl.shift;
                }
                break;

        }

        if (AIVScript.vehicleStatus != VehicleStatus.EmptyOff)
        {

            foreach (Light brakeLight in bikeLights.brakeLights)
            {
                if (accel < 0 || speed < 1.0f)
                {
                    brakeLight.intensity = Mathf.Lerp(brakeLight.intensity, 8, 0.1f);
                }
                else
                {
                    brakeLight.intensity = Mathf.Lerp(brakeLight.intensity, 0, 0.1f);
                }

                brakeLight.enabled = brakeLight.intensity == 0 ? false : true;
            }

        }

        // handle automatic shifting
        if (bikeSetting.automaticGear && (currentGear == 1) && (accel < 0.0f))
        {
            if (speed < 1.0f)
                ShiftDown(); // reverse


        }
        else if (bikeSetting.automaticGear && (currentGear == 0) && (accel > 0.0f))
        {
            if (speed < 5.0f)
                ShiftUp(); // go from reverse to first gear

        }
        else if (bikeSetting.automaticGear && (motorRPM > bikeSetting.shiftUpRPM) && (accel > 0.0f) && speed > 10.0f && !brake)
        {
            // if (speed > 20)
            ShiftUp(); // shift up

        }
        else if (bikeSetting.automaticGear && (motorRPM < bikeSetting.shiftDownRPM) && (currentGear > 1))
        {
            ShiftDown(); // shift down
        }



        if (speed < 1.0f) Backward = true;
        

        if (currentGear == 0 && Backward == true)
        {
            if (speed < bikeSetting.gears[0] * -10)
                accel = -accel; // in automatic mode we need to hold arrow down for reverse
        }
        else
        {
            Backward = false;
        }

        wantedRPM = (5500.0f * accel) * 0.1f + wantedRPM * 0.9f;

        float rpm = 0.0f;
        int motorizedWheels = 0;
        bool floorContact = false;
        int currentWheel = 0;

        foreach (WheelComponent w in wheels)
        {
            WheelHit hit;
            WheelCollider col = w.collider;

            if (w.drive)
            {
                if (!NeutralGear && brake && currentGear < 2)
                {
                    rpm += accel * bikeSetting.idleRPM;

                }
                else
                {
                    if (!NeutralGear)
                    {
                        rpm += col.rpm;
                    }
                    else
                    {
                        rpm += ((bikeSetting.idleRPM * 2.0f) * accel);
                    }
                }

                motorizedWheels++;
            }


            if (crash)
            {
                w.collider.enabled = false;
                w.wheel.GetComponent<Collider>().enabled= true;
            }
            else
            {
                w.collider.enabled = true;
                w.wheel.GetComponent<Collider>().enabled = false;
            }




            if (brake || accel < 0.0f)
            {
                if ((accel < 0.0f) || (brake && w == wheels[1]))
                {

                    if (brake && (accel > 0.0f))
                    {
                        slip = Mathf.Lerp(slip, bikeSetting.slipBrake, accel * 0.01f);
                    }
                    else if (speed > 1.0f)
                    {
                        slip = Mathf.Lerp(slip, 1.0f, 0.002f);
                    }
                    else
                    {
                        slip = Mathf.Lerp(slip, 1.0f, 0.02f);
                    }


                    wantedRPM = 0.0f;
                    col.brakeTorque = bikeSetting.brakePower;
                    w.rotation = w_rotate;

                }
            }
            else
            {
                col.brakeTorque = accel == 0 ? col.brakeTorque = 3000 : col.brakeTorque = 0;
                slip = Mathf.Lerp(slip, 1.0f, 0.02f);
                w_rotate = w.rotation;
            }
       
            WheelFrictionCurve fc = col.forwardFriction;

            if (w == wheels[1])
            {

                fc.stiffness = bikeSetting.stiffness / slip;
                col.forwardFriction = fc;
                fc = col.sidewaysFriction;
                fc.stiffness = bikeSetting.stiffness / slip;
                col.sidewaysFriction = fc; 
            }

            if (shift && (currentGear > 1 && speed > 50.0f) && shifmotor)
            {
                shifting = true;
                if (powerShift == 0) { shifmotor = false; }

                powerShift = Mathf.MoveTowards(powerShift, 0.0f, Time.deltaTime * 10.0f);

                bikeSounds.nitro.volume = Mathf.Lerp(bikeSounds.nitro.volume, 0.3f, Time.deltaTime * 10.0f);

                if (!bikeSounds.nitro.isPlaying)
                {
                    bikeSounds.nitro.GetComponent<AudioSource>().Play();

                }

                curTorque = powerShift > 0 ? bikeSetting.shiftPower : bikeSetting.bikePower;
                bikeParticles.shiftParticle1.emissionRate = Mathf.Lerp(bikeParticles.shiftParticle1.emissionRate, powerShift > 0 ? 50 : 0, Time.deltaTime * 10.0f);
                bikeParticles.shiftParticle2.emissionRate = Mathf.Lerp(bikeParticles.shiftParticle2.emissionRate, powerShift > 0 ? 50 : 0, Time.deltaTime * 10.0f);
            }
            else
            {
                shifting = false;

                if (powerShift > 20)
                {
                    shifmotor = true;
                }

                bikeSounds.nitro.volume = Mathf.MoveTowards(bikeSounds.nitro.volume, 0.0f, Time.deltaTime * 2.0f);

                if (bikeSounds.nitro.volume == 0)
                    bikeSounds.nitro.Stop();

                powerShift = Mathf.MoveTowards(powerShift, 100.0f, Time.deltaTime * 5.0f);
                curTorque = bikeSetting.bikePower;
                bikeParticles.shiftParticle1.emissionRate = Mathf.Lerp(bikeParticles.shiftParticle1.emissionRate, 0, Time.deltaTime * 10.0f);
                bikeParticles.shiftParticle2.emissionRate = Mathf.Lerp(bikeParticles.shiftParticle2.emissionRate, 0, Time.deltaTime * 10.0f);
            }


            w.rotation = Mathf.Repeat(w.rotation + Time.deltaTime * col.rpm * 360.0f / 60.0f, 360.0f);
            w.wheel.localRotation = Quaternion.Euler(w.rotation,0.0f, 0.0f);

            Vector3 lp = w.axle.localPosition;

            if (col.GetGroundHit(out hit) && (w == wheels[1] || (w == wheels[0] && Wheelie == 0)))
            {


                if (bikeParticles.brakeParticlePrefab)
                {
                    if (Particle[currentWheel] == null)
                    {
                        Particle[currentWheel] = Instantiate(bikeParticles.brakeParticlePrefab, w.wheel.position, Quaternion.identity) as GameObject;
                        Particle[currentWheel].name = "WheelParticle";
                        Particle[currentWheel].transform.parent = transform;
                        Particle[currentWheel].AddComponent<AudioSource>();
                        Particle[currentWheel].GetComponent<AudioSource>().volume = 0.2f;
                        Particle[currentWheel].GetComponent<AudioSource>().maxDistance = 50;
                        Particle[currentWheel].GetComponent<AudioSource>().spatialBlend = 1;
                        Particle[currentWheel].GetComponent<AudioSource>().dopplerLevel = 5;
                        Particle[currentWheel].GetComponent<AudioSource>().rolloffMode = AudioRolloffMode.Custom;
                    }


                    var pc = Particle[currentWheel].GetComponent<ParticleSystem>();
                    bool WGrounded = false;


                    for (int i = 0; i < bikeSetting.hitGround.Length; i++)
                    {

                        if (hit.collider.CompareTag(bikeSetting.hitGround[i].tag))
                        {
                            WGrounded = bikeSetting.hitGround[i].grounded;

                            if ((brake || Mathf.Abs(hit.sidewaysSlip) > 0.5f) && speed > 1)
                            {
                                Particle[currentWheel].GetComponent<AudioSource>().clip = bikeSetting.hitGround[i].brakeSound;
                            }
                            else if (Particle[currentWheel].GetComponent<AudioSource>().clip != bikeSetting.hitGround[i].groundSound && !Particle[currentWheel].GetComponent<AudioSource>().isPlaying)
                            {

                                Particle[currentWheel].GetComponent<AudioSource>().clip = bikeSetting.hitGround[i].groundSound;
                            }

                            Particle[currentWheel].GetComponent<ParticleSystem>().startColor = bikeSetting.hitGround[i].brakeColor;

                        }
                    }

                    if (WGrounded && speed > 5 && !brake)
                    {

                        pc.enableEmission = true;

                        Particle[currentWheel].GetComponent<AudioSource>().volume = 0.2f;

                        if (!Particle[currentWheel].GetComponent<AudioSource>().isPlaying)
                            Particle[currentWheel].GetComponent<AudioSource>().Play();

                    }
                    else if ((brake || Mathf.Abs(hit.sidewaysSlip) > 0.6f) && speed > 1)
                    {

                        if ((accel < 0.0f) || ((brake || Mathf.Abs(hit.sidewaysSlip) > 0.6f) &&w == wheels[1]))
                        {

                            if (!Particle[currentWheel].GetComponent<AudioSource>().isPlaying)
                                Particle[currentWheel].GetComponent<AudioSource>().Play();
                            pc.enableEmission = true;
                            Particle[currentWheel].GetComponent<AudioSource>().volume = Mathf.Clamp((speed / 60), 0, 0.5f);

                        }

                    }
                    else
                    {

                        pc.enableEmission = false;
                        Particle[currentWheel].GetComponent<AudioSource>().volume = Mathf.Lerp(Particle[currentWheel].GetComponent<AudioSource>().volume, 0, Time.deltaTime * 10.0f);
                    }

                }


                lp.y -= Vector3.Dot(w.wheel.position - hit.point, transform.TransformDirection(0, 1, 0)) - (col.radius);
                lp.y = Mathf.Clamp(lp.y, w.startPos.y - bikeWheels.setting.Distance, w.startPos.y + bikeWheels.setting.Distance);


                floorContact = floorContact || (w.drive);


                if (!crash)
                {
                    myRigidbody.angularDrag = 10.0f;
                }
                else
                {
                    myRigidbody.angularDrag = 0.0f;
                }


                grounded = true;

                if (w.collider.GetComponent<WheelSkidmarks>())
                w.collider.GetComponent<WheelSkidmarks>().enabled = true;

            }
            else
            {
                grounded = false;

                if (w.collider.GetComponent<WheelSkidmarks>())
                w.collider.GetComponent<WheelSkidmarks>().enabled = false;



                if (Particle[currentWheel] != null)
                {
                    var pc = Particle[currentWheel].GetComponent<ParticleSystem>();
                    pc.enableEmission = false;
                }

              

                lp.y = w.startPos.y - bikeWheels.setting.Distance;

                if (!wheels[0].collider.isGrounded && !wheels[1].collider.isGrounded)
                {

                    myRigidbody.centerOfMass = new Vector3(0, 0.2f, 0);
                    myRigidbody.angularDrag = 1.0f;

                    myRigidbody.AddForce(0, -10000, 0);
                }

            }


            currentWheel++;
            w.axle.localPosition = lp;

        }


        if (motorizedWheels > 1)
        {
            rpm = rpm / motorizedWheels;
        }


        motorRPM = 0.95f * motorRPM + 0.05f * Mathf.Abs(rpm * bikeSetting.gears[currentGear]);
        if (motorRPM > 5500.0f) motorRPM = 5200.0f;


        int index = (int)(motorRPM / efficiencyTableStep);
        if (index >= efficiencyTable.Length) index = efficiencyTable.Length - 1;
        if (index < 0) index = 0;



        float newTorque = curTorque * bikeSetting.gears[currentGear] * efficiencyTable[index];


        // go set torque to the wheels
        foreach (WheelComponent w in wheels)
        {
            WheelCollider col = w.collider;

            // of course, only the wheels connected to the engine can get engine torque
            if (w.drive)
            {

                if (Mathf.Abs(col.rpm) > Mathf.Abs(wantedRPM))
                {

                    col.motorTorque = 0;
                }
                else
                {
                    // 
                    float curTorqueCol = col.motorTorque;

                    if (!brake && accel != 0 && NeutralGear == false)
                    {
                        if ((speed < bikeSetting.LimitForwardSpeed && currentGear > 0) ||
                            (speed < bikeSetting.LimitBackwardSpeed && currentGear == 0))
                        {

                            col.motorTorque = curTorqueCol * 0.9f + newTorque * 1.0f;
                        }
                        else
                        {
                            col.motorTorque = 0;
                            col.brakeTorque = 2000;
                        }
                    }
                    else
                    {
                        col.motorTorque = 0;

                    }
                }

            }



            float SteerAngle = Mathf.Clamp((speed) / bikeSetting.maxSteerAngle, 1.0f, bikeSetting.maxSteerAngle);
            col.steerAngle = steer * (w.maxSteer / SteerAngle);

        }

        if (AIVScript.vehicleStatus != VehicleStatus.EmptyOff)
        {

            if (bikeSounds.IdleEngine != null)
            {

                float pitch = Mathf.Clamp(0.5f + ((motorRPM - bikeSetting.idleRPM) / (bikeSetting.shiftUpRPM - bikeSetting.idleRPM) * 0.5f), 0.0f, 10.0f);

                pitch = Mathf.Clamp(0.6f + ((motorRPM - bikeSetting.idleRPM) / (bikeSetting.shiftUpRPM - bikeSetting.idleRPM) * 0.5f), 0.0f, 10.0f);
                bikeSounds.IdleEngine.pitch = pitch;
                bikeSounds.IdleEngine.volume = Mathf.MoveTowards(bikeSounds.IdleEngine.volume, 0.5f + Mathf.Abs(accel), 0.01f);

            }
        }
    }



    /////////////// Show Normal Gizmos ////////////////////////////


    void OnDrawGizmos()
    {

        if (!bikeSetting.showNormalGizmos || Application.isPlaying) return;

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Gizmos.matrix = rotationMatrix;
        Gizmos.color = new Color(1, 0, 0, 0.5f);

        Gizmos.DrawCube(Vector3.up/1.6f, new Vector3(0.5f, 1.0f, 2.5f));
        Gizmos.DrawSphere(bikeSetting.shiftCentre, 0.2f);

    }



}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public class DemoController : MonoBehaviour
{
    public GameObject Robot;
    public ROSConnection ROSBackend;
    public ArticulationBody BaseArticulation;

    public List<float> ArticulationPositions;
    public List<float> ArticulationTargets;

    public List<string> JointNames;
    public List<ArticulationBody> JointArticulations;
    public List<ArticulationBody> GripperJoints;
    Dictionary<string, ArticulationBody> RobotJoints;
    Dictionary<string, List<float>> PreconfiguredPositions;

    Queue<IEnumerator> GripperControlQueue = new Queue<IEnumerator>();
    Queue<IEnumerator> ArmPlannedMotionQueue = new Queue<IEnumerator>();

    public TextAsset PlaybackFile;
    public bool ExecutePlayback = false;
    bool IsPlaybackExecuting = false;

    private void Start()
    {
        DefineRobotComponents();
        StartCoroutine(GripperQueueManagerCoroutine());

        // ROSBackend = ROSConnection.GetOrCreateInstance();
    }

    private void Update()
    {
        BaseArticulation.GetDriveTargets(ArticulationTargets);
        BaseArticulation.GetJointPositions(ArticulationPositions);

        if (PlaybackFile != null && ExecutePlayback == true && IsPlaybackExecuting == false)
        {
            StartCoroutine(PlaybackFromCSV());
        }
    }

    /* ------------------------------------------------------------------------------------------------------------------ */
    /*                                                     ARM CONTROL                                                    */
    /* ------------------------------------------------------------------------------------------------------------------ */

    private void MakeRobotRigid()
    {
        for (int i = 0; i < 6; i++)
        {
            ArticulationDrive drive = JointArticulations[i].xDrive;
            drive.stiffness = 5000f;
            drive.damping = 8000f;
            JointArticulations[i].xDrive = drive;
        }
    }

    private void MakeRobotCompliant()
    {
        for (int i = 0; i < JointArticulations.Count; i++)
        {
            ArticulationDrive drive = JointArticulations[i].xDrive;
            drive.stiffness = 0f;
            drive.damping = 30f;
            JointArticulations[i].xDrive = drive;
        }
    }

    private void SetJointTargetDegree(int index, float degree)
    {
        ArticulationDrive drive = JointArticulations[index].xDrive;
        drive.target = degree;
        JointArticulations[index].xDrive = drive;
    }

    /* ------------------------------------------------------------------------------------------------------------------ */
    /*                                                   GRIPPER CONTROL                                                  */
    /* ------------------------------------------------------------------------------------------------------------------ */

    public void CloseGripper() { GripperControlQueue.Enqueue(CloseGripperCoroutine()); }

    public void OpenGripper() { GripperControlQueue.Enqueue(OpenGripperCoroutine()); }

    private IEnumerator GripperQueueManagerCoroutine()
    {
        while (true)
        {
            if (GripperControlQueue.Count > 0)
            {
                yield return StartCoroutine(GripperControlQueue.Dequeue());
            }
            yield return null;
        }
    }

    private IEnumerator CloseGripperCoroutine(float timeToComplete = 1f, int steps = 25)
    {
        WaitForSecondsRealtime lag = new WaitForSecondsRealtime(timeToComplete / steps);
        float deltaAngle = (0.8f / steps) * Mathf.Rad2Deg;
        for (int i = 0; i < steps; i++)
        {
            for (int j = 0; j < GripperJoints.Count; j++)
            {
                ArticulationDrive drive = GripperJoints[j].xDrive;
                if (j == 0 || j == 3)
                {
                    drive.target -= deltaAngle;
                }
                else
                {
                    drive.target += deltaAngle;

                }
                GripperJoints[j].xDrive = drive;
            }
            yield return lag;
        }
    }

    private IEnumerator OpenGripperCoroutine(float timeToComplete = 1f, int steps = 25)
    {
        WaitForSecondsRealtime lag = new WaitForSecondsRealtime(timeToComplete / steps);
        float deltaAngle = (0.8f / steps) * Mathf.Rad2Deg;
        for (int i = 0; i < steps; i++)
        {
            foreach (ArticulationBody joint in GripperJoints)
            {
                ArticulationDrive drive = joint.xDrive;
                if (drive.target < 0)
                {
                    drive.target += deltaAngle;
                }
                else
                {
                    drive.target -= deltaAngle;
                }
                joint.xDrive = drive;
            }
            yield return lag;
        }
    }

    /* ------------------------------------------------------------------------------------------------------------------ */
    /*                                                  PLAYBACK FROM CSV                                                 */
    /* ------------------------------------------------------------------------------------------------------------------ */

    private IEnumerator PlaybackFromCSV()
    {
        if (PlaybackFile == null) yield return null;
        else
        {
            IsPlaybackExecuting = true;
            string fileContent = PlaybackFile.text.TrimEnd('\n');
            List<string> rawTrajectories = new List<string>(fileContent.Split("\n"));
            List<List<float>> stampedTrajectories = new List<List<float>>();
            foreach(string raw in rawTrajectories)
            {
                List<string> content = new List<string>(raw.Split(","));
                List<float> parsedValues = new List<float>();
                foreach(string rawValue in content)
                {
                    parsedValues.Add((float)Convert.ToDouble(rawValue));
                }
                stampedTrajectories.Add(parsedValues);
            }

            // this is a little naive...
            float averageDelay = stampedTrajectories[stampedTrajectories.Count - 1][0] / stampedTrajectories.Count;

            foreach(List<float> trajectory in stampedTrajectories)
            {
                List<float> jointTargets = new List<float>();
                List<float> jointVelocities = new List<float>();
                BaseArticulation.GetDriveTargets(jointTargets);
                BaseArticulation.GetDriveTargetVelocities(jointVelocities);

                if ((int)trajectory[0] == 0)
                {
                    SnapArmToConfiguration(trajectory.GetRange(1, 6));
                    continue;
                }


                // for (int i = 1; i < trajectory.Count; i++)
                // {
                    // for (int j = 0; j < 6; j++)
                    // {
                    //     jointTargets[j] = trajectory[i];
                    //     jointVelocities[j] = trajectory[i];
                    // }
                    // BaseArticulation.SetDriveTargets(jointTargets);
                    // BaseArticulation.SetDriveTargetVelocities(jointVelocities);
                    // SetJointTargetDegree(i - 1, trajectory[i] * Mathf.Rad2Deg);
                    // SnapArmToConfiguration(trajectory.GetRange(1, 6));
                // }

                for (int i = 0; i < 6; i++)
                {
                    jointTargets[i] = trajectory[i + 1];
                    jointVelocities[i] = trajectory[i + 7];
                }
                BaseArticulation.SetDriveTargets(jointTargets);
                BaseArticulation.SetDriveTargetVelocities(jointVelocities);

                yield return new WaitForSeconds(averageDelay);
            }

            yield return new WaitForSeconds(0.5f);
            IsPlaybackExecuting = false;
            ExecutePlayback = false;
        }
    }

    /* ------------------------------------------------------------------------------------------------------------------ */
    /*                                                  "CHEAT" FUNCTIONS                                                 */
    /* ------------------------------------------------------------------------------------------------------------------ */

    private void SnapJointToTargetAngle(int index, float target)
    {
        List<float> current = new List<float>();
        BaseArticulation.GetJointPositions(current);
        current[index] = target;
        BaseArticulation.SetDriveTargets(current);
        BaseArticulation.SetJointPositions(current);
    }

    private void SnapArmToPosition(string position)
    {
        if (!PreconfiguredPositions.ContainsKey(position))
        {
            Debug.Log($"{position} is not known in preset library...");
            return;
        }
        BaseArticulation.SetDriveTargets(PreconfiguredPositions[position]);
        BaseArticulation.SetJointPositions(PreconfiguredPositions[position]);
    }

    private void SnapArmToConfiguration(List<float> joints)
    {
        List<float> current = new List<float>();
        BaseArticulation.GetJointPositions(current);
        for (int i = 0; i < joints.Count; i++)
        {
            current[i] = joints[i];
        }
        BaseArticulation.SetDriveTargets(current);
        BaseArticulation.SetJointPositions(current);
    }

    /* ------------------------------------------------------------------------------------------------------------------ */
    /*                                             ROBOT AND OTHER DEFINITIONS                                            */
    /* ------------------------------------------------------------------------------------------------------------------ */

    private void DefineRobotComponents()
    {
        Robot = this.gameObject;

        string baseChain = "base_link/base_link_inertia";
        BaseArticulation = Robot.transform.Find(baseChain).GetComponent<ArticulationBody>();

        JointNames = new List<string>() {
            "shoulder_pan_joint",
            "shoulder_lift_joint",
            "elbow_joint",
            "wrist_1_joint",
            "wrist_2_joint",
            "wrist_3_joint",
            "left_inner_knuckle_joint",
            "left_inner_finger_joint",
            "finger_joint",
            "right_inner_knuckle_joint",
            "right_inner_finger_joint",
            "right_finger_joint",
        };

        JointArticulations = new List<ArticulationBody>() {
            Robot.transform.Find(baseChain + "/shoulder_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/left_knuckle_link/left_bar_link/left_distal_phalanx_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/left_proximal_phalanx_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/left_knuckle_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/right_knuckle_link/right_bar_link/right_distal_phalanx_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/right_proximal_phalanx_link").GetComponent<ArticulationBody>(),
            Robot.transform.Find(baseChain + "/shoulder_link/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link/flange/tool0/robotiq_2f_base_palm/right_knuckle_link").GetComponent<ArticulationBody>(),
        };

        GripperJoints = JointArticulations.GetRange(6, 6);

        RobotJoints = new Dictionary<string, ArticulationBody>();
        for (int i = 0; i < JointNames.Count; i++)
        {
            RobotJoints.Add(JointNames[i], JointArticulations[i]);
        }

        PreconfiguredPositions = new Dictionary<string, List<float>>() {
            {"zero", new List<float>(new float[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0})},
            {"home", new List<float>(new float[] {0, -Mathf.PI / 2.0f, 0, -Mathf.PI / 2.0f, 0, 0, 0, 0, 0, 0, 0, 0})}
        };

        MakeRobotRigid();
        SnapArmToPosition("home");
    }
}
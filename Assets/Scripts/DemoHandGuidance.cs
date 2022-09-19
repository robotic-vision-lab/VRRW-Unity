using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoHandGuidance : MonoBehaviour
{
    public bool Freedrive;
    public GameObject RobotGameObject;
    public List<ArticulationBody> JointArticulations;

    public float LoggingRate = 10f;
    WaitForSecondsRealtime LoggingDelay;
    string LogFileName;

    // Start is called before the first frame update
    void Start()
    {
        Freedrive = false;
        RobotGameObject = gameObject;
        JointArticulations = FindSubsequentJoints();
        LoggingDelay = new WaitForSecondsRealtime(1.0f / LoggingRate);
        LogFileName = "DMPLogs/Trajectory-" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".csv";

        // create initial file
        var dummy = File.Create(LogFileName);
        dummy.Dispose();

        // start logging CoRoutine
        // StartCoroutine(LoggingCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (!Freedrive) MakeArmStiff();
        else MakeArmCompliant();

        string logging = Time.realtimeSinceStartup.ToString() + ", ";
        string joints = "";
        string velocities = "";
        foreach(ArticulationBody ab in JointArticulations)
        {
            float currentTarget = ab.jointPosition[0];
            float currentAngularVelocity = ab.jointVelocity[0];
            joints += currentTarget.ToString() + ", ";
            velocities += currentAngularVelocity + ", ";
            var drive = ab.xDrive;
            if (Freedrive)
            {
                drive.target = currentTarget * Mathf.Rad2Deg;
                ab.xDrive = drive;
            }
        }
        if (Freedrive)
        {
            using (FileStream fs = new FileStream(LogFileName, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs)) sw.WriteLine(logging + joints + velocities);
        }
        else
        {
            using (FileStream fs = new FileStream(LogFileName, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs)) sw.WriteLine(logging + "0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ");
        }
        Debug.Log(logging + joints + velocities);
    }

    // IEnumerator LoggingCoroutine()
    // {
    //     while (true)
    //     {
    //         if (Freedrive)
    //         {
    //             string logging = Time.realtimeSinceStartup.ToString() + ", ";
    //             string joints = "";
    //             string velocities = "";
    //             foreach(ArticulationBody ab in JointArticulations)
    //             {
    //                 float currentTarget = ab.jointPosition[0];
    //                 float currentAngularVelocity = ab.jointVelocity[0];
    //                 joints += currentTarget.ToString() + ", ";
    //                 velocities += currentAngularVelocity + ", ";
    //                 var drive = ab.xDrive;
    //                 if (Freedrive)
    //                 {
    //                     drive.target = currentTarget * Mathf.Rad2Deg;
    //                     ab.xDrive = drive;
    //                 }
    //             }
    //             logging += (joints + velocities);
    //             using (FileStream fs = new FileStream(LogFileName, FileMode.Append, FileAccess.Write))
    //             using (StreamWriter sw = new StreamWriter(fs))
    //             {
    //                 sw.WriteLine(logging);
    //             }
    //         }
    //         yield return new WaitForSecondsRealtime(0.1f);
    //     }
    // }

    // Get joints
    List<ArticulationBody> FindSubsequentJoints()
    {
        string searchPath = "base_link/base_link_inertia/shoulder_link";

        return new List<ArticulationBody>() {
            RobotGameObject.transform.Find(searchPath).GetComponent<ArticulationBody>(),
            RobotGameObject.transform.Find(searchPath + "/upper_arm_link").GetComponent<ArticulationBody>(),
            RobotGameObject.transform.Find(searchPath + "/upper_arm_link/forearm_link").GetComponent<ArticulationBody>(),
            RobotGameObject.transform.Find(searchPath + "/upper_arm_link/forearm_link/wrist_1_link").GetComponent<ArticulationBody>(),
            RobotGameObject.transform.Find(searchPath + "/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link").GetComponent<ArticulationBody>(),
            RobotGameObject.transform.Find(searchPath + "/upper_arm_link/forearm_link/wrist_1_link/wrist_2_link/wrist_3_link").GetComponent<ArticulationBody>(),
        };
    }

    // Make arm stiff
    void MakeArmStiff()
    {
        for (int i = 0; i < JointArticulations.Count; i++)
        {
            ArticulationBody joint = JointArticulations[i];
            ArticulationDrive drive = joint.xDrive;
            drive.stiffness = 100000f;
            drive.damping = 20000f;

            // straight vertical
            // drive.target = (i == 1 || i == 3) ? -90f : 0f;

            // right angle
            // if (i == 1) drive.target = -90f;
            // else if (i == 2) drive.target = 90f;
            // else if (i == 5) drive.target = -180f;
            // else drive.target = 0;

            if (i == 1 || i == 2) drive.target = -90f;
            else if (i == 3) drive.target = 270f;
            else if (i == 4) drive.target = 90f;
            else drive.target = 0;

            joint.xDrive = drive;
        }
    }

    // Make arm compliant
    void MakeArmCompliant()
    {
        for (int i = 0; i < JointArticulations.Count; i++)
        {
            ArticulationBody joint = JointArticulations[i];
            ArticulationDrive drive = joint.xDrive;
            drive.stiffness = 0f;
            drive.damping = (i <= 2) ? 30f : 30f;
            joint.xDrive = drive;
        }
    }
}

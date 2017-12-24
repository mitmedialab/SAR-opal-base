// Jacqueline Kory Westlund
// December 2017
//
// The MIT License (MIT)
// Copyright (c) 2016 Personal Robots Group
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Net;

namespace opal
{
    /// <summary>
    /// This behavior allows the user to enter information about a game session'
    /// prior to the start of the session, such as a participant/user ID and an
    /// IP address for connecting to ROS.
    /// </summary>
    public class StartupROSSession : MonoBehaviour
    {
        GameObject validationText;
        void OnAwake()
        {
        }
        void OnEnable()
        {
            this.validationText = GameObject.FindGameObjectWithTag(Constants.TAG_VALIDATION);
            this.validationText.GetComponent<Text>().enabled = false;
        }

        void OnDestroy()
        {
        }

        void Update()
        {
            // if user presses escape or 'back' button on android, exit program
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        }

        public void OnROSIPChanged(string ip)
        {
            Logger.Log("IP changed: " + ip);
            IPAddress addr;
            // Check whether the string entered in the ROSMASTER IP address box
            // was actually a valid IP address. If not, don't save it.
            if (ip.Split('.').Length != 4 || !IPAddress.TryParse(ip, out addr))
            {
                Logger.Log("Not valid IP!");
                this.validationText.GetComponent<Text>().enabled = true;
                return;
            }
            else
            {
                Logger.Log("Valid IP!");
                this.validationText.GetComponent<Text>().enabled = false;
                // The ROSMASTER_IP is a static variable so we can access it from
                // another scene.
                Constants.ROSMASTER_IP = ip;
            }
        }

        public void OnPIDChanged(string pid)
        {
            // The PID is a static variable so we can access it from another scene.
            Logger.Log("PID changed: " + pid);
            Constants.PID = pid;
        }

        public void OnStartButtonClick()
        {
            Logger.Log("Attempting to start game with ROSMASTER IP: " + 
                Constants.ROSMASTER_IP + " and PID: " + Constants.PID);
            SceneManager.LoadScene(Constants.START_SCENE);
        }

    }
}


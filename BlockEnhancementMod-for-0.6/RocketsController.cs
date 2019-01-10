﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Blocks;
using Modding.Levels;
using BlockEnhancementMod.Blocks;

namespace BlockEnhancementMod
{
    class RocketsController : SingleInstance<RocketsController>
    {
        public override string Name { get; } = "Rockets Controller";

        private bool iAmLockedByRocket = false;
        private bool isFirstFrame = true;
        public static bool DisplayWarning { get; internal set; } = true;
        private FixedCameraController cameraController;
        public Dictionary<BlockBehaviour, int> rocketTargetDict;
        public Dictionary<int, Dictionary<KeyCode, Stack<TimedRocket>>> playerGroupedRockets;
        public bool launchStarted = false;
        private static readonly float transparancy = 0.5f;
        private static readonly float screenOffset = 128f;
        private static readonly float warningHeight = 60f;
        private static readonly float warningWidth = 180f;
        private static readonly float borderThickness = 10f;
        private static readonly Color warningBorderColor = new Color(1, 0, 0, transparancy);
        private static readonly Rect warningRect = new Rect(Screen.width - screenOffset - warningWidth, Screen.height - screenOffset - warningHeight, warningWidth, warningHeight);
        private static Texture2D redTexture;
        private static Texture2D RedTexture
        {
            get
            {
                if (redTexture == null)
                {
                    redTexture = new Texture2D(1, 1);
                    redTexture.SetPixel(0, 0, warningBorderColor);
                    redTexture.Apply();
                }

                return redTexture;
            }
        }

        public RocketsController()
        {
            rocketTargetDict = new Dictionary<BlockBehaviour, int>();
            playerGroupedRockets = new Dictionary<int, Dictionary<KeyCode, Stack<TimedRocket>>>();
        }

        void FixedUpdate()
        {
            if (!StatMaster.levelSimulating)
            {
                if (playerGroupedRockets.Count > 0)
                {
                    playerGroupedRockets.Clear();
                }
            }
            if (PlayerMachine.GetLocal() != null)
            {
                if (PlayerMachine.GetLocal().InternalObject.isSimulating)
                {
                    if (isFirstFrame)
                    {
                        isFirstFrame = launchStarted = false;
                        cameraController = FindObjectOfType<FixedCameraController>();
                        rocketTargetDict.Clear();
                    }
                }
                else
                {
                    if (!isFirstFrame)
                    {
                        rocketTargetDict.Clear();
                        isFirstFrame = true;
                    }
                }
            }
            if (!isFirstFrame)
            {
                if (PlayerMachine.GetLocal() != null && rocketTargetDict != null && !isFirstFrame)
                {
                    try
                    {
                        foreach (var rocketTargetPair in rocketTargetDict)
                        {
                            if (!rocketTargetPair.Key.ParentMachine.isSimulating)
                            {
                                RemoveRocketTarget(rocketTargetPair.Key);
                            }
                        }
                    }
                    catch { }
                    if (rocketTargetDict.Count == 0)
                    {
                        iAmLockedByRocket = false;
                    }
                    else
                    {
                        foreach (var rocketTargetPair in rocketTargetDict)
                        {
                            if (PlayerMachine.GetLocal() != null)
                            {
                                if (rocketTargetPair.Value == (StatMaster.isMP ? PlayerMachine.GetLocal().Player.NetworkId : 0))
                                {
                                    iAmLockedByRocket = true;
                                    break;
                                }
                                else
                                {
                                    iAmLockedByRocket = false;
                                }
                            }
                            else
                            {
                                iAmLockedByRocket = false;
                            }
                        }

                    }
                }
            }
        }

        readonly GUIStyle missileWarningStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = new Color(1, 0, 0, transparancy) },
            alignment = TextAnchor.MiddleCenter,
        };

        private void DrawBorder()
        {
            //Top
            GUI.DrawTexture(new Rect(warningRect.xMin, warningRect.yMin, warningRect.width, borderThickness), RedTexture);
            // Left
            GUI.DrawTexture(new Rect(warningRect.xMin, warningRect.yMin + borderThickness, borderThickness, warningRect.height - 2 * borderThickness), RedTexture);
            // Right
            GUI.DrawTexture(new Rect(warningRect.xMax - borderThickness, warningRect.yMin + borderThickness, borderThickness, warningRect.height - 2 * borderThickness), RedTexture);
            // Bottom
            GUI.DrawTexture(new Rect(warningRect.xMin, warningRect.yMax - borderThickness, warningRect.width, borderThickness), RedTexture);
        }

        private void OnGUI()
        {
            if (iAmLockedByRocket && DisplayWarning)
            {
                if (cameraController != null)
                {
                    if (cameraController.activeCamera != null)
                    {
                        if (cameraController.activeCamera.CamMode == FixedCameraBlock.Mode.FirstPerson)
                        {
                            DrawBorder();
                            GUI.Box(warningRect, "Missile Alert", missileWarningStyle);
                        }
                    }
                }
            }
        }

        public void UpdateRocketTarget(BlockBehaviour rocket, int targetMachineID)
        {
            if (rocketTargetDict.ContainsKey(rocket))
            {
                rocketTargetDict[rocket] = targetMachineID;
            }
            else
            {
                rocketTargetDict.Add(rocket, targetMachineID);
            }
        }

        public void RemoveRocketTarget(BlockBehaviour rocket)
        {
            if (rocketTargetDict.ContainsKey(rocket))
            {
                rocketTargetDict.Remove(rocket);
            }
        }

        public IEnumerator LaunchRocketFromGroup(int id, KeyCode key)
        {
            launchStarted = true;
            float defaultDelay = 0.25f;
            if (playerGroupedRockets.TryGetValue(id, out Dictionary<KeyCode, Stack<TimedRocket>> timedRocketDict))
            {
                if (timedRocketDict.TryGetValue(key, out Stack<TimedRocket> timedRockets))
                {
                    if (timedRockets.Count > 0)
                    {
                        TimedRocket rocket = timedRockets.Pop();
                        if (rocket != null)
                        {
                            RocketScript rocketScript = rocket.GetComponent<RocketScript>();
                            if (rocketScript != null)
                            {
                                rocket.Fire(0f);
                                rocket.hasFired = true;
                                rocket.hasExploded = false;
                                if (rocketScript.autoGrabberRelease && rocket.grabbers.Count > 0)
                                {
                                    foreach (var grabber in rocket.grabbers)
                                    {
                                        grabber?.StartCoroutine(grabber.IEBreakJoint());
                                    }
                                }
                                defaultDelay = Mathf.Clamp(rocketScript.groupFireRate, 0.1f, 1f);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < defaultDelay * 100; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            launchStarted = false;
            yield return null;
        }
    }
}

using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using PegasusShared;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Runtime.InteropServices;
using Blizzard.T5.Core.Time;

namespace HearthPlugin
{
    //插件描述特性 分别为 插件ID 插件名字 插件版本(必须为数字)
    [BepInPlugin("HearthPlugin", "SpeedPlugin", "2.0")]
    [BepInProcess("Hearthstone.exe")]
    public class SpeedPlugin : BaseUnityPlugin //继承BaseUnityPlugin
    {
        static ConfigEntry<float> SpeedConfig;
        static ConfigEntry<KeyCode> HotKey;
        static ConfigEntry<bool> enableBattleConfig;
        static ConfigEntry<bool> enableConfig;
        static ConfigEntry<string> VersionInfoConfig;
        static ConfigEntry<string> KeyConfig;

        //是否是酒馆模式
        private static bool isInBattleGround;
        //战斗动画加速开关
        private static bool enableBattle;
        //整活动画加速开关
        private static bool isEnabled;
        //战斗状态
        private static bool brawlMode;
        private static bool isShow;
        private static string versionInfo;
        private static bool tempBattleInc;
        private static string showGUIstr;
        private static float showGUItime;
        private static float showGUIduration;

        private static string keyInfo;
        private static string machineInfo;
        private static bool enableKey;

        private static bool IsInBattleGround { get => isInBattleGround; set => isInBattleGround = value; }
        private static bool EnableBattle { get => enableBattle; set => enableBattle = value; }
        private static bool IsShow { get => isShow; set => isShow = value; }


        private static string VersionInfo { get => versionInfo; set => versionInfo = value; }
        private static bool IsEnabled { get => isEnabled; set => isEnabled = value; }
        private static bool BrawlMode { get => brawlMode; set => brawlMode = value; }
        private static bool TempBattleInc { get => tempBattleInc; set => tempBattleInc = value; }
        private static string ShowGUIstr { get => showGUIstr; set => showGUIstr = value; }
        private static float ShowGUItime { get => showGUItime; set => showGUItime = value; }
        private static float ShowGUIduration { get => showGUIduration; set => showGUIduration = value; }
        private static string KeyInfo { get => keyInfo; set => keyInfo = value; }
        private static string MachineInfo { get => machineInfo; set => machineInfo = value; }
        private static bool EnableKey { get => enableKey; set => enableKey = value; }
        private static string ExpireTime { get; set; }
        private static string LoadMessage { get; set; }
        private static bool IsExpire { get; set; }
        private static float ShowExpireTime { get; set; }




        //Unity的Start生命周期
        void Start()
        {
            ConfigBind();
            EnableKey = true;
            Harmony.CreateAndPatchAll(typeof(SpeedPlugin));
            base.StartCoroutine(this.Enumerator());

        }

        void ConfigBind()
        {
            SpeedConfig = Config.Bind<float>("config", "Speed", 5f, "自定义变速倍数，变速范围0.2-5");
            SpeedConfig.Value = Mathf.Clamp(SpeedConfig.Value, 0.2f, 5);
            HotKey = Config.Bind<KeyCode>("config", "Hotkey", KeyCode.F8, "插件信息界面的快捷键");
            enableBattleConfig = Config.Bind<bool>("config", "enableBattle", true, "酒馆战斗动画加速开关");
            EnableBattle = enableBattleConfig.Value;
            VersionInfoConfig = Config.Bind<string>("config", "VersionInfo", "炉石插件V2.0", "版本信息");
            VersionInfoConfig.Value = "炉石插件V2.0";
            if (EnableKey)
            {
                VersionInfo = "炉石插件VIP版V2.0";
            }
            else
            {
                VersionInfo = VersionInfoConfig.Value;
            }
            enableConfig = Config.Bind<bool>("config", "enabled", true, "酒馆整活动画开关");
            IsEnabled = enableConfig.Value;
            KeyConfig = Config.Bind<string>("config", "key", "xxxxxxx", "注册码信息");
            KeyInfo = KeyConfig.Value;
            ShowGUItime = Time.realtimeSinceStartup;
            ShowGUIstr = VersionInfo + "已加载！";
            ShowGUIduration = 12f;
            ExpireTime = "";
            ShowExpireTime = 0f;
            //输出日志
            Logger.LogInfo("Config init!");
        }

       




        void Update()
        {
            CheckInBattleGround();
            CheckExpire();
            CheckKeyDown();

        }

        private bool CheckExpire()
        {
            if (ExpireTime.Equals("") || ExpireTime == null)
            {
                return IsExpire = false;
            }
            DateTime expireDateTime = Convert.ToDateTime(ExpireTime);
            DateTime now = DateTime.Now;
            if (DateTime.Compare(now, expireDateTime) > 0)
            {
                if (ShowExpireTime.Equals(0f))
                {
                    ShowGUIstr = "插件注册码已过期，请加群865615441获取注册码重新激活！";
                    ShowExpireTime = Time.realtimeSinceStartup;
                    ShowGUIduration = 300f;
                }
                return IsExpire = true;
            }
            else
            {
                return IsExpire = false;
            }
        }

        private void CheckKeyDown()
        {
            if (!IsExpire && EnableKey)
            {
                //插件信息界面开关
                if (Input.GetKeyDown(HotKey.Value))
                {
                    IsShow = !IsShow;
                }
                //整活动画加速开关
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    IsEnabled = !IsEnabled;
                    ShowGUIstr = "整活动画加速已" + (IsEnabled ? "开启" : "关闭");
                    ShowGUItime = Time.realtimeSinceStartup;
                    ShowGUIduration = 1f;
                    if (IsEnabled)
                    {
                        enableConfig.Value = true;
                    }
                    else
                    {
                        enableConfig.Value = false;
                    }

                }
                //战斗动画加速
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    EnableBattle = !EnableBattle;
                    ShowGUIstr = "战斗动画加速已" + (EnableBattle ? "开启" : "关闭");
                    ShowGUItime = Time.realtimeSinceStartup;
                    ShowGUIduration = 1f;
                    if (EnableBattle)
                    {
                        enableBattleConfig.Value = true;
                    }
                    else
                    {
                        enableBattleConfig.Value = false;
                    }

                }
                //跳过动画快捷开关
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    if (IsInBattleGround)
                    {
                        Network network = Network.Get();
                        if (network != null)
                        {
                            network.DisconnectFromGameServer();
                        }
                    }
                    else
                    {
                        ShowGUIstr = "只能在酒馆内拔线！！！";
                        ShowGUItime = Time.realtimeSinceStartup;
                        ShowGUIduration = 1f;
                    }

                }
            }
            else if (IsExpire || !EnableKey)
            {
                IsShow = false;
                enableConfig.Value = false;
                enableBattleConfig.Value = false;
                //跳过动画快捷开关
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    if (IsInBattleGround)
                    {
                        Network network = Network.Get();
                        if (network != null)
                        {
                            network.DisconnectFromGameServer();
                        }
                    }
                    else
                    {
                        ShowGUIstr = "只能在酒馆内拔线！！！";
                        ShowGUItime = Time.realtimeSinceStartup;
                        ShowGUIduration = 1f;
                    }

                }
            }

        }

        void OnGUI()
        {

            ShowWelcome();
            ShowPlugInfo();


        }
        //插件加载信息
        private void ShowWelcome()
        {
            if (IsExpire && EnableKey && (Time.realtimeSinceStartup - ShowExpireTime < ShowGUIduration))
            {
                GUI.Label(new Rect(10f, 10f, 800f, 40f), ShowGUIstr);
            }

            if (!EnableKey || IsExpire)
            {
                GUI.Label(new Rect(10f, 10f, 800f, 40f), "炉石插件交流qq群：865615441 免费使用，请加群获取KEY");
            }
            else if (Time.realtimeSinceStartup - ShowGUItime < ShowGUIduration)
            {
                GUI.Label(new Rect(10f, 10f, 800f, 40f), ShowGUIstr);
            }

        }

        private void ShowPlugInfo()
        {
            if (EnableKey)
            {
                if (IsShow)
                {
                    GUI.Window(666, new Rect(50, 50, 250, 300), GUIplugFunc, VersionInfo);
                }
            }

        }

        void GUIplugFunc(int id)
        {

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(new GUIContent(),GUI.skin.box);
            GUILayout.Label(new GUIContent(VersionInfo));
            GUILayout.Label(new GUIContent("炉石插件交流qq群：865615441"));
            GUILayout.Label(new GUIContent("快捷键信息：\n" +
                            "酒馆整活动画加速开关:F1 \n"+
                            "酒馆战斗动画加速开关:F2 \n"+
                            "跳过动画开关：F3 \n"+"插件信息界面开关:F8"));
            if (ExpireTime != null || !ExpireTime.Equals(""))
            {
                
                GUILayout.Label(new GUIContent("过期时间：" + ExpireTime));
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(new GUIContent(), GUI.skin.box);
            if (GUILayout.Button("整活动画", GUILayout.Width(80)))
            {
                IsEnabled = !IsEnabled;
                ShowGUIstr = "整活动画加速已" + (IsEnabled ? "开启" : "关闭");
                ShowGUItime = Time.realtimeSinceStartup;
                ShowGUIduration = 1f;
                if (IsEnabled)
                {
                    enableConfig.Value = true;
                }
                else
                {
                    enableConfig.Value = false;
                }
            }
            if (GUILayout.Button("战斗动画", GUILayout.Width(80)))
            {
                EnableBattle = !EnableBattle;
                ShowGUIstr = "战斗动画加速已" + (EnableBattle ? "开启" : "关闭");
                ShowGUItime = Time.realtimeSinceStartup;
                ShowGUIduration = 1f;
                if (EnableBattle)
                {
                    enableBattleConfig.Value = true;
                }
                else
                {
                    enableBattleConfig.Value = false;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(new GUIContent(), GUI.skin.box);
            if (GUILayout.Button("跳过动画", GUILayout.Width(80)))
            {
                if (IsInBattleGround)
                {
                    Network network = Network.Get();
                    if (network != null)
                    {
                        network.DisconnectFromGameServer();
                    }
                }
                else
                {
                    ShowGUIstr = "只能在酒馆内拔线！！！";
                    ShowGUItime = Time.realtimeSinceStartup;
                    ShowGUIduration = 1f;
                }
            }
            if (GUILayout.Button("关闭", GUILayout.Width(80)))
            {
                IsShow = false;
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();

        }


        /**
         * 拔线按钮方法
         */
        [HarmonyPostfix, HarmonyPatch(typeof(GameMenu), "GetButtons")]
        private static void ReconnectBtn(GameMenu __instance, List<UIBButton> __result)
        {
            if (!SpeedPlugin.IsInBattleGround)
            {
                return;
            }
            UIBButton item = GameMenu.Get().CreateMenuButton("reconnect", "跳过动画", delegate (UIEvent uievent_0)
            {
                Network network = Network.Get();
                if (network != null)
                {
                    network.DisconnectFromGameServer();
                }
                __instance.Hide();
            });
            __result.Insert(1, item);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameMenu), "GetButtons")]
        private static void PlugBtn(GameMenu __instance, List<UIBButton> __result)
        {
            if (!SpeedPlugin.IsInBattleGround && !IsExpire && EnableKey)
            {
                UIBButton item = GameMenu.Get().CreateMenuButton("PlugInfo", "炉石插件", delegate (UIEvent uievent_0)
                {
                    IsShow = true;
                    __instance.Hide();
                });
                __result.Insert(1, item);
            }

        }

        [HarmonyPrefix, HarmonyPatch(typeof(SpellStateAnimObject), "Play")]
        private static void AnimSpeed(SpellStateAnimObject __instance)
        {
            if (EnableBattle && IsInBattleGround && !IsExpire && EnableKey)
            {
                __instance.m_AnimSpeed = SpeedConfig.Value;
                return;
            }
        }

        /**
         * 判断是否酒馆模式
         */
        private void CheckInBattleGround()
        {
            if (!SpeedPlugin.IsInBattleGround)
            {
                SpeedPlugin.IsInBattleGround = true;
            }
            GameMgr gameMgr = GameMgr.Get();
            if (gameMgr == null || gameMgr.GetGameType() != GameType.GT_BATTLEGROUNDS)
            {
                GameMgr gameMgr2 = GameMgr.Get();
                if (gameMgr2 == null || gameMgr2.GetGameType() != GameType.GT_BATTLEGROUNDS_FRIENDLY)
                {
                    SpeedPlugin.IsInBattleGround = false;
                }

            }

        }

        private static string Gmethod(int int_1 = 2, int int_2 = 20)
        {
            string text = "";
            StackTrace stackTrace = new StackTrace();
            int num = int_1;
            while (num < stackTrace.FrameCount && num < int_2)
            {
                MethodBase method = stackTrace.GetFrame(num).GetMethod();
                text += string.Format("{0}.{1}\n", method.DeclaringType.FullName, method.Name);
                num++;
            }
            return text;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OptionsMenu), "Awake")]
        private static void OptionConfig(OptionsMenu __instance)
        {
            if (IsExpire || !EnableKey)
            {
                return;
            }
            if (!IsInBattleGround)
            {
                CheckBox screenShakeCheckbox = __instance.m_screenShakeCheckbox;
                if (ExpireTime != null || !ExpireTime.Equals(""))
                {
                    screenShakeCheckbox.m_uberText.Text = "\n开启屏幕抖动\n" + VersionInfo + "\n过期时间：" + ExpireTime;
                }
                else
                {
                    screenShakeCheckbox.m_uberText.Text = "\n开启屏幕抖动\n" + VersionInfo;
                }
                screenShakeCheckbox.m_uberText.UseEditorText = true;
                screenShakeCheckbox.m_uberText.GameStringLookup = false;
            }
            else
            {
                CheckBox checkbox = __instance.m_screenShakeCheckbox;
                checkbox.m_uberText.Text = "\n整活动画加速\n";
                checkbox.m_uberText.UseEditorText = true;
                checkbox.m_uberText.GameStringLookup = false;
                checkbox.SetChecked(IsEnabled);
                checkbox.SetEnabled(true, false);
                checkbox.ClearEventListeners();
                checkbox.AddEventListener(UIEventType.RELEASE, delegate (UIEvent uIEvent)
                {
                    IsEnabled = checkbox.IsChecked();
                    if (IsEnabled)
                    {
                        enableConfig.Value = true;
                    }
                    else
                    {
                        enableConfig.Value = false;
                    }
                });
                CheckBox checkbox2 = __instance.m_spectatorOpenJoinCheckbox;
                checkbox2.m_uberText.Text = "战斗动画加速";
                checkbox2.m_uberText.UseEditorText = true;
                checkbox2.m_uberText.GameStringLookup = false;
                checkbox2.SetChecked(EnableBattle);
                checkbox2.SetEnabled(true, false);
                checkbox2.ClearEventListeners();
                checkbox2.AddEventListener(UIEventType.RELEASE, delegate (UIEvent uievent_0)
                {
                    EnableBattle = checkbox2.IsChecked();
                    if (EnableBattle)
                    {
                        enableBattleConfig.Value = true;
                    }
                    else
                    {
                        enableBattleConfig.Value = false;
                    }

                });
            }
            typeof(OptionsMenu).GetMethod("UpdateUI", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[0]);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(iTween), "SetArgs", new Type[] { typeof(GameObject), typeof(Hashtable) })]
        private static void Method2(Hashtable args)
        {
            if (IsEnabled && IsInBattleGround && !BrawlMode && !IsExpire && EnableKey)
            {
                if (args.ContainsKey("oncompletetarget") && (args["oncompletetarget"].ToString()
                    .Contains("Zone_Friendly_Play") || args["oncompletetarget"].ToString().Contains("Zone_Friendly_Hand")))
                {
                    args["time"] = 0.01f;
                }
                return;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(WaitForSeconds), MethodType.Constructor, new Type[] { typeof(float) })]
        private static void Method3(ref float seconds)
        {
            if (IsEnabled && IsInBattleGround && !BrawlMode && !IsExpire && EnableKey)
            {

                if ((double)seconds > 0.01)
                {
                    string text = Gmethod(3, 8);
                    if (text.Contains("ActivateCreatorSpawnMinionSpell") || text.Contains("DelaySpellFinished")
                        || text.Contains("ActivateActorBattlecrySpell") || text.Contains("HandleMissionEventWithTiming")
                        || text.Contains("WaitForAnimation") || text.Contains("CompleteTasksFromMetaData") || text.Contains("WaitAndSpawn")
                        || text.Contains("SplatAnimCoroutine") || text.Contains("WaitUntilAnimationIsCompleteAndThenUnblockInput")
                        || text.Contains("WaitAndPrepareForDeathAnimation") || text.Contains("AnimatePlayToDeck"))
                    {
                        seconds = 0.01f;
                    }
                }
                return;
            }
        }
        [HarmonyPostfix, HarmonyPatch(typeof(ActionData), "CreateAction", new Type[] { typeof(ActionData.Context), typeof(int) })]
        private static void Method4(List<string> ___actionNames, int actionIndex, FsmStateAction __result)
        {
            if (IsEnabled && IsInBattleGround && !BrawlMode && !IsExpire && EnableKey)
            {

                if (___actionNames[actionIndex] == "HutongGames.PlayMaker.Actions.Wait")
                {
                    string text = Gmethod(2, 20);
                    if (text.Contains("Card.GetActorSpell") || text.Contains("CardEffect.GetSpell") || text.Contains("Actor.LoadSpell") || text.Contains("TriggerSpellController.ActivateCardEffects"))
                    {
                        ((Wait)__result).time.Value = 0.001f;
                    }
                }
                return;
            }
        }

        private IEnumerator Enumerator()
        {
            return new SpeedPlugin.Ss(0);
        }

        private sealed class Ss : IEnumerator<object>
        {
            private int state;
            private object current;

            public Ss(int v)
            {
                this.state = v;
            }

            object IEnumerator<object>.Current => this.current;

            object IEnumerator.Current => this.current;


            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public bool MoveNext()
            {

                switch (this.state)
                {
                    case 0:
                        this.state = -1;
                        break;
                    case 1:
                        this.state = -1;
                        try
                        {

                            if (IsInBattleGround)
                            {
                                GameState gameState = GameState.Get();
                                string text = null;
                                if (gameState != null)
                                {
                                    global::Player opposingSidePlayer = gameState.GetOpposingSidePlayer();
                                    if (opposingSidePlayer != null)
                                    {
                                        global::Entity hero = opposingSidePlayer.GetHero();
                                        text = ((hero != null) ? hero.GetCardId() : null);
                                    }
                                    if (text != null)
                                    {
                                        if (!BrawlMode && !text.Contains("TB_BaconShopBob") && EnableBattle)
                                        {
                                            BrawlMode = true;
                                            TimeScaleMgr.Get().PushTemporarySpeedIncrease(4f);
                                            TempBattleInc = true;
                                            UnityEngine.Debug.LogError("进入战斗状态");

                                        }
                                        else if (BrawlMode && text.Contains("TB_BaconShopBob") && TempBattleInc)
                                        {
                                            BrawlMode = false;
                                            TimeScaleMgr.Get().PopTemporarySpeedIncrease();
                                            TempBattleInc = false;
                                            UnityEngine.Debug.LogError("结束战斗状态");
                                        }
                                    }
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError(ex.Message);
                        }
                        break;

                    case -1:
                        return false;
                }

                this.current = new WaitForSeconds(1f);
                this.state = 1;
                return true;

            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        public class Messagebox
        {

            [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
            public static extern int MessageBox(IntPtr handle, string message, string title, int type);
        }



    }
}


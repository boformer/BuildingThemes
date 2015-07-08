using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace BuildingThemes.GUI
{
    public static class PolicyPanelEnabler
    {
        public static bool Unlock 
        {
            get 
            {
                return BuildingThemesManager.instance.Configuration.UnlockPolicyPanel;
            }
            set
            {
                BuildingThemesManager.instance.Configuration.UnlockPolicyPanel = value;
                BuildingThemesManager.instance.SaveConfig();

                if (registered) UnlockPolicyToolbarButton();
            }
        }

        private static bool registered = false;

        public static void Register() 
        {
            if (!registered)
            {
                UnlockManager.instance.m_milestonesUpdated += new Action(TimedUnlock);
                registered = true;
            }

        }

        public static void Unregister()
        {
            if (registered)
            {
                UnlockManager.instance.m_milestonesUpdated -= new Action(TimedUnlock);
                registered = false;
            }
        }

        public static void TimedUnlock() 
        {
            if(Unlock) 
            {
                // Hook up the Elapsed event for the timer. 
                var timer = new Timer(300);
            
                timer.Elapsed += delegate(System.Object source, ElapsedEventArgs e) 
                {
                    timer.Enabled = false;
                    timer.Dispose();
                    UnlockPolicyToolbarButton();
                };
                timer.Enabled = true;
            }
        }
        
        public static void UnlockPolicyToolbarButton() 
        {
            if(Unlock) 
            {
                Debugger.Log("unlockPolicyToolbarButton");
                
                var uITabstrip = ToolsModifierControl.mainToolbar.component as UITabstrip;

                var policiesButtonTransform = ToolsModifierControl.mainToolbar.gameObject.transform.Find("Policies");

                if (policiesButtonTransform == null) return;

                Debugger.Log("unlocking");

                policiesButtonTransform.gameObject.GetComponent<UIButton>().isEnabled = true;
                
            }
        }
    }
}

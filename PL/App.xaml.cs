using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace PL
{
    public partial class App : Application
    {
        static App()
        {
            // Set WebBrowser control to use latest IE version
            SetBrowserFeatureControl();
        }

        private static void SetBrowserFeatureControl()
        {
            // http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

            // FeatureControl settings are per-process
            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            // make the control act like IE11
            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, 11001);

            // enable JavaScript
            SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
        }

    
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // הגדרת אבטחת WebBrowser לפני יצירת חלונות
            SetWebBrowserSecurityLevel();
        }

        private static void SetWebBrowserSecurityLevel()
        {
            // הגדרת רמת האבטחה של WebBrowser לנמוכה (Low) כדי לאפשר סקריפטים מקומיים
            try
            {
                string appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

                // הגדרת גרסת IE לגרסה האחרונה
                SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", appName, 11001); // IE11 Edge mode

                // ביטול אזהרות JavaScript
                SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", appName, 0);
                SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", appName, 1);
                SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", appName, 1);
                SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", appName, 1);
                SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", appName, 1);

                // הגדרות נוספות לביטול אזהרות אבטחה
                SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", appName, 0);
                SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING", appName, 1);
                SetBrowserFeatureControlKey("FEATURE_ENABLE_SCRIPT_PASTE_URLACTION_IF_PROMPT", appName, 1);

                // הגדרת רמת האבטחה של האזור המקומי (Local Machine Zone) לנמוכה
                string rootKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\0";
                Microsoft.Win32.Registry.SetValue(rootKey, "1400", 0, Microsoft.Win32.RegistryValueKind.DWord); // Enable ActiveX
                Microsoft.Win32.Registry.SetValue(rootKey, "1200", 0, Microsoft.Win32.RegistryValueKind.DWord); // Run ActiveX and plugins
                Microsoft.Win32.Registry.SetValue(rootKey, "1201", 0, Microsoft.Win32.RegistryValueKind.DWord); // Initialize ActiveX controls not marked as safe
                Microsoft.Win32.Registry.SetValue(rootKey, "1001", 0, Microsoft.Win32.RegistryValueKind.DWord); // JavaScript prompt
                Microsoft.Win32.Registry.SetValue(rootKey, "1004", 0, Microsoft.Win32.RegistryValueKind.DWord); // JavaScript download
                Microsoft.Win32.Registry.SetValue(rootKey, "1209", 0, Microsoft.Win32.RegistryValueKind.DWord); // Run .NET Framework-reliant components signed with Authenticode
                Microsoft.Win32.Registry.SetValue(rootKey, "1806", 0, Microsoft.Win32.RegistryValueKind.DWord); // Allow scripting of Internet Explorer WebBrowser control
                Microsoft.Win32.Registry.SetValue(rootKey, "2201", 0, Microsoft.Win32.RegistryValueKind.DWord); // ActiveX controls and plugins
                Microsoft.Win32.Registry.SetValue(rootKey, "2000", 0, Microsoft.Win32.RegistryValueKind.DWord); // Binary and script behaviors

                System.Diagnostics.Debug.WriteLine("WebBrowser security settings applied successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting WebBrowser security: {ex.Message}");
            }
        }

        private static void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                    string.Format(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\{0}", feature),
                    Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    key.SetValue(appName, value, Microsoft.Win32.RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting browser feature {feature}: {ex.Message}");
            }
        }
    }
        ///// <summary>
        ///// Interaction logic for App.xaml
        ///// </summary>
        //public partial class App : Application
        //{
        //}

   
}

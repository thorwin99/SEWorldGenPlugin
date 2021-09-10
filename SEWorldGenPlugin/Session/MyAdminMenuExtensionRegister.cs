using SEWorldGenPlugin.Utilities;
using System;
using System.Reflection;
using VRage.Game.Components;

namespace SEWorldGenPlugin.GUI.AdminMenu
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MyAdminMenuExtensionRegister : MySessionComponentBase
    {
        public override void LoadData()
        {
            LoadAllSubMenus();
        }

        protected override void UnloadData()
        {
            MyAdminMenuExtension.UnregisterAllSubMenus();
        }

        /// <summary>
        /// Loads all admin sub menus
        /// </summary>
        private void LoadAllSubMenus()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                MyPluginLog.Debug("Registering admin sub menus in: " + assembly.FullName);
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(MyPluginAdminMenuSubMenu)) && !type.IsGenericType)
                        {
                            MyPluginLog.Log("Registering admin sub menu " + type.Name);

                            MyPluginAdminMenuSubMenu sub = Activator.CreateInstance(type) as MyPluginAdminMenuSubMenu;
                            MyAdminMenuExtension.RegisterSubMenu(sub);
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    MyPluginLog.Log("Couldnt register admin sub menus for assembly " + assembly.FullName, LogLevel.WARNING);
                }
            }
        }
    }
}

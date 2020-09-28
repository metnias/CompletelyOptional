using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using System.Reflection;

namespace OptionalUI
{
    /// <summary>
    /// Use this support ConfigMachine without being dependent
    /// </summary>
    public class InterfaceWrapper : OptionInterface
    {
        /// <summary>
        /// Declare 'Ctor' method in your proxy to run anything in constructor
        /// </summary>
        /// <param name="mod">PartialityMod instance</param>
        /// <param name="proxy">proxy class</param>
        public InterfaceWrapper(PartialityMod mod, object proxy) : base(mod)
        {
            this.proxy = proxy;
            MethodInfo method = proxy.GetType().GetMethod("Ctor");
            if (method != null)
            {
                method.Invoke(proxy, new object[] { this });
            }
        }

        public object proxy;

        public override void ConfigOnChange()
        {
            MethodInfo method = this.proxy.GetType().GetMethod("ConfigOnChange");
            if (method == null) { base.ConfigOnChange(); return; }
            method.Invoke(this.proxy, new object[] { this });
        }

        public override void DataOnChange()
        {
            MethodInfo method = this.proxy.GetType().GetMethod("DataOnChange");
            if (method == null) { base.DataOnChange(); return; }
            method.Invoke(this.proxy, new object[] { this });
        }

        public override void SlotOnChange()
        {
            MethodInfo method = this.proxy.GetType().GetMethod("SlotOnChange");
            if (method == null) { base.SlotOnChange(); return; }
            method.Invoke(this.proxy, new object[] { this });
        }

        public override bool Configuable()
        {
            MethodInfo method = this.proxy.GetType().GetMethod("Configuable");
            if (method == null) { return base.Configuable(); }
            return (bool)method.Invoke(this.proxy, new object[] { this });
        }

        public override void Update(float dt)
        {
            MethodInfo method = this.proxy.GetType().GetMethod("Update");
            if (method == null) { base.Update(dt); return; }
            method.Invoke(this.proxy, new object[] { this, dt });
        }

        /// <summary>
        /// Create 'public string DefaultData()' method to override this
        /// </summary>
        public override string defaultData
        {
            get
            {
                MethodInfo method = this.proxy.GetType().GetMethod("DefaultData");
                if (method == null) { return base.defaultData; }
                return (string)method.Invoke(this.proxy, new object[] { this });
            }
        }

        public override void Initialize()
        {
            MethodInfo method = this.proxy.GetType().GetMethod("Initialize");
            if (method == null) { base.Initialize(); return; }
            method.Invoke(this.proxy, new object[] { this });
        }

        public override void LoadData()
        {
            MethodInfo method = this.proxy.GetType().GetMethod("LoadData");
            if (method == null) { base.LoadData(); return; }
            method.Invoke(this.proxy, new object[] { this });
        }

        public override bool SaveData()
        {
            MethodInfo method = this.proxy.GetType().GetMethod("SaveData");
            if (method == null) { return base.SaveData(); }
            return (bool)method.Invoke(this.proxy, new object[] { this });
        }

        public override void Signal(UItrigger trigger, string signal)
        {
            MethodInfo method = this.proxy.GetType().GetMethod("Signal");
            if (method == null) { base.Signal(trigger, signal); return; }
            method.Invoke(this.proxy, new object[] { this, trigger, signal });
        }



    }
}

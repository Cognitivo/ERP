﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cognitivo.Sales {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    public sealed partial class OrderSetting : global::System.Configuration.ApplicationSettingsBase {
        
        private static OrderSetting defaultInstance = ((OrderSetting)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new OrderSetting())));
        
        public static OrderSetting Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool VAT_Included {
            get {
                return ((bool)(this["VAT_Included"]));
            }
            set {
                this["VAT_Included"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool VAT_Excluded {
            get {
                return ((bool)(this["VAT_Excluded"]));
            }
            set {
                this["VAT_Excluded"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int TransDate_OffSet {
            get {
                return ((int)(this["TransDate_OffSet"]));
            }
            set {
                this["TransDate_OffSet"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool quicklook_AvailableCredit {
            get {
                return ((bool)(this["quicklook_AvailableCredit"]));
            }
            set {
                this["quicklook_AvailableCredit"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool quicklook_Telephone {
            get {
                return ((bool)(this["quicklook_Telephone"]));
            }
            set {
                this["quicklook_Telephone"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool quicklook_Project {
            get {
                return ((bool)(this["quicklook_Project"]));
            }
            set {
                this["quicklook_Project"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool quicklook_Branch {
            get {
                return ((bool)(this["quicklook_Branch"]));
            }
            set {
                this["quicklook_Branch"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool quicklook_SalesRep {
            get {
                return ((bool)(this["quicklook_SalesRep"]));
            }
            set {
                this["quicklook_SalesRep"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool quicklook_SalesBudget {
            get {
                return ((bool)(this["quicklook_SalesBudget"]));
            }
            set {
                this["quicklook_SalesBudget"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AllowDuplicateItems {
            get {
                return ((bool)(this["AllowDuplicateItems"]));
            }
            set {
                this["AllowDuplicateItems"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool filterbyBranch {
            get {
                return ((bool)(this["filterbyBranch"]));
            }
            set {
                this["filterbyBranch"] = value;
            }
        }
    }
}

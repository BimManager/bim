﻿#pragma checksum "..\..\..\..\Windows\RebarsCollectorWnd.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "EDABA47F59E2340B76B98B17C4A48D5CBD7BB918"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace TektaRevitPlugins {
    
    
    /// <summary>
    /// RebarsCollectorWnd
    /// </summary>
    public partial class RebarsCollectorWnd : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 56 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cb_partitions;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cb_host_marks;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cb_assembly_marks;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chb_is_scheduled;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chb_is_calculable;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chb_is_assembly;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_ok;
        
        #line default
        #line hidden
        
        
        #line 73 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_cancel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TektaRevitPlugins;component/windows/rebarscollectorwnd.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.cb_partitions = ((System.Windows.Controls.ComboBox)(target));
            
            #line 57 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
            this.cb_partitions.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cb_partitions_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.cb_host_marks = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 3:
            this.cb_assembly_marks = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.chb_is_scheduled = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 5:
            this.chb_is_calculable = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 6:
            this.chb_is_assembly = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 7:
            this.btn_ok = ((System.Windows.Controls.Button)(target));
            
            #line 72 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
            this.btn_ok.Click += new System.Windows.RoutedEventHandler(this.btn_ok_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.btn_cancel = ((System.Windows.Controls.Button)(target));
            
            #line 73 "..\..\..\..\Windows\RebarsCollectorWnd.xaml"
            this.btn_cancel.Click += new System.Windows.RoutedEventHandler(this.btn_cancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


﻿#pragma checksum "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "A2392A177CC56E1C0A378D94F8A996FD2DA4E58E"
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
using TektaRevitPlugins;


namespace TektaRevitPlugins {
    
    
    /// <summary>
    /// WndMultitableSchedule
    /// </summary>
    public partial class WndMultitableSchedule : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 57 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cb_schedules;
        
        #line default
        #line hidden
        
        
        #line 71 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cb_partitions;
        
        #line default
        #line hidden
        
        
        #line 74 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cb_host_marks;
        
        #line default
        #line hidden
        
        
        #line 77 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cb_assemblies;
        
        #line default
        #line hidden
        
        
        #line 79 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cb_block_num;
        
        #line default
        #line hidden
        
        
        #line 90 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cb_structure_type;
        
        #line default
        #line hidden
        
        
        #line 92 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chb_show_title;
        
        #line default
        #line hidden
        
        
        #line 95 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chb_concrete_quantity;
        
        #line default
        #line hidden
        
        
        #line 101 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_ok;
        
        #line default
        #line hidden
        
        
        #line 102 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
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
            System.Uri resourceLocater = new System.Uri("/TektaRevitPlugins;component/multitableschedule/wndmultitableschedule.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
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
            this.cb_schedules = ((System.Windows.Controls.ComboBox)(target));
            
            #line 57 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
            this.cb_schedules.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cb_schedules_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.cb_partitions = ((System.Windows.Controls.ComboBox)(target));
            
            #line 72 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
            this.cb_partitions.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cb_partitions_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.cb_host_marks = ((System.Windows.Controls.ComboBox)(target));
            
            #line 75 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
            this.cb_host_marks.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cb_host_marks_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.cb_assemblies = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 5:
            this.cb_block_num = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 6:
            this.cb_structure_type = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 7:
            this.chb_show_title = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 8:
            this.chb_concrete_quantity = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 9:
            this.btn_ok = ((System.Windows.Controls.Button)(target));
            
            #line 101 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
            this.btn_ok.Click += new System.Windows.RoutedEventHandler(this.btn_ok_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.btn_cancel = ((System.Windows.Controls.Button)(target));
            
            #line 102 "..\..\..\..\MultitableSchedule\WndMultitableSchedule.xaml"
            this.btn_cancel.Click += new System.Windows.RoutedEventHandler(this.btn_cancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

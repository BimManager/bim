﻿#pragma checksum "..\..\..\..\Windows\CpValuesWnd.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "8A89244DD2BDADF52EC9C861122176FB4F8A34FD"
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
using TektaRevitPlugins.Windows;


namespace TektaRevitPlugins.Windows {
    
    
    /// <summary>
    /// CpValuesWnd
    /// </summary>
    public partial class CpValuesWnd : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 41 "..\..\..\..\Windows\CpValuesWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lb_copy_from;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\..\Windows\CpValuesWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lb_copy_to;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\Windows\CpValuesWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txt_value;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\..\Windows\CpValuesWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_ok;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\..\Windows\CpValuesWnd.xaml"
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
            System.Uri resourceLocater = new System.Uri("/TektaRevitPlugins;component/windows/cpvalueswnd.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Windows\CpValuesWnd.xaml"
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
            this.lb_copy_from = ((System.Windows.Controls.ListBox)(target));
            
            #line 42 "..\..\..\..\Windows\CpValuesWnd.xaml"
            this.lb_copy_from.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.lb_copy_from_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.lb_copy_to = ((System.Windows.Controls.ListBox)(target));
            return;
            case 3:
            this.txt_value = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.btn_ok = ((System.Windows.Controls.Button)(target));
            
            #line 53 "..\..\..\..\Windows\CpValuesWnd.xaml"
            this.btn_ok.Click += new System.Windows.RoutedEventHandler(this.btn_ok_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btn_cancel = ((System.Windows.Controls.Button)(target));
            
            #line 54 "..\..\..\..\Windows\CpValuesWnd.xaml"
            this.btn_cancel.Click += new System.Windows.RoutedEventHandler(this.btn_cancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


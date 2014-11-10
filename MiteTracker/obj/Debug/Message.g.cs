﻿#pragma checksum "..\..\Message.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "40C5DCB2A4CE3293D5018E274F046CD7"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.18444
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
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


namespace Custom {
    
    
    /// <summary>
    /// Message
    /// </summary>
    public partial class Message : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 4 "..\..\Message.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Custom.Message Meldung;
        
        #line default
        #line hidden
        
        
        #line 8 "..\..\Message.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid WindowGrid;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\Message.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Rectangle WindowRectangle;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\Message.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label Title;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\Message.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Rectangle Icon;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\Message.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Viewbox ViewBoxText;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\Message.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock Text;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\Message.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label Bg;
        
        #line default
        #line hidden
        
        
        #line 70 "..\..\Message.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Ok;
        
        #line default
        #line hidden
        
        
        #line 71 "..\..\Message.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Cancel;
        
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
            System.Uri resourceLocater = new System.Uri("/MiteTracker;component/message.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Message.xaml"
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
            this.Meldung = ((Custom.Message)(target));
            
            #line 4 "..\..\Message.xaml"
            this.Meldung.KeyDown += new System.Windows.Input.KeyEventHandler(this.Meldung_KeyDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.WindowGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.WindowRectangle = ((System.Windows.Shapes.Rectangle)(target));
            
            #line 18 "..\..\Message.xaml"
            this.WindowRectangle.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.DragMe);
            
            #line default
            #line hidden
            return;
            case 4:
            this.Title = ((System.Windows.Controls.Label)(target));
            
            #line 19 "..\..\Message.xaml"
            this.Title.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.DragMe);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Icon = ((System.Windows.Shapes.Rectangle)(target));
            
            #line 40 "..\..\Message.xaml"
            this.Icon.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.DragMe);
            
            #line default
            #line hidden
            return;
            case 6:
            this.ViewBoxText = ((System.Windows.Controls.Viewbox)(target));
            return;
            case 7:
            this.Text = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 8:
            this.Bg = ((System.Windows.Controls.Label)(target));
            
            #line 50 "..\..\Message.xaml"
            this.Bg.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.DragMe);
            
            #line default
            #line hidden
            return;
            case 9:
            this.Ok = ((System.Windows.Controls.Button)(target));
            
            #line 70 "..\..\Message.xaml"
            this.Ok.Click += new System.Windows.RoutedEventHandler(this.Ok_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.Cancel = ((System.Windows.Controls.Button)(target));
            
            #line 71 "..\..\Message.xaml"
            this.Cancel.Click += new System.Windows.RoutedEventHandler(this.Cancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Navigation;
using System.Windows.Controls.Primitives;
using System.Reflection;
using System.Drawing;
using System.Configuration;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using System.Text;

namespace DockableDialog.Forms
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MainPage : Page, IDockablePaneProvider
    {
        public MainPage()
        {
            InitializeComponent();
        }
        //#region Data
        //private Guid m_targetGuid;
        //private DockPosition m_position = DockPosition.Bottom;
        //private int m_left = 1;
        //private int m_right = 1;
        //private int m_top = 1;
        //private int m_bottom = 1;
        //#endregion

        public void SetupDockablePane(Autodesk.Revit.UI.DockablePaneProviderData data)
        {
            data.FrameworkElement = this as FrameworkElement;
            data.InitialState = new Autodesk.Revit.UI.DockablePaneState();
            data.InitialState.DockPosition = DockPosition.Tabbed;
            data.InitialState.TabBehind = Autodesk.Revit.UI.DockablePanes.BuiltInDockablePanes.ProjectBrowser;

        }
        //public void SetInitialDockingParameters(int left, int right, int top, int bottom, DockPosition position, Guid targetGuid)
        //{
        //    m_position = position;
        //    m_left = left;
        //    m_right = right;
        //    m_top = top;
        //    m_bottom = bottom;
        //    m_targetGuid = targetGuid;
        //}
    }
}

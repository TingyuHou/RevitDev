using System;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using DockableDialog.Forms;
using DockableDialog.Properties;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DockableDialog
{
    /// <summary>
    /// 添加图标
    /// </summary>
    public class Ribbon : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            a.CreateRibbonTab("RevitDev");
            RibbonPanel AECPanelDebug = a.CreateRibbonPanel("RevitDev", "RevitDev");
            string path = Assembly.GetExecutingAssembly().Location;

            #region DockableWindow
            PushButtonData pushButtonRegisterDockableWindow = new PushButtonData("RegisterDockableWindow", "RegisterDockableWindow", path, "DockableDialog.RegisterDockableWindow");
            pushButtonRegisterDockableWindow.LargeImage = GetImage(Resources.green.GetHbitmap());
           
            pushButtonRegisterDockableWindow.AvailabilityClassName = "DockableDialog.AvailabilityNoOpenDocument";
            PushButtonData pushButtonShowDockableWindow = new PushButtonData("Show DockableWindow", "Show DockableWindow", path, "DockableDialog.ShowDockableWindow");
            pushButtonShowDockableWindow.LargeImage = GetImage(Resources.red.GetHbitmap());
            PushButtonData pushButtonHideDockableWindow = new PushButtonData("Hide DockableWindow", "Hide DockableWindow", path, "DockableDialog.HideDockableWindow");
            pushButtonHideDockableWindow.LargeImage = GetImage(Resources.orange.GetHbitmap());
            //IList<RibbonItem> ribbonpushButtonDockableWindow = AECPanelDebug.AddStackedItems(pushButtonRegisterDockableWindow, pushButtonShowDockableWindow, pushButtonHideDockableWindow);

            RibbonItem ri1 = AECPanelDebug.AddItem(pushButtonRegisterDockableWindow);
            RibbonItem ri2 = AECPanelDebug.AddItem(pushButtonShowDockableWindow);
            RibbonItem ri3 = AECPanelDebug.AddItem(pushButtonHideDockableWindow);
            #endregion

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        private System.Windows.Media.Imaging.BitmapSource GetImage(IntPtr bm)
        {
            System.Windows.Media.Imaging.BitmapSource bmSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bm,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            return bmSource;
        }
    }



    /// <summary>
    /// 限制只能够在没有打开项目的状态来注册窗体 You can only register a dockable dialog in "Zero doc state"
    /// </summary>
    /// 

    public class AvailabilityNoOpenDocument : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(
          UIApplication a,
          CategorySet b)
        {
            if (a.ActiveUIDocument == null)
            {
                return true;
            }
            return false;
        }
    }




    /// <summary>
    /// 注册窗体并添加处理点击事件Register your dockable dialog
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class RegisterDockableWindow : IExternalCommand
    {
        MainPage m_MyDockableWindow = null;

        ExternalCommandData m_externalCommandData;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            m_externalCommandData = commandData;
         
            MainPage MainDockableWindow = new MainPage();
            m_MyDockableWindow = MainDockableWindow;         

            DockablePaneId dpid = new DockablePaneId(new Guid("{D7C963CE-B7CA-426A-8D51-6E8254D21157}"));
            commandData.Application.RegisterDockablePane(dpid, "欢迎参加Revit开发培训！", MainDockableWindow as IDockablePaneProvider);

            commandData.Application.ViewActivated += new EventHandler<ViewActivatedEventArgs>(Application_ViewActivated);


            m_MyDockableWindow.btn_newWall.Click += Btn_newWall_Click;
            m_MyDockableWindow.btn_sel.Click += Btn_sel_Click;
            m_MyDockableWindow.btn_getWallCenter.Click += Btn_getWallCenter_Click;
            m_MyDockableWindow.btn_delWall.Click += Btn_delWall_Click;

            return Result.Succeeded;
        }

      
    
     

        /// <summary>
        /// 创建新的墙体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_newWall_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Document doc = m_externalCommandData.Application.ActiveUIDocument.Document;

            TaskDialog.Show("ceshi", "hello" + doc.Title + "Btn_newWall_Click");

        }

        /// <summary>
        /// 交互选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_sel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Document doc = m_externalCommandData.Application.ActiveUIDocument.Document;

            TaskDialog.Show("ceshi", "hello" + doc.Title + "Btn_sel_Click");
        }

        /// <summary>
        /// 获取墙的中点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_getWallCenter_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Document doc = m_externalCommandData.Application.ActiveUIDocument.Document;

            TaskDialog.Show("ceshi", "hello" + doc.Title + "Btn_getWallCenter_Click");
        }


        /// <summary>
        /// 删除墙
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_delWall_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Document doc = m_externalCommandData.Application.ActiveUIDocument.Document;

            TaskDialog.Show("ceshi", "hello" + doc.Title + "Btn_delWall_Click");
        }

        void Application_ViewActivated(object sender, ViewActivatedEventArgs e)
        {

            string msg = "\t欢迎参加橄榄山Revit开发培训！";

            m_MyDockableWindow.lblProjectName.Content = e.Document.Title+msg;
        }
    }




    /// <summary>
    /// 显示窗体Show dockable dialog
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    public class ShowDockableWindow : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            DockablePaneId dpid = new DockablePaneId(new Guid("{D7C963CE-B7CA-426A-8D51-6E8254D21157}"));
            DockablePane dp = commandData.Application.GetDockablePane(dpid);
            dp.Show();
            return Result.Succeeded;
        }
    }

    /// <summary>
    ///隐藏窗体 Hide dockable dialog
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    public class HideDockableWindow : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            DockablePaneId dpid = new DockablePaneId(new Guid("{D7C963CE-B7CA-426A-8D51-6E8254D21157}"));
            DockablePane dp = commandData.Application.GetDockablePane(dpid);
            dp.Hide();
            return Result.Succeeded;
        }
    }




}

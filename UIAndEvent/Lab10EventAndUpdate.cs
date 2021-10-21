#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Events;
#endregion

namespace GLSRevitDev
{

    /// <summary>
    /// Event 
    /// 
    /// cf. Developer Guide, Section 24 Event(pp278) - list of events you can subscribe 
    /// Appexdix G. API User Interface Guidelines (pp381), Task Dialog (pp404) 
    /// 
    /// External application to register/unregister document changed event. 
    /// Simply reports what has been changed  
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class Lab10_UIEventApp : IExternalApplication
    {
        // 时间和动态更新的注册
        public static bool m_showEvent = false;

        /// <summary>
        /// OnShutdown() - called when Revit ends. 
        /// </summary>
        public Result OnShutdown(UIControlledApplication app)
        {
            // (1) unregister our document changed event hander 
            // 关闭软件的时候取消事件的注册
            app.ControlledApplication.DocumentChanged -= GLSDevUILabs_DocumentChanged;

            return Result.Succeeded;
        }

        /// <summary>
        /// OnStartup() - called when Revit starts. 
        /// </summary>
        public Result OnStartup(UIControlledApplication app)
        {
            // (1) resgister our document changed event hander 
            //注册文件改变的时事件
            app.ControlledApplication.DocumentChanged += GLSDevUILabs_DocumentChanged;

            // (2) register our dynamic model updater (WindowDoorUpdater class definition below.) 
            //注册模型动态更新
            // We are going to keep doors and windows at the center of the wall. 
            // Construct our updater. 
            WindowDoorUpdater winDoorUpdater = new WindowDoorUpdater(app.ActiveAddInId);
            // ActiveAddInId is from addin menifest. 
            // Register it 
            //注册
            UpdaterRegistry.RegisterUpdater(winDoorUpdater);

            // Tell which elements we are interested in being notified about. 
            // We want to know when wall changes its length. 

            ElementClassFilter wallFilter = new ElementClassFilter(typeof(Wall));
            //添加触发器
            UpdaterRegistry.AddTrigger(winDoorUpdater.GetUpdaterId(), wallFilter, Element.GetChangeTypeGeometry());//构件的几何改变 材质等的不会触发

            return Result.Succeeded;
        }

        /// <summary>
        /// This is our event handler. Simply report the list of element ids which have been changed. 
        /// </summary>
        public void GLSDevUILabs_DocumentChanged(object sender, DocumentChangedEventArgs args)
        {
            if (!m_showEvent) return;

            // You can get the list of ids of element added/changed/modified. 
            Document rvtdDoc = args.GetDocument();


            ICollection<ElementId> idsAdded = args.GetAddedElementIds();
            ICollection<ElementId> idsDeleted = args.GetDeletedElementIds();
            ICollection<ElementId> idsModified = args.GetModifiedElementIds();

            // Put it in a string to show to the user. 
            string msg = "添加的构件Id: ";
            foreach (ElementId id in idsAdded)
            {
                msg += id.IntegerValue.ToString() + " ";
            }

            msg += "\n删除的构件Id: ";
            foreach (ElementId id in idsDeleted)
            {
                msg += id.IntegerValue.ToString() + " ";
            }

            msg += "\n更改的构件Id: ";
            foreach (ElementId id in idsModified)
            {
                msg += id.IntegerValue.ToString() + " ";
            }

            //带有确认和取消的对话框
            TaskDialogResult res = default(TaskDialogResult);
            res = TaskDialog.Show("是否继续显示此对话框", msg, TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel);

            // If the user chooses to cancel, show no more event. 
            if (res == TaskDialogResult.Cancel)
            {
                m_showEvent = false;
            }
        }
    }


    #region 事件



    /// <summary>
    /// External command to toggle event message on/off 
    /// </summary> 
    [Transaction(TransactionMode.Manual)]
    public class Lab10_UIEvent : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            Lab10_UIEventApp.m_showEvent = !Lab10_UIEventApp.m_showEvent;

            return Result.Succeeded;
        }

    }

    [Transaction(TransactionMode.Manual)]
    public class Lab10_UIEventOn : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            Lab10_UIEventApp.m_showEvent = true;

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class Lab10_UIEventOff : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            Lab10_UIEventApp.m_showEvent = false;

            return Result.Succeeded;
        }
    }


    #endregion 事件

    //======================================================== 
    // dynamic model update - derive from IUpdater class 
    //======================================================== 

    public class WindowDoorUpdater : IUpdater
    {
      
      //  /*
        // Unique id for this updater = addin GUID + GUID for this specific updater. 
        UpdaterId m_updaterId = null;

        // Flag to indicate if we want to perform 
        public static bool m_updateActive = false;

        /// <summary>
        /// Constructor 
        /// </summary>
        public WindowDoorUpdater(AddInId id)
        {
            m_updaterId = new UpdaterId(id, new Guid("EF43510F-38CB-4980-844C-72174A674D56"));
        }

        /// <summary>
        /// This is the main function to do the actual job. 
        /// For this exercise, we assume that we want to keep the door and window always at the center. 
        /// </summary>
        public void Execute(UpdaterData data)
        {
            if (!m_updateActive) return;

            Document rvtDoc = data.GetDocument();
            ICollection<ElementId> idsModified = data.GetModifiedElementIds();

            foreach (ElementId id in idsModified)
            {
                //  Wall aWall = rvtDoc.get_Element(id) as Wall; // For 2012
                Wall aWall = rvtDoc.GetElement(id) as Wall; // For 2013
                CenterWindowDoor(rvtDoc, aWall);
            }
        }

        #region 操作

        /// <summary>
        /// Helper function for Execute. 
        /// Checks if there is a door or a window on the given wall. 
        /// If it does, adjust the location to the center of the wall. 
        /// For simplicity, we assume there is only one door or window. 
        /// (TBD: or evenly if there are more than one.) 
        /// </summary>
        public void CenterWindowDoor(Document rvtDoc, Wall aWall)
        {
            // Find a winow or a door on the wall. 
            FamilyInstance e = FindWindowDoorOnWall(rvtDoc, aWall);
            if (e == null) return;

            // Move the element (door or window) to the center of the wall. 

            // Center of the wall

            LocationCurve wallLocationCurve = aWall.Location as LocationCurve;

            //XYZ pt1 = wallLocationCurve.Curve.get_EndPoint( 0 ); // 2013
            //XYZ pt2 = wallLocationCurve.Curve.get_EndPoint( 1 ); // 2013
            XYZ pt1 = wallLocationCurve.Curve.GetEndPoint(0); // 2014
            XYZ pt2 = wallLocationCurve.Curve.GetEndPoint(1); // 2014

            XYZ midPt = (pt1 + pt2) * 0.5;

            LocationPoint loc = e.Location as LocationPoint;
            loc.Point = new XYZ(midPt.X, midPt.Y, loc.Point.Z);
        }

        /// <summary>
        /// Helper function 
        /// Find a door or window on the given wall. 
        /// If it does, return it. 
        /// </summary>
        public FamilyInstance FindWindowDoorOnWall(Document rvtDoc, Wall aWall)
        {
            // Collect the list of windows and doors 
            // No object relation graph. So going hard way. 
            // List all the door instances 
            var windowDoorCollector = new FilteredElementCollector(rvtDoc);
            windowDoorCollector.OfClass(typeof(FamilyInstance));

            ElementCategoryFilter windowFilter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
            ElementCategoryFilter doorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            LogicalOrFilter windowDoorFilter = new LogicalOrFilter(windowFilter, doorFilter);

            windowDoorCollector.WherePasses(windowDoorFilter);
            IList<Element> windowDoorList = windowDoorCollector.ToElements();

            // This is really bad in a large model!
            // You might have ten thousand doors and windows.
            // It would make sense to add a bounding box containment or intersection filter as well.

            // Check to see if the door or window is on the wall we got. 
            foreach (FamilyInstance e in windowDoorList)
            {
                if (e.Host.Id.Equals(aWall.Id))
                {
                    return e;
                }
            }

            // If you come here, you did not find window or door on the given wall. 

            return null;
        }


        #endregion 操作

        /// <summary>
        /// This will be shown when the updater is not loaded. 
        /// </summary>
        public string GetAdditionalInformation()
        {
            return "Door/Window updater: keeps doors and windows at the center of walls.";
        }

        /// <summary>
        /// Specify the order of executing updaters. 
        /// </summary>
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.DoorsOpeningsWindows;
        }

        /// <summary>
        /// Return updater id. 
        /// </summary>
        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        /// <summary>
        /// User friendly name of the updater 
        /// </summary>
        public string GetUpdaterName()
        {
            return "Window/Door Updater";
        }
        

    }


    #region 控制事件和动态更新的ON OFF

    /// <summary>
    /// External command to toggle windowDoor updater on/off 
    /// </summary> 
    [Transaction(TransactionMode.Manual)]
    public class Lab10_UIDynamicModelUpdate : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            if (WindowDoorUpdater.m_updateActive)
            {
                WindowDoorUpdater.m_updateActive = false;
            }
            else
            {
                WindowDoorUpdater.m_updateActive = true;
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class Lab10_UIDynamicModelUpdateOn : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            WindowDoorUpdater.m_updateActive = true;

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class Lab10_UIDynamicModelUpdateOff : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            WindowDoorUpdater.m_updateActive = false;

            return Result.Succeeded;
        }
    }
    #endregion 控制事件和动态更新的ON OFF

}

#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace GLSRevitDev
{

    #region 访问参数

    [Transaction(TransactionMode.Manual)]
    public class Lab6Parameter_ShowElementParameters : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {

            string title = "GlsRevitDevRemind";

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //执行命令前先选择一些构件

            if (uidoc.Selection.GetElementIds().Count < 1)
            {
                TaskDialog.Show(title, "请先选择构件后再执行此命令");
                return Result.Cancelled;

            }

            foreach (ElementId eid in uidoc.Selection.GetElementIds())
            {
                Element ele = doc.GetElement(eid); // enable us to assign to e2 in case analyseTypeParameters == true
                ParamHelper.ShowParameters(ele, title);

            }

            return Result.Succeeded;

        }
    }
    #endregion 访问参数




    #region 设置墙的上部楼层


    [Transaction(TransactionMode.Manual)]
    public class Lab6Parameter_SetWallTopFloor : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {


            string title = "GlsRevitDevRemind";

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;


            //1 get a wall         
            Reference r = uidoc.Selection.PickObject(ObjectType.Element, "Pick a wall, please");
            Wall wall = doc.GetElement(r) as Wall;

            if (wall == null)
            {
                TaskDialog.Show(title, "选择的构件不是墙，请重新选择！");
                return Result.Cancelled;
            }

            FilteredElementCollector fec = new FilteredElementCollector(doc);
            fec.OfCategory(BuiltInCategory.OST_Levels).OfClass(typeof(Level));

            List<Level> listLevel = fec.ToElements().Cast<Level>().ToList();

            Level level = null;

            foreach (Level lev in listLevel)
            {
                if (lev.Name.Equals("标高 2"))
                {
                    level = lev;
                    break;
                }
            }

            Transaction ts = new Transaction(doc, "墙属性修改");
            ts.Start();


            Parameter para = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE); //顶部约束

            para.Set(level.Id);



            ts.Commit();

            //2 get all the Levels 
            return Result.Succeeded;
        }
    }





    #endregion 设置墙的上部楼层


    /// <summary>
    /// 给选择的构件的类别添加共享参数
    /// </summary>
    #region 共享参数
    [Transaction(TransactionMode.Manual)]
    public class Lab6Parameter_AddSharedParameterToWall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string title = "GlsRevitDevRemind";
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Selection sel = uidoc.Selection;
            //sel.PickObject(ObjectType.Element, "请选择要添加共享参数的类型的一个构件");
            Transaction ts = new Transaction(doc);
            ts.Start("添加共享参数");
            try
            {
                CategorySet catSet = new CategorySet();
                catSet.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls));//给墙添加

                string msg = "helloworld";
                ParamHelper.AddSharedParameter(catSet, doc, "RevitDevSharedParam", BuiltInParameterGroup.PG_TEXT, ParameterType.Text, "RevitDev", true, true, out msg);


                FilteredElementCollector fec = new FilteredElementCollector(doc);
                List<Element> listEle = fec.OfCategory(BuiltInCategory.OST_Walls).ToElements().ToList();



                foreach (var item in listEle)
                {
                    Wall wa = item as Wall;
                    string paraValue = Convert.ToString(DateTime.Today.DayOfYear);
                    if (wa != null)
                    {
#if REVIT2016 || EVIT2017
                        wa.LookupParameter("RevitDevSharedParam").Set(paraValue);

#else
                        wa.get_Parameter("RevitDevSharedParam").Set(paraValue);
#endif


                    }
                }


                ts.Commit();
            }
            catch (Exception ex)
            {
                TaskDialog.Show(title, ex.Message);
                ts.RollBack();
            }

            return Result.Succeeded;
        }
    }



    #endregion 共享参数
}

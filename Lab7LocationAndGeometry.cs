#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Text;
using System.Linq;
#endregion
namespace GLSRevitDev
{ /// <summary>
  /// 获取墙的中心线
  /// </summary>
    [TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    ///显示位置信息
    public class Lab7LocationAndGeometry_CommandShowEleLocation : IExternalCommand
    {
        string title = "GlsRevitDevRemind";
        public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
        {

            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;

            //select a wall      
            Reference ref1 = app.ActiveUIDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "请选择构件来查看位置信息");
            Element elem = doc.GetElement(ref1);

            ShowLocation(elem);

            return Result.Succeeded;
        }


        public void ShowLocation(Element e)
        {

            string s = "Location Information: " + "\n" + "\n";
            Location loc = e.Location;

            if (loc is LocationPoint)
            {
                // (1) we have a location point 

                LocationPoint locPoint = (LocationPoint)loc;
                XYZ pt = locPoint.Point;
                double r = locPoint.Rotation;

                s += "LocationPoint" + "\n";
                s += "Point = " + PointToString(pt) + "\n";
                s += "Rotation = " + r.ToString() + "\n";
            }
            else if (loc is LocationCurve)
            {
                // (2) we have a location curve 

                LocationCurve locCurve = (LocationCurve)loc;
                Curve crv = locCurve.Curve;

                s += "LocationCurve" + "\n";
                s += "EndPoint(0)/Start Point = " + PointToString(crv.GetEndPoint(0)) + "\n";
                s += "EndPoint(1)/End point = " + PointToString(crv.GetEndPoint(1)) + "\n";
                s += "Length = " + UnitUtils.ConvertFromInternalUnits(crv.Length, DisplayUnitType.DUT_MILLIMETERS).ToString() + "mm\n";

                // Location Curve also has property JoinType at the end 


            }
            TaskDialog.Show(title + "--Show Location", s);
        }

        public static string PointToString(XYZ p)
        {
            if (p == null)
            {
                return "";
            }

            return string.Format("({0},{1},{2})",
              p.X.ToString("F2"), p.Y.ToString("F2"),
              p.Z.ToString("F2"));
            //两位小数
        }

    }

    /// <summary>
    /// 显示几何信息
    /// </summary>

    [TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    public class Lab7LocationAndGeometry_CommandShowGeometry : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;
            string title = "GlsRevitDevRemind";
            //select a wall      
            Reference ref1 = app.ActiveUIDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "请选择构件来查看几何信息");
            Element elem = doc.GetElement(ref1);

            //' first, set a geometry option
            Options opt = new Options();
            opt.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement geomElem = elem.get_Geometry(opt);
            string msg = GeometryElementToString(geomElem);


            TaskDialog.Show(title, msg);

            return Result.Succeeded;
        }


        public static string GeometryElementToString(GeometryElement geomElem)
        {
            string str = string.Empty;
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    // ex. wall 
                    Solid solid = (Solid)geomObj;

                    //str += GeometrySolidToString(solid); 
                    StringBuilder sb = new StringBuilder();

                    if (solid.Faces.Size > 0)
                    {
                        sb.Append("构件的几何信息：\n");
                        sb.Append(string.Format("体积：{0},\t面积：{1},\t面数：{2},\t边数：{3}",
                            UnitUtils.ConvertFromInternalUnits(solid.Volume, DisplayUnitType.DUT_CUBIC_METERS),
                            UnitUtils.ConvertFromInternalUnits(solid.SurfaceArea, DisplayUnitType.DUT_SQUARE_METERS),
                            solid.Faces.Size, solid.Edges.Size));

                    }
                    str += sb.ToString();
                }
                else if (geomObj is GeometryInstance)
                {
                    // ex. door/window 

                    str += " -- Geometry.Instance -- " + "\n";
                    GeometryInstance geomInstance = (GeometryInstance)geomObj;
                    GeometryElement geoElem = geomInstance.SymbolGeometry;

                    str += GeometryElementToString(geoElem);
                }
                else if (geomObj is Curve)
                {
                    Curve curv = (Curve)geomObj;
                    //str += GeometryCurveToString(curv); 

                    str += "Curve" + "\n";
                }
                else if (geomObj is Mesh)
                {
                    Mesh mesh = (Mesh)geomObj;
                    //str += GeometryMeshToString(mesh); 

                    str += "Mesh" + "\n";
                }
                else
                {
                    str += " *** unkown geometry type" + geomObj.GetType().ToString();
                }
            }
            return str;
        }

        /// <summary>
        /// 射线法查找与梁相交的墙
        /// </summary>


    }

    ///获取墙中心点, 在其上创建一个门. (读取几何信息, 对象创建)

    [TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    public class Lab7LocationAndGeometry_CommandNewDoorAtCenterWall : IExternalCommand
    {
        string title = "GlsRevitDevRemind";
        public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
        {

            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;

            //select a wall      
            Reference ref1 = app.ActiveUIDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "请选择一面墙来放置门");
            Wall wall = doc.GetElement(ref1) as Wall;

            Curve loc = (wall.Location as LocationCurve).Curve;
            XYZ pt0 = loc.GetEndPoint(0);
            XYZ pt1 = loc.GetEndPoint(1);

            XYZ ptMiddle = (pt0 + pt1) / 2;

            //Document.NewFamilyInstance
            //public FamilyInstance NewFamilyInstance(
            //XYZ location,
            //FamilySymbol symbol,
            //Element host,
            //Level level,
            //StructuralType structuralType)

            //过滤门族出来
            FilteredElementCollector fec = new FilteredElementCollector(doc);
            fec.OfCategory(BuiltInCategory.OST_Doors).OfClass(typeof(FamilySymbol));

            FamilySymbol doorSymbol = fec.ToElements().ToList().First() as FamilySymbol;

            Level level = doc.GetElement(wall.LevelId) as Level;



            Transaction ts = new Transaction(doc);
            ts.Start("墙中点处创建门");
            doc.Create.NewFamilyInstance(ptMiddle, doorSymbol, wall, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            ts.Commit();

            return Result.Succeeded;
        }





    }


    ///Lab6. 获取墙开洞后的侧面积 (读取几何信息)


    [TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    public class Lab7LocationAndGeometry_CommandGetAreaOfWall : IExternalCommand
    {
        string title = "GlsRevitDevRemind";
        public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
        {

            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;

            //select a wall      
            Reference ref1 = app.ActiveUIDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "请选择一面墙来查看侧面面积");
            Wall wall = doc.GetElement(ref1) as Wall;

            double area = GetSideFace(wall);

            TaskDialog.Show(title, "侧面积为：" + UnitUtils.ConvertFromInternalUnits(area, DisplayUnitType.DUT_SQUARE_METERS).ToString());



            return Result.Succeeded;
        }

        private double GetSideFace(Wall wall)
        {
            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Medium;

            GeometryElement geoElem = wall.get_Geometry(opt);
            IEnumerator<GeometryObject> Objects = geoElem.GetEnumerator();
            Objects.Reset();
            while (Objects.MoveNext())
            {
                GeometryObject geo = Objects.Current;
                Solid solid = geo as Solid;
                if (solid != null && solid.Faces.Size > 0)
                {
                    foreach (Face face in solid.Faces)
                    {
                        //寻找开洞过的侧面
                        if (face.EdgeLoops.Size > 1)
                        {
                            return face.Area;
                        }
                    }
                }
            }
            return 0.0;
        }
    }




    /// <summary>
    /// 相交法查找梁下的支撑墙
    /// </summary>
    [TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Lab7LocationAndGeometry_FindSupporting : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
        {

            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;
            Transaction trans = new Transaction(doc, "ExComm");
            trans.Start();

            Selection sel = app.ActiveUIDocument.Selection;
            Reference ref1 = sel.PickObject(ObjectType.Element, "Please pick a beam");
            FamilyInstance beam = doc.GetElement(ref1) as FamilyInstance;

            //Read the beam's location line
            LocationCurve lc = beam.Location as LocationCurve;
            Curve curve = lc.Curve;

            XYZ ptStart = curve.GetEndPoint(0);
            XYZ ptEnd = curve.GetEndPoint(1);

            //move the two point a little bit lower, so the ray can go through the wall
            XYZ offset = new XYZ(0, 0, 0.01);
            ptStart = ptStart - offset;
            ptEnd = ptEnd - offset;

            View3D view3d = null;
            view3d = doc.ActiveView as View3D;
            if (view3d == null)
            {
                TaskDialog.Show("3D view", "current view should be 3D view");
                return Result.Failed;
            }

            double beamLen = curve.Length;

            ElementClassFilter filter = new ElementClassFilter(typeof(Wall));

            ReferenceIntersector refIntersector = new ReferenceIntersector(filter, FindReferenceTarget.All, view3d);// The target type of references to return.
            IList<ReferenceWithContext> listReferenceWithContext = refIntersector.Find(ptStart, (ptEnd - ptStart));//起点 方向
            List<ElementId> listEID = new List<ElementId>();
            foreach (ReferenceWithContext item in listReferenceWithContext)
            {
                Reference reference = item.GetReference();
                listEID.Add(reference.ElementId);
            }

//#if REVIT2014
             foreach (var item in listEID)
            {
                sel.Elements.Add(doc.GetElement(item));
            }
//#else
//            sel.SetElementIds(listEID);
//#endif
           
           


          


            trans.Commit();

            return Result.Succeeded;
        }
    }


    /// <summary>
    /// 获取墙的地面面积
    /// </summary>
    [TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    public class Lab7LocationAndGeometry_GetWallBFace : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
        {

            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;

            //select a wall      
            Reference ref1 = app.ActiveUIDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Please pick a wall");
            Element elem = doc.GetElement(ref1);
            Wall wall = elem as Wall;

            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Medium;

            GeometryElement geoElem = wall.get_Geometry(opt);
            IEnumerator<GeometryObject> Objects = geoElem.GetEnumerator();
            while (Objects.MoveNext())
            {
                GeometryObject geo = Objects.Current;
                Solid solid = geo as Solid;
                if (solid != null && solid.Faces.Size > 0)
                    FindBottomFace(solid);
            }

            return Result.Succeeded;
        }
        Face FindBottomFace(Solid solid)
        {
            PlanarFace pf = null;
            foreach (Face face in solid.Faces)
            {
                pf = face as PlanarFace;
                if (null != pf)
                {
                    if (Math.Abs(pf.Normal.X) < 0.01 && Math.Abs(pf.Normal.Y) < 0.01 && pf.Normal.Z < 0)
                    {
                        TaskDialog.Show("Wall Bottom Face", "Area is " + pf.Area.ToString() + "; Origin = (" + pf.Origin.X.ToString() + "  " + pf.Origin.Y.ToString() + "  " + pf.Origin.Z.ToString() + ")");

                        break;
                    }
                }
            }
            return pf;
        }

    }


    /// <summary>
    /// 柱子的底面面积
    /// </summary>

    [TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Lab7LocationAndGeometry_GetColumnBottomFace : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
        {

            UIApplication app = commandData.Application;
            Document doc = app.ActiveUIDocument.Document;

            //select a column      
            Reference ref1 = app.ActiveUIDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Please pick a column");
            Element elem = doc.GetElement(ref1);
            FamilyInstance column = elem as FamilyInstance;

            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Medium;
            GeometryElement geoElem = column.get_Geometry(opt);
            IEnumerator<GeometryObject> Objects = geoElem.GetEnumerator();



            while (Objects.MoveNext())
            {
                GeometryObject obj = Objects.Current;

                if (obj is Solid)
                {
                    Solid solid = obj as Solid;
                    FindBottomFace(solid);
                }
                else if (obj is GeometryInstance)
                {
                    GeometryInstance geoInstance = obj as GeometryInstance;
                    GeometryElement geoElement = geoInstance.GetInstanceGeometry();

                    IEnumerator<GeometryObject> ObjectsIN = geoElement.GetEnumerator();



                    while (ObjectsIN.MoveNext())
                    {
                        GeometryObject obj2 = ObjectsIN.Current;

                        if (obj2 is Solid)
                        {
                            Solid solid2 = obj2 as Solid;
                            if (solid2.Faces.Size > 0)
                                FindBottomFace(solid2);
                        }
                    }
                }
            }
            return Result.Succeeded;
        }
        Face FindBottomFace(Solid solid)
        {
            PlanarFace pf = null;
            foreach (Face face in solid.Faces)
            {
                pf = face as PlanarFace;
                if (null != pf)
                {
                    if (Math.Abs(pf.Normal.X) < 0.01 && Math.Abs(pf.Normal.Y) < 0.01 && pf.Normal.Z < 0)
                    {
                        TaskDialog.Show("column Bottom Face", "Area is " + pf.Area.ToString() + "; Origin = (" + pf.Origin.X.ToString() + "  " + pf.Origin.Y.ToString() + "  " + pf.Origin.Z.ToString() + ")");

                        break;
                    }
                }
            }
            return pf;
        }

    }
}

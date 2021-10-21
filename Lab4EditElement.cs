using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;


namespace GLSRevitDev
{
    class Lab4EditElement
    { /// <summary>
      /// 修改类型
      /// </summary>
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class EditElement1 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {

                Document doc = commandData.Application.ActiveUIDocument.Document;
                //选择一个构件
                Selection sel = commandData.Application.ActiveUIDocument.Selection;
                Reference selRef = sel.PickObject(ObjectType.Element);
                //得到一个元素
                Element ele = doc.GetElement(selRef.ElementId);

                Transaction trans = new Transaction(doc);

                trans.Start("修改元素");

                //对类型进行修改
                //如果是墙
                if (ele is Wall)
                {
                    //修改墙类型
                    ChangeWallType(ele, doc);
                }
                //如果是天花板
                if (ele is Ceiling)
                {
                    //修改天花板
                    ChangeCeilingType(ele, doc);
                }
                //如果是门
                if (ele is FamilyInstance && ele.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                {
                    //修改门类型
                    ChangeDoorFamilySymbol(ele, doc);
                }


                trans.Commit();
                return Result.Succeeded;
            }

            /// <summary>
            /// 修改墙类型
            /// </summary>
            /// <param name="ele"></param>
            /// <param name="doc"></param>
            public void ChangeWallType(Element ele, Document doc)
            {
                //得到墙的类型
                FilteredElementCollector walltypeCollcetor = new FilteredElementCollector(doc);
                walltypeCollcetor.OfClass(typeof(WallType));

                WallType newWalltye = doc.GetElement(walltypeCollcetor.ElementAt(0).Id) as WallType;

                //得到墙
                Wall wall = doc.GetElement(ele.Id) as Wall;
                //转换墙类型
                wall.WallType = newWalltye;
            }


            /// <summary>
            /// 修改天花板的类型
            /// </summary>
            /// <param name="ele"></param>
            /// <param name="doc"></param>
            public void ChangeCeilingType(Element ele, Document doc)
            {

                //得到天花板的类型
                FilteredElementCollector ceilingCollcetor = new FilteredElementCollector(doc);
                ceilingCollcetor.OfClass(typeof(CeilingType));

                CeilingType NewCeilType = doc.GetElement(ceilingCollcetor.ElementAt(0).Id) as CeilingType;

                //修改天花板的类型
                ele.ChangeTypeId(NewCeilType.Id);

            }

            /// <summary>
            /// 修改门类型
            /// </summary>
            /// <param name="ele"></param>
            /// <param name="doc"></param>
            public void ChangeDoorFamilySymbol(Element ele, Document doc)
            {

                //得到门类型
                FilteredElementCollector doorSymbolCollector = new FilteredElementCollector(doc);
                doorSymbolCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Doors);

                FamilySymbol Newdoorsymbol = doc.GetElement(doorSymbolCollector.ElementAt(0).Id) as FamilySymbol;

                FamilyInstance doorInstance = doc.GetElement(ele.Id) as FamilyInstance;

                //修改门类型
                doorInstance.Symbol = Newdoorsymbol;

            }

        }


        /// <summary>
        /// 修改参数
        /// </summary>
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class EditElement2 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {

                Document doc = commandData.Application.ActiveUIDocument.Document;
                //选择一个构件
                Selection sel = commandData.Application.ActiveUIDocument.Selection;
                Reference selRef = sel.PickObject(ObjectType.Element);
                //得到一个元素
                Element ele = doc.GetElement(selRef.ElementId);

                Transaction trans = new Transaction(doc);

                trans.Start("修改元素");

                //如果是墙
                if (ele is Wall)
                {
                    //修改墙参数
                    EditParameter(ele, doc);
                }

                trans.Commit();
                return Result.Succeeded;


            }
            /// <summary>
            /// 编辑参数
            /// </summary>
            /// <param name="ele"></param>
            /// <param name="doc"></param>
            public void EditParameter(Element ele, Document doc)
            {
                Wall aWall = doc.GetElement(ele.Id) as Wall;
                //修改墙的顶部偏移//修改这个参数时，必须有顶部标高，没有顶部标高时，这个参数为只读
                aWall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).Set(14.0);
                //修改墙的注释
                aWall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set("Modified by API");
            }


        }


        /// <summary>
        /// 修改location
        /// </summary>
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class EditElement3 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {

                Document doc = commandData.Application.ActiveUIDocument.Document;
                //选择一个构件
                Selection sel = commandData.Application.ActiveUIDocument.Selection;
                Reference selRef = sel.PickObject(ObjectType.Element);
                //得到一个元素
                Element ele = doc.GetElement(selRef.ElementId);

                Transaction trans = new Transaction(doc);

                trans.Start("修改元素");

                //如果是墙
                if (ele is Wall)
                {
                    //修改墙的位置线
                    ChangeLocationCurve(ele, doc);
                }
                //如果是柱子
                if (ele is FamilyInstance &&
                    (ele.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns || ele.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Columns))
                {
                    //修改柱子位置点
                    ChangeLocationPoint(ele, doc);

                }

                trans.Commit();
                return Result.Succeeded;


            }
            /// <summary>
            /// 修改墙线
            /// </summary>
            /// <param name="ele"></param>
            /// <param name="doc"></param>
            public void ChangeLocationCurve(Element ele, Document doc)
            {
                //得到ele的位置线
                LocationCurve locationcurve = ele.Location as LocationCurve;

                //创建一条新的线
                XYZ st = new XYZ(10, 10, 0);
                XYZ ed = new XYZ(100, 100, 0);

                Line line = Line.CreateBound(st, ed);

                //修改墙的线
                locationcurve.Curve = line;
            }

            /// <summary>
            /// 修改柱子位置点
            /// </summary>
            /// <param name="ele"></param>
            /// <param name="doc"></param>
            public void ChangeLocationPoint(Element ele, Document doc)
            {

                //得到ele的位置点
                LocationPoint locationPoint = ele.Location as LocationPoint;
                //创建新的点//如果这个点不在门所在的墙的墙线上，就不能修改门位置
                XYZ NewPoint = new XYZ(50, 50, 0);
                //修改到新的点
                locationPoint.Point = NewPoint;

            }

        }


        /// <summary>
        /// 移动旋转镜像复制
        /// </summary>
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class EditElement4 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {

                Document doc = commandData.Application.ActiveUIDocument.Document;
                //选择一个构件
                Selection sel = commandData.Application.ActiveUIDocument.Selection;
                Reference selRef = sel.PickObject(ObjectType.Element);
                //得到一个元素
                Element ele = doc.GetElement(selRef.ElementId);

                Transaction trans = new Transaction(doc);

                trans.Start("修改元素");

                //对类型进行修改
                //如果是墙
                if (ele is Wall)
                {
                    //移动墙
                    moveElement(ele, doc);

                    //旋转墙
                    RotateElement(ele, doc);

                    //镜像墙
                    MirrorElement(ele, doc);

                    //复制墙
                    CopyElement(ele, doc);

                }

                trans.Commit();
                return Result.Succeeded;


            }


            /// <summary>
            /// 移动ele
            /// </summary>
            /// <param name="ele"></param>
            /// <param name="doc"></param>
            public void moveElement(Element ele, Document doc)
            {
                //移动的坐标
                XYZ moveXyz = new XYZ(30, 30, 0);
                //移动//原坐标加上新坐标
                ElementTransformUtils.MoveElement(doc, ele.Id, moveXyz);

            }


            /// <summary>
            /// 旋转元素
            /// </summary>
            /// <param name="ele"></param>
            /// <param name="doc"></param>
            public void RotateElement(Element ele, Document doc)
            {
                //旋转轴，一般是与locationcurve所在的平面垂直
                LocationCurve locationcurve = ele.Location as LocationCurve;
                XYZ st = locationcurve.Curve.GetEndPoint(0);
                XYZ ed = new XYZ(st.X, st.Y, 30);
                Line axisline = Line.CreateBound(st, ed);
                //绕着垂直于起点的竖向轴，旋转90度
                double angel = Math.PI / 2;
                ElementTransformUtils.RotateElement(doc, ele.Id, axisline, angel);

            }


            /// <summary>
            /// 镜像元素
            /// </summary>
            /// <param name="ele"></param>
            /// <param name="doc"></param>
            public void MirrorElement(Element ele, Document doc)
            {
                //创建一个镜像的平面，这个平面一般和Z轴垂直
                Plane plane = new Plane(XYZ.BasisX, XYZ.Zero);
                ElementTransformUtils.MirrorElement(doc, ele.Id, plane);

            }

            /// <summary>
            /// 复制元素
            /// </summary>
            /// <param name="ele"></param>
            /// <param name="doc"></param>
            public void CopyElement(Element ele, Document doc)
            {
                //移动的方向向量//原坐标加上方向向量
                XYZ transcation = new XYZ(100, 100, 0);
                //复制元素
                ElementTransformUtils.CopyElement(doc, ele.Id, transcation);

            }


        }
    }
}

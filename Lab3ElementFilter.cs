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
    class Lab3ElementFilter
    {

        /// <summary>
        /// 收集墙类型得到个数
        /// </summary>
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class elementFilter1 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {

                Document doc = commandData.Application.ActiveUIDocument.Document;

                //收集墙类型，得到墙类型的个数
                FilteredElementCollector WallTypeCol = new FilteredElementCollector(doc);
                WallTypeCol.OfClass(typeof(WallType));
                int wallTypeNumber = WallTypeCol.Count();
                TaskDialog.Show("Acommand", wallTypeNumber.ToString());

                return Result.Succeeded;
            }

        }




        /// <summary>
        /// 收集墙实例,得到墙实例个数
        /// </summary>
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class elementFilter2 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {

                Document doc = commandData.Application.ActiveUIDocument.Document;

                //收集墙实例,得到墙实例个数
                FilteredElementCollector WallCollector = new FilteredElementCollector(doc);
                WallCollector.OfClass(typeof(Wall));
                IList<Element> WallList = WallCollector.ToElements();
                TaskDialog.Show("Acommand", WallList.Count().ToString());

                return Result.Succeeded;
            }

        }




        /// <summary>
        /// 过滤得到指定墙类型
        /// </summary>
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class elementFilter3 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {

                Document doc = commandData.Application.ActiveUIDocument.Document;

                //调用方法，得到指定墙类型的Element
                Element walltypeEle = FindFamilyType_Wall_v1("常规 - 200mm", doc);
                TaskDialog.Show("Acommand", walltypeEle.Id.IntegerValue.ToString());

                return Result.Succeeded;
            }

            /// <summary>
            /// 得到指定的墙类型
            /// </summary>
            /// <param name="wallTypeName"></param>
            /// <param name="_doc"></param>
            /// <returns></returns>
            public Element FindFamilyType_Wall_v1(string wallTypeName, Document _doc)
            {
                //过滤出所有的墙类型
                FilteredElementCollector wallTypeCollector1 = new FilteredElementCollector(_doc);
                wallTypeCollector1.OfClass(typeof(WallType));

                // LINQ查询到符合名字的墙类型
                var wallTypeElems1 = from element in wallTypeCollector1
                                     where element.Name.Equals(wallTypeName)
                                     select element;

                // 得到指定的墙类型
                Element wallType1 = null;

                if (wallTypeElems1.Count() > 0)
                {
                    wallType1 = wallTypeElems1.First<Element>();
                }
                return wallType1;
            }

        }




        /// <summary>
        /// 过滤得到指定门类型
        /// </summary>
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class elementFilter4 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {

                Document doc = commandData.Application.ActiveUIDocument.Document;

                //调用方法，得到指定的门类型的Element
                Element doorTypeEle = FindFamilyType_Door_v1("0915 x 2134 mm", doc);
                TaskDialog.Show("Acommand", doorTypeEle.Id.IntegerValue.ToString());

                return Result.Succeeded;
            }

            /// <summary>
            /// 得到指定的门类型
            /// </summary>
            /// <param name="doorTypeName"></param>
            /// <param name="_doc"></param>
            /// <returns></returns>
            public Element FindFamilyType_Door_v1(string doorTypeName, Document _doc)
            {
                // 过滤出可载入族的类型
                FilteredElementCollector doorFamilyCollector1 = new FilteredElementCollector(_doc);
                doorFamilyCollector1.OfClass(typeof(FamilySymbol));
                //过滤出类别是门的族类型
                doorFamilyCollector1.OfCategory(BuiltInCategory.OST_Doors);

                //linq查找指定的族类型
                var doorTypeElems =
                    from element in doorFamilyCollector1
                    where element.Name.Equals(doorTypeName)
                    select element;

                //得到指定门类型
                Element doorType1 = null;

                if (doorTypeElems.Count() > 0)
                {
                    doorType1 = doorTypeElems.ElementAt(0);

                }
                return doorType1;
            }

        }




        /// <summary>
        /// 快速过滤得到门实例
        /// </summary>
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class elementFilter5 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {

                Document doc = commandData.Application.ActiveUIDocument.Document;
                //调用方法，快速过滤得到门实例
                Element doorInstance1 = FindFamilyInstance_Door_v0(doc);
                TaskDialog.Show("Acommand", doorInstance1.Id.IntegerValue.ToString());

                return Result.Succeeded;
            }


            /// <summary>
            /// 快速过滤出门实例
            /// </summary>
            /// <param name="_doc"></param>
            /// <returns></returns>
            public Element FindFamilyInstance_Door_v0(Document _doc)
            {

                //快速过滤门实例
                FilteredElementCollector doorInstanceCollector1 = new FilteredElementCollector(_doc);
                doorInstanceCollector1.OfClass(typeof(FamilyInstance));
                doorInstanceCollector1.OfCategory(BuiltInCategory.OST_Doors);


                // 得到一个门实例
                Element doorInstance = null;

                if (doorInstanceCollector1.Count() > 0)
                {
                    doorInstance = doorInstanceCollector1.ElementAt(0);

                }
                return doorInstance;

            }

        }



        /// <summary>
        /// 慢速过滤得到门实例
        /// </summary>
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class elementFilter6 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {

                Document doc = commandData.Application.ActiveUIDocument.Document;

                //调用方法，慢速过滤得到门实例
                Element doorInstance = FindFamilyInstance_Door_v1(doc);
                TaskDialog.Show("Acommand", doorInstance.Id.IntegerValue.ToString());

                return Result.Succeeded;
            }

            /// <summary>
            /// 慢速过滤出门实例
            /// </summary>
            /// <param name="_doc"></param>
            /// <returns></returns>
            public Element FindFamilyInstance_Door_v1(Document _doc)
            {
                //得到一个门类型
                FilteredElementCollector doorSymbolCollector1 = new FilteredElementCollector(_doc);
                doorSymbolCollector1.OfClass(typeof(FamilySymbol));
                doorSymbolCollector1.OfCategory(BuiltInCategory.OST_Doors);
                FamilySymbol doorSymbol = _doc.GetElement(doorSymbolCollector1.ElementAt(0).Id) as FamilySymbol;

                //慢速过滤门实例
                FilteredElementCollector doorInstanceCollector1 = new FilteredElementCollector(_doc);
                FamilyInstanceFilter instanceFilter = new FamilyInstanceFilter(_doc, doorSymbol.Id);
                doorInstanceCollector1.WherePasses(instanceFilter);

                // 得到一个门实例
                Element doorInstance = null;

                if (doorInstanceCollector1.Count() > 0)
                {
                    doorInstance = doorInstanceCollector1.ElementAt(0);

                }
                return doorInstance;
            }


        }




        /// <summary>
        /// 逻辑过滤器
        /// </summary>
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class elementFilter7 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {

                Document doc = commandData.Application.ActiveUIDocument.Document;

                //逻辑过滤器
                AndLogicalFilter(doc);

                return Result.Succeeded;
            }

            /// <summary>
            /// 逻辑过滤器
            /// </summary>
            /// <param name="_doc"></param>
            public void AndLogicalFilter(Document _doc)
            {

                FilteredElementCollector ElementCollector = new FilteredElementCollector(_doc);
                // 创建一个类过滤器来过滤出所有的 FamilyInstance 类的元素。
                ElementClassFilter familyInstanceFilter = new
                ElementClassFilter(typeof(FamilyInstance));
                // 创建一个类别过滤器来过滤出所有的内建类型为 OST_Doors 的元素。
                ElementCategoryFilter doorsCategoryfilter =
                new ElementCategoryFilter(BuiltInCategory.OST_Doors);
                // 创建一个逻辑过滤器来组合前面两个过滤器，实现过滤出所有 Door实例。
                LogicalAndFilter doorInstancesFilter = new LogicalAndFilter(familyInstanceFilter, doorsCategoryfilter);

                ICollection<ElementId> doorsList = ElementCollector.WherePasses(doorInstancesFilter).ToElementIds();

                TaskDialog.Show("Acommand", doorsList.Count().ToString());


            }

        }

    }
}

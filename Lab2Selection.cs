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
    class Lab2Selection
    {
        [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
        public class ClassSelection : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
            {
                //选择一个元素
                Document doc = commandData.Application.ActiveUIDocument.Document;
                Selection sel = commandData.Application.ActiveUIDocument.Selection;
                Reference selRef = sel.PickObject(ObjectType.Element);
                //
                Element ele = doc.GetElement(selRef.ElementId);

                if (ele is Wall)
                {
                    //得到墙
                    Wall wall = doc.GetElement(ele.Id) as Wall;

                    //得到墙类型
                    WallType wallType = wall.WallType;

                    //得到类别
                    Category wallCate = wall.Category;

                    TaskDialog.Show("Name", wall.Name + "\n" + wallType.Name + "\n" + wallCate.Name);


                }

                if (ele is FamilyInstance)
                {
                    //得到族实例
                    FamilyInstance familyinstance = doc.GetElement(ele.Id) as FamilyInstance;

                    //得到类型
                    FamilySymbol familySymbol = familyinstance.Symbol;

                    //得到族
                    Family family = familySymbol.Family;

                    TaskDialog.Show("Name", familyinstance.Name + "\n" + familySymbol.Name + "\n" + family.Name);
                }

                return Result.Succeeded;
            }
        }

    }
}

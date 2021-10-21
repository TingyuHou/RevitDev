using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage; // needed for Extensible Storage 
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace GLSRevitDev
{
  [Transaction(TransactionMode.Manual)]
  class Lab14_Dimension : IExternalCommand
  {
    /// <summary>
    /// 执行函数
    /// </summary>
    /// <param name="commandData">传入参数</param>
    /// <param name="message">传出错误消息</param>
    /// <param name="elements">传出错误相关对象</param>
    /// <returns></returns>
    public Result Execute(ExternalCommandData commandData,
      ref string message, ElementSet elements)
    {
      UIDocument uiDoc = commandData.Application.ActiveUIDocument;
      Document doc = uiDoc.Document;

      Selection sel = uiDoc.Selection;
      Reference ref1 = sel.PickObject(ObjectType.Element,
        "please pick a wall for creating dimension for thikness");
      Wall w = doc.GetElement(ref1) as Wall;
      Line line = (w.Location as LocationCurve).Curve as Line;

      ReferenceArray references = new ReferenceArray();
      Reference ref2 = sel.PickObject(ObjectType.Face,
        "please pick one side of the wall");
      Reference ref3 = sel.PickObject(ObjectType.Face,
        "please pick the other side of the wall");
      references.Append(ref2);
      references.Append(ref3);

      Transaction trans = new Transaction(doc);
      trans.Start("createDimension");
      // create the new dimension
      Dimension dimension = doc.Create.NewDimension(doc.ActiveView,
        line, references);
      trans.Commit();

      add(1, 2);

      return Result.Succeeded;
    }

    /// <summary>
    /// 求相交
    /// </summary>
    /// <param name="a">加数</param>
    /// <param name="b">被加数</param>
    /// <returns></returns>
    public int add(int a, int b)
    {
      return a + b;
    }

    //XYZ pt1 = line.GetEndPoint(0);
    //XYZ pt2 = line.GetEndPoint(1);

    //XYZ v = (pt2 - pt1).Normalize();
    //XYZ vOffset = v.CrossProduct(new XYZ(0,0,1));

    //XYZ pt1Offset = pt1 + 0.5 * vOffset;
    //XYZ pt2Offset = pt2 + 0.5 * vOffset;

    //Line dimLine = Line.CreateBound(pt1Offset, pt2Offset);


  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

namespace GLSRevitDev
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Lab8RibbonTest : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication app)
        {

            app.CreateRibbonTab("Revit2016Dev_UI_TAB");//new tab

            RibbonPanel rp = app.CreateRibbonPanel("Revit2016Dev_UI_TAB", "Revit2016Dev_UI_Ribbon");// tab name  panel Name
            string assemblyPath = @" D:\@@@work\20170430\Revit2016DevZFC\Revit2016Dev_LocationAndGeometry\bin\Debug\2016\Revit2016Dev_LocationAndGeometry.dll";
            string className = "Revit2016Dev_LocationAndGeometry.CommandShowEleLocation";

            //内部名字 必须保证唯一； 显示在button上的文字；dll的路径；全名称，包括namespace的名称
            PushButtonData pbd = new PushButtonData("ShowLocation", "显示位置信息", assemblyPath, className);

            PushButton pushButton = rp.AddItem(pbd) as PushButton;
            string imgPath = @"D:\@@@work\gls_Apparent.ico";
            pushButton.LargeImage = new BitmapImage(new Uri( imgPath));

            pushButton.ToolTip = "显示构件的位置信息";

            return Result.Succeeded;
        }
    }
}

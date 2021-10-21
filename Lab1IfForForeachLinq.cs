using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;


namespace GLSRevitDev
{

    /// <summary>
    /// 一个简单的类
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Lab1IfForForeachLink1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            ASimpolCalss asample = new ASimpolCalss();  //创建一个simpolclass类的实例
            int number = asample.aNumber;//实例的字段可取值
            asample.aNumber = 2;//也可以赋值
            string thisResult = asample.aMethod("GLS");  //调用类里的方法

            TaskDialog.Show("ASimpolClass", "The number : " + number.ToString() + "\n" + "The Result : " + thisResult);//revit显示输出

            return Result.Succeeded;

        }

    }




    /// <summary>
    /// if else
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Lab1IfForForeachLink2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //If　Else 输出显示
            IfNumber(10);

            return Result.Succeeded;

        }
        /// <summary>
        /// if else 语句
        /// </summary>
        /// <param name="number"></param>
        public void IfNumber(int number)
        {

            //判断三次
            if (number > 0 && number < 1)
            {
                number = 0;
            }
            else if (number >= 1 && number < 2)
            {
                number = 1;
            }
            else if (number >= 2)
            {
                number = 2;
            }
            TaskDialog.Show("If Else", number.ToString());

        }

    }




    /// <summary>
    /// for
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Lab1IfForForeachLink3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            //for 输出显示
            ForNumber(3);

            return Result.Succeeded;

        }
        /// <summary>
        /// for 语句
        /// </summary>
        /// <param name="number"></param>
        public void ForNumber(int number)
        {
            int Addresult = 0;
            if (number > 0)
            {
                for (int i = 0; i < number; i++)
                {
                    Addresult = i + Addresult;
                }
            }
            TaskDialog.Show("For", Addresult.ToString());

        }
    }




    /// <summary>
    /// foreach
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Lab1IfForForeachLink4 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            //foreach 输出显示
            ForeachNumber();

            return Result.Succeeded;
        }
        /// <summary>
        /// foreach 语句
        /// </summary>
        /// <param name="NumberList"></param>
        public void ForeachNumber()
        {
            int AddResult = 0;
            //创建一个list
            List<int> NumberList = new List<int>();
            NumberList.Add(1);
            NumberList.Add(2);
            NumberList.Add(3);

            foreach (int number in NumberList)
            {
                AddResult = number + AddResult;
            }
            TaskDialog.Show("For", AddResult.ToString());
        }

    }




    /// <summary>
    /// break continue
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Lab1IfForForeachLink5 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Continue Break 输出显示
            BreakContinueNumber();

            return Result.Succeeded;
        }

        /// <summary>
        /// break continue 语句
        /// </summary>
        public void BreakContinueNumber()
        {
            string result = null;
            for (int i = 0; i < 5; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                if (i == 4)
                {
                    break;
                }
                result = result + "\n" + i.ToString();
            }
            TaskDialog.Show("BreakContinue", result);
        }


    }




    /// <summary>
    /// link查询
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Lab1IfForForeachLink6 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //link1  输出显示
            Link1();

            return Result.Succeeded;
        }
        /// <summary>
        /// link查询
        /// </summary>
        /// <returns></returns>
        public void Link1()
        {
            string result = null;
            int[] array1 = { 4, 3, 1, 2, 5 };
            var query1 = from number in array1
                         let aNumber = number + 1
                         where aNumber > 2
                         orderby aNumber
                         select aNumber;
            foreach (int number in query1)
            {
                result = result + number.ToString();
            }

            TaskDialog.Show("LINQ", result);
        }

    }




    /// <summary>
    /// link分组
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Lab1IfForForeachLink7 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            //link2  输出显示
            Link2();

            return Result.Succeeded;

        }
        /// <summary>
        /// link分组
        /// </summary>
        /// <returns></returns>
        public void Link2()
        {
            string result = null;
            int[] array1 = { 4, 3, 1, 2, 5, 4, 3, 1, 2, 5, 1, 2, 5 };
            var query1 = from number in array1
                         group number by number;
            foreach (IGrouping<int, int> number in query1)
            {
                result = result + "\n" + number.Key.ToString();
            }

            TaskDialog.Show("LINQ", result);
        }

    }



}

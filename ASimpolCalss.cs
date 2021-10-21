using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace GLSRevitDev
{
    /// <summary>
    /// 一个简单的类
    /// </summary>
    public class ASimpolCalss
    {   
        public int aNumber = 1;//声明字段

        /// <summary>
        /// 一个简单的方法
        /// </summary>
        /// <param name="aWord"></param>
        /// <returns></returns>
        public string aMethod(string aWord)  //创建一个方法
        {
            aWord = "Welcome !";  //将参数赋值
            return aWord;  //返回参数值
        }

        public void printNumber(string aWord)//Void 没有返回值
        {
            aWord = "Welcome";
        }
    }
}

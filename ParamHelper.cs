using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GLSRevitDev
{
    class ParamHelper
    {
        /// <summary>
        /// Show the parameter values of an element.
        /// </summary>
        public static void ShowParameters(Element e, string header)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Parameter param in e.Parameters)
            {
                string eleName = e.Name;
                string paraName = param.Definition.Name;
                string val = ParameterToString(param);
                sb.Append(eleName + ":\t" + paraName + " \t= \t" + val + "\r\n");
            }

            TaskDialog.Show(header, sb.ToString());
        }

        /// <summary>
        /// Helper function: return a string form of a given parameter.
        /// </summary>
        public static string ParameterToString(Parameter param)
        {
            string val = "none";

            if (param == null)
            {
                return val;
            }

            // To get to the parameter value, we need to pause it depending on its storage type 

            switch (param.StorageType)
            {
                case StorageType.Double:
                    double dVal = param.AsDouble();
                    val = dVal.ToString();
                    break;
                case StorageType.Integer:
                    int iVal = param.AsInteger();
                    val = iVal.ToString();
                    break;
                case StorageType.String:
                    string sVal = param.AsString();
                    val = sVal;
                    break;
                case StorageType.ElementId:
                    ElementId idVal = param.AsElementId();
                    val = idVal.IntegerValue.ToString();
                    break;
                case StorageType.None:
                    break;
            }

            return val;
        }



        #region Helpers for shared parameters
        /// <summary>
        /// Helper to get shared parameters file.
        /// </summary>
        public static DefinitionFile GetSharedParamsFile(
          Autodesk.Revit.ApplicationServices.Application app)
        {
            // Get current shared params file name
            string sharedParamsFileName;
            try
            {
                sharedParamsFileName = app.SharedParametersFilename;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No shared params file set:" + ex.Message);
                return null;
            }
            if (0 == sharedParamsFileName.Length)
            {
                //Environment.SpecialFolder.Personal

                string path = (Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + "GLSBIM" + "\\RevitDevSharedParams.txt");
                StreamWriter stream;
                stream = new StreamWriter(path);
                stream.Close();
                app.SharedParametersFilename = path;
                sharedParamsFileName = app.SharedParametersFilename;
            }
            // Get the current file object and return it
            DefinitionFile sharedParametersFile;
            try
            {
                sharedParametersFile = app.OpenSharedParameterFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannnot open shared params file:" + ex.Message);
                sharedParametersFile = null;
            }
            return sharedParametersFile;
        }

        /// <summary>
        /// Helper to get shared params group.
        /// </summary>
        public static DefinitionGroup GetOrCreateSharedParamsGroup(
         DefinitionFile sharedParametersFile,
         string groupName)
        {
            DefinitionGroup g = sharedParametersFile.Groups.get_Item(groupName);
            if (null == g)
            {
                try
                {
                    g = sharedParametersFile.Groups.Create(groupName);
                }
                catch (Exception)
                {
                    g = null;
                }
            }
            return g;
        }

        /// <summary>
        /// Helper to get shared params definition.
        /// </summary>
        public static Definition GetOrCreateSharedParamsDefinition(
          DefinitionGroup defGroup,
          ParameterType defType,
          string defName,
          bool visible)
        {
            Definition definition = defGroup.Definitions.get_Item(defName);
            if (null == definition)
            {
                try
                {

#if REVIT2016 
                    ExternalDefinitionCreationOptions opt = new ExternalDefinitionCreationOptions(defName, defType);
                    opt.Visible = visible;
                    definition = defGroup.Definitions.Create(opt);
#else    
                    definition = defGroup.Definitions.Create(defName, defType, visible);
#endif


                }
                catch (Exception)
                {
                    definition = null;
                }
            }
            return definition;
        }


        //为指定类别创建爱你共享参数, 不包含事物
        public static bool AddSharedParameter(CategorySet catSet, Document doc, string paramName, BuiltInParameterGroup bipg, ParameterType paramType, string defGroupName, bool isInstance, bool isVisible, out string message)
        {
            message = "";
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;

            #region 判断是否已经存在这个名字的参数

          
            /*
            //first if already the shared parameter exist.
            BindingMap bindingMap = doc.ParameterBindings;
            DefinitionBindingMapIterator iter = bindingMap.ForwardIterator();
            iter.Reset();

            while (iter.MoveNext())
            {
                Definition tempDefinition = iter.Key;

                // find the definition of which the name is the appointed one
                if (tempDefinition.Name == paramName)
                {
                    //it exists
                    Autodesk.Revit.DB.Binding binding = doc.ParameterBindings.get_Item(tempDefinition);
                    if (isInstance == true)
                    {
                        InstanceBinding instBinding = binding as InstanceBinding;
                        CategorySet cats = instBinding.Categories;
                        foreach (Category cat in catSet)
                        {
                            bool exist = false;
                            foreach (Category catExist in instBinding.Categories)
                            {
                                if (cat.Id.IntegerValue == catExist.Id.IntegerValue)
                                {
                                    exist = true;
                                }
                            }
                            if (exist == false)
                                cats.Insert(cat);

                            Autodesk.Revit.DB.Binding bindingNew = null;
                            bindingNew = app.Create.NewInstanceBinding(cats);
                            // We could check if already bound, but looks like Insert will just ignore it in such case
                            doc.ParameterBindings.ReInsert(tempDefinition, bindingNew, bipg);
                        }
                        return true;
                    }
                    else
                    {
                        TypeBinding typeBinding = binding as TypeBinding;
                        CategorySet cats = typeBinding.Categories;
                        foreach (Category cat in catSet)
                        {
                            bool exist = false;
                            foreach (Category catExist in typeBinding.Categories)
                            {
                                if (cat.Id.IntegerValue == catExist.Id.IntegerValue)
                                {
                                    exist = true;
                                }
                            }
                            if (exist == false)
                                cats.Insert(cat);

                            Autodesk.Revit.DB.Binding bindingNew = null;
                            bindingNew = app.Create.NewTypeBinding(cats);
                            // We could check if already bound, but looks like Insert will just ignore it in such case
                            doc.ParameterBindings.ReInsert(tempDefinition, bindingNew, bipg);
                        }
                        return true;
                    }
                }
            }
            */
            #endregion 判断是否已经存在这个名字的参数
            //先看看是否有文件.
            //若无共享参数文件,创建一个.
         

            // Get the current shared params definition file
            DefinitionFile sharedParamsFile = GetSharedParamsFile(app);
            if (null == sharedParamsFile)
            {
                message = "Error getting the shared params file.";  //返回错误消息
                return false;
            }

            // Get or create the shared params group
            DefinitionGroup sharedParamsGroup = GetOrCreateSharedParamsGroup(
              sharedParamsFile, defGroupName);
            if (null == sharedParamsGroup)
            {
                message = "Error getting the shared params group.";
                return false;
            }



            // Visibility of the new parameter:
            // Category.AllowsBoundParameters property indicates if a category can 
            // have shared or project parameters. If it is false, it may not be bound 
            // to shared parameters using the BindingMap. Please note that non-user-visible 
            // parameters can still be bound to these categories. 


            // Get or create the shared params definition
            Definition fireRatingParamDef = GetOrCreateSharedParamsDefinition(
              sharedParamsGroup, paramType, paramName, isVisible);
            if (null == fireRatingParamDef)
            {
                message = "Error in creating shared parameter.";
                return false;
            }
            //if (doc.ParameterBindings.Contains(fireRatingParamDef) == true)  //exists
            //{
            //  Autodesk.Revit.DB.Binding binding = doc.ParameterBindings.get_Item(fireRatingParamDef);
            //  if (isInstance == true)
            //  {
            //    InstanceBinding instBinding = binding as InstanceBinding;
            //    foreach (Category cat in catSet)
            //    {
            //      instBinding.Categories.Insert(cat);
            //    }
            //  }
            //  else
            //  {
            //    TypeBinding typeBinding = binding as TypeBinding;
            //    foreach (Category cat in catSet)
            //    {
            //      typeBinding.Categories.Insert(cat);
            //    }
            //  }
            //}
            //else
            {
                // Bind the param
                try
                {
                    Autodesk.Revit.DB.Binding binding = null;
                    if (isInstance == true)
                        binding = app.Create.NewInstanceBinding(catSet);
                    else
                        binding = app.Create.NewTypeBinding(catSet);
                    // We could check if already bound, but looks like Insert will just ignore it in such case
                    doc.ParameterBindings.Insert(fireRatingParamDef, binding, bipg);
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get GUID for a given shared param name.
        /// </summary>
        /// <param name="app">Revit application</param>
        /// <param name="defGroup">Definition group name</param>
        /// <param name="defName">Definition name</param>
        /// <returns>GUID</returns>
        public static Guid SharedParamGUID(Autodesk.Revit.ApplicationServices.Application app, string defGroup, string defName)
        {
            Guid guid = Guid.Empty;
            try
            {
                DefinitionFile file = app.OpenSharedParameterFile();
                DefinitionGroup group = file.Groups.get_Item(defGroup);
                Definition definition = group.Definitions.get_Item(defName);
                ExternalDefinition externalDefinition = definition as ExternalDefinition;
                guid = externalDefinition.GUID;
            }
            catch (Exception)
            {
            }
            return guid;
        }
        #endregion // Helpers for shared parameters


    }
}

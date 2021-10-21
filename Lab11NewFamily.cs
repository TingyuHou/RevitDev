#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
#endregion

namespace GLSRevitDev
{

    /// <summary>
    /// L柱子
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class Lab11NewFamily_FamilyCreateColumnLShape : IExternalCommand
    {
        // member variables for top level access to the Revit database
        //
        Application _app;
        Document _doc;
        string title = "GLSRevitDev";

        // command main
        //
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            // objects for the top level access
            //
            _app = commandData.Application.Application;
            _doc = commandData.Application.ActiveUIDocument.Document;

            // (0) This command works in the context of family editor only.
            //     We also check if the template is for an appropriate category if needed.
            //     Here we use a Column(i.e., Metric Column.rft) template.
            //     Although there is no specific checking about metric or imperial, our lab only works in metric for now.
            //
            if (!isRightTemplate(BuiltInCategory.OST_Columns))
            {
                TaskDialog.Show(title, "请打开 公制柱.rft");
                return Result.Failed;
            }

            using (Transaction ts = new Transaction(_doc))
            {
                ts.Start("L");


                // (1.1) 添加俩参照平面 add reference planes
                addReferencePlanes();


                //创建拉伸体
                Extrusion pSolid = createSolid();
                _doc.Regenerate();

                // (2)创建对齐之后锁定  只有高度相同才能够锁定 revitApi不会自动拉伸到对齐
                addAlignments(pSolid);

                //添加参数
                addParameters();

                //添加标签
                addDimensions();

                //添加多个族类型
                addTypes();
                ts.Commit();
            }

            // test family parameter value modification:
            //modifyFamilyParamValue();

            // finally, return
            return Result.Succeeded;
        }

        // ============================================
        //   (0) check if we have a correct template
        // ============================================
        bool isRightTemplate(BuiltInCategory targetCategory)
        {
            // This command works in the context of family editor only.
            //
            if (!_doc.IsFamilyDocument)
            {
                TaskDialog.Show(title, "This command works only in the family editor.");
                return false;
            }

            // Check the template for an appropriate category here if needed.
            //
            Category cat = _doc.Settings.Categories.get_Item(targetCategory);
            if (_doc.OwnerFamily == null)
            {
                TaskDialog.Show(title, "This command only works in the family context.");
                return false;
            }
            if (!cat.Id.Equals(_doc.OwnerFamily.FamilyCategory.Id))
            {
                TaskDialog.Show(title, "Category of this family document does not match the context required by this command.");
                return false;
            }

            // if we come here, we should have a right one.
            return true;
        }

        /// <summary>
        /// (1.1) 添加俩参照平面 add reference planes
        /// </summary>
        void addReferencePlanes()
        {
            //
            // we are defining a simple L-shape profile like the following:
            //
            //  5 tw 4
            //   +-+
            //   | | 3          h = height
            // d | +---+ 2
            //   +-----+ td
            //  0        1
            //  6  w
            //
            //
            // we want to add ref planes along (1) 2-3 and (2)3-4.
            // Name them "OffsetH" and "OffsetV" respectively. (H for horizontal, V for vertical).
            //
            double tw = mmToFeet(150.0);  // thickness added for Lab2.  Hard-coding for simplicity.
            double td = mmToFeet(150.0);

            //
            // (1) add a horizonal ref plane 2-3.
            //
            // get a plan view
            View pViewPlan = findElement(typeof(ViewPlan), "低于参照标高") as View; //低于参照标高

            // we have predefined ref plane: Left/Right/Front/Back
            // get the ref plane at Front, which is aligned to line 2-3
            ReferencePlane refFront = findElement(typeof(ReferencePlane), "前") as ReferencePlane;//Front 前

            // get the bubble and free ends from front ref plane and offset by td.
            //与前视图相同的参照线
            XYZ p1 = refFront.BubbleEnd;
            XYZ p2 = refFront.FreeEnd;
            XYZ pBubbleEnd = new XYZ(p1.X, p1.Y + td, p1.Z);
            XYZ pFreeEnd = new XYZ(p2.X, p2.Y + td, p2.Z);

            // create a new one reference plane and name it "OffsetH"
            //
            ReferencePlane refPlane = _doc.FamilyCreate.NewReferencePlane(pBubbleEnd, pFreeEnd, XYZ.BasisZ, pViewPlan);
            refPlane.Name = "OffsetH";

            //
            // (2) do the same to add a vertical reference plane.
            //

            // find the ref plane at left, which is aligned to line 3-4
            ReferencePlane refLeft = findElement(typeof(ReferencePlane), "左") as ReferencePlane;

            // get the bubble and free ends from front ref plane and offset by td.
            //
            p1 = refLeft.BubbleEnd;
            p2 = refLeft.FreeEnd;
            pBubbleEnd = new XYZ(p1.X + tw, p1.Y, p1.Z);
            pFreeEnd = new XYZ(p2.X + tw, p2.Y, p2.Z);

            // create a new reference plane and name it "OffsetV"
            //与前视图相同的参照线
            refPlane = _doc.FamilyCreate.NewReferencePlane(pBubbleEnd, pFreeEnd, XYZ.BasisZ, pViewPlan);
            refPlane.Name = "OffsetV";
        }

        /// <summary>
        /// 创建拉伸体
        /// </summary>
        /// <returns></returns>
        Extrusion createSolid()
        {
            //
            // (1) define a simple L-shape profile
            //定义一个截面形状所需要的线
            //
            CurveArrArray pProfile = createProfileLShape();


            // (2) 草图 create a sketch plane   低于参照标高 低于参照标高 参照平面

            ReferencePlane pRefPlane = findElement(typeof(ReferencePlane), "参照平面") as ReferencePlane;  // need to know from the template

            SketchPlane pSketchPlane = SketchPlane.Create(_doc, pRefPlane.Plane);  // Revit 2014

            double dHeight = mmToFeet(4000.0);

            // (4) create an extrusion here. at this point. just an box, nothing else.
            //
            bool bIsSolid = true;  // as oppose to void.
            return _doc.FamilyCreate.NewExtrusion(bIsSolid, pProfile, pSketchPlane, dHeight);
        }


        //创建截面形状————L
        CurveArrArray createProfileLShape()
        {
            //
            // define a simple L-shape profile
            //
            //  5 tw 4
            //   +-+
            //   | | 3          h = height
            // d | +---+ 2
            //   +-----+ td
            //  0        1
            //  6  w
            //

            // sizes (hard coded for simplicity)
            // note: these need to match reference plane. otherwise, alignment won't work.
            // as an exercise, try changing those values and see how it behaves.
            //
            double w = mmToFeet(600.0); // those are hard coded for simplicity here. in practice, you may want to find out from the references)
            double d = mmToFeet(600.0);
            double tw = mmToFeet(150.0); // thickness added for Lab2
            double td = mmToFeet(150.0);

            // define vertices
            //
            const int nVerts = 6; // the number of vertices

            XYZ[] pts = new XYZ[] {
        new XYZ(-w / 2.0, -d / 2.0, 0.0),
        new XYZ(w / 2.0, -d / 2.0, 0.0),
        new XYZ(w / 2.0, (-d / 2.0) + td, 0.0),
        new XYZ((-w / 2.0) + tw, (-d / 2.0) + td, 0.0),
        new XYZ((-w / 2.0) + tw, d / 2.0, 0.0),
        new XYZ(-w / 2.0, d / 2.0, 0.0),
        new XYZ(-w / 2.0, -d / 2.0, 0.0) };  // the last one is to make the loop simple

            // define a loop. define individual edges and put them in a curveArray
            //
            CurveArray pLoop = _app.Create.NewCurveArray();
            for (int i = 0; i < nVerts; ++i)
            {
                //Line line = _app.Create.NewLineBound(pts[i], pts[i + 1]);  // Revit 2013
                Line line = Line.CreateBound(pts[i], pts[i + 1]);  // Revit 2014
                pLoop.Append(line);
            }

            // then, put the loop in the curveArrArray as a profile
            //
            CurveArrArray pProfile = _app.Create.NewCurveArrArray();
            pProfile.Append(pLoop);
            // if we come here, we have a profile now.

            return pProfile;
        }

        //创建矩形截面————口
        CurveArrArray createProfileRectangle()
        {
            //
            // define a simple rectangular profile
            //
            //  3     2
            //   +---+
            //   |   | d    h = height
            //   +---+
            //  0     1
            //  4  w
            //

            // sizes (hard coded for simplicity)
            // note: these need to match reference plane. otherwise, alignment won't work.
            // as an exercise, try changing those values and see how it behaves.
            //
            double w = mmToFeet(600.0);
            double d = mmToFeet(600.0);

            // define vertices
            //
            const int nVerts = 4; // the number of vertices

            XYZ[] pts = new XYZ[] {
        new XYZ(-w / 2.0, -d / 2.0, 0.0),
        new XYZ(w / 2.0, -d / 2.0, 0.0),
        new XYZ(w / 2.0, d / 2.0, 0.0),
        new XYZ(-w / 2.0, d / 2.0, 0.0),
        new XYZ(-w / 2.0, -d / 2.0, 0.0) };

            // define a loop. define individual edges and put them in a curveArray
            //
            CurveArray pLoop = _app.Create.NewCurveArray();
            for (int i = 0; i < nVerts; ++i)
            {
                //Line line = _app.Create.NewLineBound(pts[i], pts[i + 1]);  // Revit 2013
                Line line = Line.CreateBound(pts[i], pts[i + 1]);  // Revit 2014
                pLoop.Append(line);
            }

            // then, put the loop in the curveArrArray as a profile
            //
            CurveArrArray pProfile = _app.Create.NewCurveArrArray();
            pProfile.Append(pLoop);
            // if we come here, we have a profile now.

            return pProfile;
        }

        /// <summary>        
        /// (2)创建对齐之后锁定  只有高度相同才能够锁定 revitApi不会自动拉伸到对齐
        /// </summary>
        /// <param name="pBox"></param>
        void addAlignments(Extrusion pBox)
        {
            //立面的对齐
            View pView = findElement(typeof(View), "前") as View;

            Level upperLevel = findElement(typeof(Level), "高于参照标高") as Level;
            Reference ref1 = upperLevel.PlaneReference;

            PlanarFace upperFace = findFace(pBox, new XYZ(0.0, 0.0, 1.0));
            Reference ref2 = upperFace.Reference;

            _doc.FamilyCreate.NewAlignment(pView, ref1, ref2);

            Level lowerLevel = findElement(typeof(Level), "低于参照标高") as Level;
            Reference ref3 = lowerLevel.PlaneReference;

            PlanarFace lowerFace = findFace(pBox, new XYZ(0.0, 0.0, -1.0)); // find a face whose normal is z-down.
            Reference ref4 = lowerFace.Reference;

            _doc.FamilyCreate.NewAlignment(pView, ref3, ref4);

            //平面的对齐         
            View pViewPlan = findElement(typeof(ViewPlan), "低于参照标高") as View;

            // find reference planes
            ReferencePlane refRight = findElement(typeof(ReferencePlane), "右") as ReferencePlane;
            ReferencePlane refLeft = findElement(typeof(ReferencePlane), "左") as ReferencePlane;
            ReferencePlane refFront = findElement(typeof(ReferencePlane), "前") as ReferencePlane;
            ReferencePlane refBack = findElement(typeof(ReferencePlane), "后面") as ReferencePlane;
            ReferencePlane refOffsetV = findElement(typeof(ReferencePlane), "OffsetV") as ReferencePlane; // added for L-shape
            ReferencePlane refOffsetH = findElement(typeof(ReferencePlane), "OffsetH") as ReferencePlane; // added for L-shape

            // find the face of the box
            // Note: findFace need to be enhanced for this as face normal is not enough to determine the face.
            //
            PlanarFace faceRight = findFace(pBox, new XYZ(1.0, 0.0, 0.0), refRight); // modified for L-shape
            PlanarFace faceLeft = findFace(pBox, new XYZ(-1.0, 0.0, 0.0));
            PlanarFace faceFront = findFace(pBox, new XYZ(0.0, -1.0, 0.0));
            PlanarFace faceBack = findFace(pBox, new XYZ(0.0, 1.0, 0.0), refBack); // modified for L-shape
            PlanarFace faceOffsetV = findFace(pBox, new XYZ(1.0, 0.0, 0.0), refOffsetV); // added for L-shape
            PlanarFace faceOffsetH = findFace(pBox, new XYZ(0.0, 1.0, 0.0), refOffsetH); // added for L-shape

            // create alignments
            //
            _doc.FamilyCreate.NewAlignment(pViewPlan, refRight.Reference, faceRight.Reference);
            _doc.FamilyCreate.NewAlignment(pViewPlan, refLeft.Reference, faceLeft.Reference);
            _doc.FamilyCreate.NewAlignment(pViewPlan, refFront.Reference, faceFront.Reference);
            _doc.FamilyCreate.NewAlignment(pViewPlan, refBack.Reference, faceBack.Reference);
            _doc.FamilyCreate.NewAlignment(pViewPlan, refOffsetV.Reference, faceOffsetV.Reference); // added for L-shape
            _doc.FamilyCreate.NewAlignment(pViewPlan, refOffsetH.Reference, faceOffsetH.Reference); // added for L-shape
        }

        // ======================================
        //   (3.1) add parameters
        // ======================================
        void addParameters()
        {
            FamilyManager mgr = _doc.FamilyManager;

            // API parameter group for Dimension is PG_GEOMETRY:
            //
            FamilyParameter paramTw = mgr.AddParameter(
              "Tw", BuiltInParameterGroup.PG_GEOMETRY,
              ParameterType.Length, false);

            FamilyParameter paramTd = mgr.AddParameter(
              "Td", BuiltInParameterGroup.PG_GEOMETRY,
              ParameterType.Length, false);

            // set initial values:
            //
            double tw = mmToFeet(150.0);
            double td = mmToFeet(150.0);
            mgr.Set(paramTw, tw);
            mgr.Set(paramTd, td);
        }

        /// <summary>
        ///   //添加标注标签
        /// </summary>
        void addDimensions()
        {
            // find the plan view
            //
            View pViewPlan = findElement(typeof(ViewPlan), "低于参照标高") as View;

            // find reference planes
            //
            ReferencePlane refLeft = findElement(typeof(ReferencePlane), "左") as ReferencePlane;
            ReferencePlane refFront = findElement(typeof(ReferencePlane), "前") as ReferencePlane;
            ReferencePlane refOffsetV = findElement(typeof(ReferencePlane), "OffsetV") as ReferencePlane; // OffsetV is added for L-shape
            ReferencePlane refOffsetH = findElement(typeof(ReferencePlane), "OffsetH") as ReferencePlane; // OffsetH is added for L-shape

            //
            // (1)  add dimension between the reference planes 'Left' and 'OffsetV', and label it as 'Tw'
            //

            // define a dimension line
            //
            XYZ p0 = refLeft.FreeEnd;
            XYZ p1 = refOffsetV.FreeEnd;
            // Line pLine = _app.Create.NewLineBound(p0, p1);  // Revit 2013
            Line pLine = Line.CreateBound(p0, p1);  // Revit 2014

            // define references
            //
            ReferenceArray pRefArray = new ReferenceArray();
            pRefArray.Append(refLeft.Reference);
            pRefArray.Append(refOffsetV.Reference);

            // create a dimension
            //
            Dimension pDimTw = _doc.FamilyCreate.NewDimension(pViewPlan, pLine, pRefArray);

            // add label to the dimension
            //
            FamilyParameter paramTw = _doc.FamilyManager.get_Parameter("Tw");
            // pDimTw.Label = paramTw;  //Revit 2013
            pDimTw.FamilyLabel = paramTw;  // Revit 2014

            //
            // (2)  do the same for dimension between 'Front' and 'OffsetH', and lable it as 'Td'
            //

            // define a dimension line
            //
            p0 = refFront.FreeEnd;
            p1 = refOffsetH.FreeEnd;
            //pLine = _app.Create.NewLineBound(p0, p1);  // Revit 2013
            pLine = Line.CreateBound(p0, p1);  // Revit 2014

            // define references
            //
            pRefArray = new ReferenceArray();
            pRefArray.Append(refFront.Reference);
            pRefArray.Append(refOffsetH.Reference);

            // create a dimension
            //
            Dimension pDimTd = _doc.FamilyCreate.NewDimension(pViewPlan, pLine, pRefArray);

            // add label to the dimension
            //
            FamilyParameter paramTd = _doc.FamilyManager.get_Parameter("Td");
            // pDimTd.Label = paramTd;  // Revit 2013
            pDimTd.FamilyLabel = paramTd;  // Revit 2014
        }

        // ======================================
        //  (3.3) add types
        // ======================================
        void addTypes()
        {
            // addType(name, Width, Depth)
            //
            //addType("600x900", 600.0, 900.0)
            //addType("1000x300", 1000.0, 300.0)
            //addType("600x600", 600.0, 600.0)

            // addType(name, Width, Depth, Tw, Td)
            //
            addType("600x900", 600.0, 900.0, 150.0, 225.0);
            addType("1000x300", 1000.0, 300.0, 250.0, 75.0);
            addType("600x600", 600.0, 600.0, 150.0, 150.0);
        }

        // add one type (version 2)
        //
        void addType(string name, double w, double d, double tw, double td)
        {
            // get the family manager from the current doc
            FamilyManager pFamilyMgr = _doc.FamilyManager;

            // add new types with the given name
            //
            FamilyType type1 = pFamilyMgr.NewType(name);

            // look for 'Width' and 'Depth' parameters and set them to the given value
            //

            // first 'Width'
            //
            FamilyParameter paramW = pFamilyMgr.get_Parameter("Width");
            double valW = mmToFeet(w);
            if (paramW != null)
            {
                pFamilyMgr.Set(paramW, valW);
            }

            // same idea for 'Depth'
            //
            FamilyParameter paramD = pFamilyMgr.get_Parameter("Depth");
            double valD = mmToFeet(d);
            if (paramD != null)
            {
                pFamilyMgr.Set(paramD, valD);
            }

            // let's set "Tw' and 'Td'
            //
            FamilyParameter paramTw = pFamilyMgr.get_Parameter("Tw");
            double valTw = mmToFeet(tw);
            if (paramTw != null)
            {
                pFamilyMgr.Set(paramTw, valTw);
            }
            FamilyParameter paramTd = pFamilyMgr.get_Parameter("Td");
            double valTd = mmToFeet(td);
            if (paramTd != null)
            {
                pFamilyMgr.Set(paramTd, valTd);
            }
        }


        // add one type (version 1)
        //
        void addType(string name, double w, double d)
        {
            // get the family manager from the current doc
            FamilyManager pFamilyMgr = _doc.FamilyManager;

            // add new types with the given name
            //
            FamilyType type1 = pFamilyMgr.NewType(name);

            // look for 'Width' and 'Depth' parameters and set them to the given value
            //
            // first 'Width'
            //
            FamilyParameter paramW = pFamilyMgr.get_Parameter("Width");
            double valW = mmToFeet(w);
            if (paramW != null)
            {
                pFamilyMgr.Set(paramW, valW);
            }

            // same idea for 'Depth'
            //
            FamilyParameter paramD = pFamilyMgr.get_Parameter("Depth");
            double valD = mmToFeet(d);
            if (paramD != null)
            {
                pFamilyMgr.Set(paramD, valD);
            }
        }

        void modifyFamilyParamValue()
        {
            FamilyManager mgr = _doc.FamilyManager;

            FamilyParameter[] a = new FamilyParameter[] {
        mgr.get_Parameter( "宽度" ),
        mgr.get_Parameter( "深度" )
      };

            foreach (FamilyType t in mgr.Types)
            {
                mgr.CurrentType = t;
                foreach (FamilyParameter fp in a)
                {
                    if (t.HasValue(fp))
                    {
                        double x = (double)t.AsDouble(fp);
                        mgr.Set(fp, 2.0 * x);
                    }
                }
            }
        }

        #region Helper Functions

        // ===============================================================
        // helper function: given a solid, find a planar 
        // face with the given normal (version 2)
        // this is a slightly enhaced version of the previous 
        // version and checks if the face is on the given reference plane.
        // ===============================================================
        /// <summary>
        /// 给定的平面相交的
        /// </summary>
        /// <param name="pBox"></param>
        /// <param name="normal"></param>
        /// <param name="refPlane"></param>
        /// <returns></returns>
        PlanarFace findFace(Extrusion pBox, XYZ normal, ReferencePlane refPlane)
        {
            // get the geometry object of the given element
            //
            Options op = new Options();
            op.ComputeReferences = true;
            GeometryElement geomElem = pBox.get_Geometry(op);

            // loop through the array and find a face with the given normal
            //
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)  // solid is what we are interested in.
                {
                    Solid pSolid = geomObj as Solid;
                    FaceArray faces = pSolid.Faces;
                    foreach (Face pFace in faces)
                    {
                        PlanarFace pPlanarFace = (PlanarFace)pFace;
                        // check to see if they have same normal
                        if ((pPlanarFace != null) && pPlanarFace.Normal.IsAlmostEqualTo(normal))
                        {
                            // additionally, we want to check if the face is on the reference plane

                            XYZ p0 = refPlane.BubbleEnd;
                            XYZ p1 = refPlane.FreeEnd;

                            Line pCurve = Line.CreateBound(p0, p1);  // Revit 2014
                            if (pPlanarFace.Intersect(pCurve) == SetComparisonResult.Subset)
                            {
                                return pPlanarFace; // we found the face
                            }
                        }
                    }
                }

                // will come back later as needed.
                //
                //else if (geomObj is Instance)
                //{
                //}
                //else if (geomObj is Curve)
                //{
                //}
                //else if (geomObj is Mesh)
                //{
                //}
            }

            // if we come here, we did not find any.
            return null;
        }

        // =============================================================
        //   helper function: find a planar face with the given normal
        // =============================================================
        /// <summary>
        /// 给定的方向的
        /// </summary>
        /// <param name="pBox"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        PlanarFace findFace(Extrusion pBox, XYZ normal)
        {
            // get the geometry object of the given element
            //
            Options op = new Options();
            op.ComputeReferences = true;
            GeometryElement geomElem = pBox.get_Geometry(op);

            // loop through the array and find a face with the given normal
            //
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)  // solid is what we are interested in.
                {
                    Solid pSolid = geomObj as Solid;
                    FaceArray faces = pSolid.Faces;
                    foreach (Face pFace in faces)
                    {
                        PlanarFace pPlanarFace = (PlanarFace)pFace;
                        if ((pPlanarFace != null) && pPlanarFace.Normal.IsAlmostEqualTo(normal)) // we found the face
                        {
                            return pPlanarFace;
                        }
                    }
                }
                // will come back later as needed.
                //
                //else if (geomObj is Instance)
                //{
                //}
                //else if (geomObj is Curve)
                //{
                //}
                //else if (geomObj is Mesh)
                //{
                //}
            }

            // if we come here, we did not find any.
            return null;
        }

        // ==================================================================================
        //   helper function: find an element of the given type and the name.
        //   You can use this, for example, to find Reference or Level with the given name.
        // ==================================================================================
        Element findElement(Type targetType, string targetName)
        {
            // get the elements of the given type
            //
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            collector.WherePasses(new ElementClassFilter(targetType));

            // parse the collection for the given name
            // using LINQ query here. 
            // 
            var targetElems = from element in collector where element.Name.Equals(targetName) select element;
            List<Element> elems = targetElems.ToList<Element>();

            if (elems.Count > 0)
            {  // we should have only one with the given name. 
                return elems[0];
            }

            // cannot find it.
            return null;
        }

        // ===============================================
        //   helper function: convert millimeter to feet
        // ===============================================
        double mmToFeet(double mmVal)
        {
            return mmVal / 304.8;
        }

        #endregion // Helper Functions
    }
}

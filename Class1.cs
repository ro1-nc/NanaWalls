using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanaWalls
{
    public class Class1 : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panel = null;
            RibbonPanel panel2 = null;
            RibbonPanel panel3 = null;
            RibbonPanel panel4 = null;
            RibbonPanel panel5 = null;
            RibbonPanel panel6 = null;
            RibbonPanel panel7 = null;
            RibbonPanel panel8 = null;
            
            string tName = "NanaWalls";
            
            string pName = "NanaWalls";
            string pName2= "Family";
            string pName3= "Chair Instance";
            string pName4= "Rotate Left";
            string pName5= "Rotate Right";
            string pName6= "Get Paramters";
            string pName7= "Rotate Door";
            string pName8= "Extrude profile";

            try
            {
                application.CreateRibbonTab(tName);
                panel = application.CreateRibbonPanel(tName, pName);
                panel2 = application.CreateRibbonPanel(tName, pName2);
                panel3 = application.CreateRibbonPanel(tName, pName3);
                panel4 = application.CreateRibbonPanel(tName, pName4);
                panel5 = application.CreateRibbonPanel(tName, pName5);
                panel6 = application.CreateRibbonPanel(tName, pName6);
                panel7 = application.CreateRibbonPanel(tName, pName7);
                panel8 = application.CreateRibbonPanel(tName, pName8);
            }
            catch (Exception e)
            {

            }



            Hello.AddButton(panel);
            FamInstance.AddButton(panel2);
            ChairInstance.AddButton(panel3);
            RotateLeftElement.AddButton(panel4);
            RotateRightElement.AddButton(panel5);
            Parametrs.AddButton(panel6);
            RotateDoor.AddButton(panel7);
            ExtrudeWall.AddButton(panel8);
            
            return Result.Succeeded;
        }
    }
}

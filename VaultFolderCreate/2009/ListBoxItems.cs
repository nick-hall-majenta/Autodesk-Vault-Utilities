/*=====================================================================
  
  This file is part of the Autodesk Vault API Code Samples.

  Copyright (C) Autodesk Inc.  All rights reserved.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/

using System;
using System.Collections.Generic;
using System.Text;

using VaultFolderCreate.DocumentSvc;

namespace VaultFolderCreate
{
    /// <summary>
    /// A list box item which contains a File object
    /// </summary>
    public class ListBoxFileItem
    {
        private File file;
        public File File
        {
            get { return file; }
        }

        public ListBoxFileItem(File f)
        {
            file = f;

        }

        /// <summary>
        /// Determines the text displayed in the ListBox
        /// </summary>
        public override string ToString()
        {
            return this.file.Name;
        }
    }


    /// <summary>
    /// A list box item which contains a BOMInst and BOMComp object
    /// </summary>
    class ListBoxBOMInstItem
    {
        public BOMInst BOMInst;
        public BOMComp BOMComp;

        public ListBoxBOMInstItem(BOMInst inst, BOMComp comp)
        {
            BOMInst = inst;
            BOMComp = comp;
        }

        public override string ToString()
        {
            return BOMComp.Name + " (Qty. " + (BOMInst.Quant * BOMComp.BaseQty) + " " + BOMComp.BaseUOM + ")";
        }
    }


    /// <summary>
    /// A list box item which contains a BOMCompAttr and BOMComp object 
    /// </summary>
    class ListBoxBOMCompAttrItem
    {
        public BOMCompAttr BOMCompAttr;
        public BOMProp BOMProp;

        public ListBoxBOMCompAttrItem(BOMCompAttr attribute, BOMProp property)
        {
            BOMCompAttr = attribute;
            BOMProp = property;
        }

        public override string ToString()
        {
            return BOMProp.DispName + " = " + BOMCompAttr.Val;
        }
    }

    /// <summary>
    /// A list box item which contains a PropDef object 
    /// </summary>
    class ListBoxPropDefItem
    {
        public PropDef PropDef;

        public ListBoxPropDefItem(PropDef propDef)
        {
            this.PropDef = propDef;
        }

        public override string ToString()
        {
            return PropDef.DispName;
        }
    }

    /// <summary>
    /// A list box item which contains a SrchCond and PropDef object 
    /// </summary>
    class ListBoxSrchCondItem
    {
        public SrchCond SrchCond;
        public PropDef PropDef;

        public ListBoxSrchCondItem(SrchCond srchCond, PropDef propDef)
        {
            this.SrchCond = srchCond;
            this.PropDef = propDef;
        }

        public override string ToString()
        {
            string conditionName = Condition.GetCondition(SrchCond.SrchOper).DisplayName;
            return String.Format("{0} {1} {2}", PropDef.DispName, conditionName, SrchCond.SrchTxt);
        }
    }
}

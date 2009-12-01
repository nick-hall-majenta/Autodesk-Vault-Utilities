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

namespace VaultFolderCreate
{
    public class Condition
    {
        public static Condition CONTAINS = new Condition("contains", 1);
        public static Condition DOES_NOT_CONTAIN = new Condition("does not contain", 2);
        public static Condition EQUALS = new Condition("equals", 3);
        public static Condition IS_EMPTY = new Condition("is empty", 4);
        public static Condition IS_NOT_EMPTY = new Condition("is not empty", 5);
        public static Condition LESS_THAN_OR_EQUAL = new Condition("<=", 9);
        public static Condition LESS_THAN = new Condition("<", 8);
        public static Condition GREATER_THAN_OR_EQUAL = new Condition(">=", 7);
        public static Condition GREATER_THAN = new Condition(">", 6);
        public static Condition NOT_EQUAL = new Condition("<>", 10);

        private static Dictionary<long, Condition> m_conditionHash = null;
 
        public static Condition GetCondition(long conditionId)
        {
            if (m_conditionHash == null)
            {
                m_conditionHash = new Dictionary<long, Condition>();
                m_conditionHash.Add(CONTAINS.Code, CONTAINS);
                m_conditionHash.Add(DOES_NOT_CONTAIN.Code, DOES_NOT_CONTAIN);
                m_conditionHash.Add(EQUALS.Code, EQUALS);
                m_conditionHash.Add(IS_EMPTY.Code, IS_EMPTY);
                m_conditionHash.Add(IS_NOT_EMPTY.Code, IS_NOT_EMPTY);
                m_conditionHash.Add(LESS_THAN_OR_EQUAL.Code, LESS_THAN_OR_EQUAL);
                m_conditionHash.Add(LESS_THAN.Code, LESS_THAN);
                m_conditionHash.Add(GREATER_THAN_OR_EQUAL.Code, GREATER_THAN_OR_EQUAL);
                m_conditionHash.Add(GREATER_THAN.Code, GREATER_THAN);
                m_conditionHash.Add(NOT_EQUAL.Code, NOT_EQUAL);
            }

            return m_conditionHash[conditionId];
        }

        long m_code;
        string m_displayName;

        /// <summary>
        /// Value to be displayed in the condition combo box.
        /// </summary>
        public string DisplayName
        {
            get { return m_displayName; }
        }

        /// <summary>
        /// A code to use when making the web service call.
        /// </summary>
        public long Code
        {
            get { return m_code; }
        }

        private Condition(string displayName, long code)
        {
            m_displayName = displayName;
            m_code = code;
        }

        /// <summary>
        /// Returns the current value of the DisplayName property.
        /// </summary>
        public override string ToString()
        {
            return m_displayName;
        }
    }
}

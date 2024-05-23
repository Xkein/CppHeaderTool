using CppAst;
using CppHeaderTool.Meta;
using CppHeaderTool.Specifies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Types
{
    public class HtProperty : HtType, IHasCppElement
    {
        public CppElement element => cppField;
        public CppField cppField;
        public bool isStatic => cppField.StorageQualifier == CppStorageQualifier.Static;
        public bool isConst
        {
            get
            {
                string str = cppField.Type.ToString();
                if (str.Contains('&'))
                    return false;
                int constPos = str.IndexOf("const");
                int ptrPos = str.IndexOf('*');
                if (ptrPos == -1)
                    return constPos >= 0;
                return ptrPos < constPos;
            }
        }

        public PropertyMeta meta;
    }
}

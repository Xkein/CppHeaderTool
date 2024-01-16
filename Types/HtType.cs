using CppAst;
using CppHeaderTool.Specifies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Types
{
    public class HtType
    {

    }

    public interface IHasCppElement
    {
        CppElement element { get; }
    }

}

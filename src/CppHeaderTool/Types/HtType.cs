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

    public static class HtTypeExtension
    {
        public static T GetUserData<T>(this CppElement element)
        {
            return (T)element.UserData;
        }
    }

    public interface IHasCppElement
    {
        CppElement element { get; }
    }

}

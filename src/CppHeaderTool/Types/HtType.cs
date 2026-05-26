using CppAst;
using CppHeaderTool.Specifies;
using CppHeaderTool.Tables;
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

        public static string FormatIdentifier(this string fullName, bool replaceArrayBrackets = true)
        {
            string identifier = fullName
                .Replace('<', '_')
                .Replace('>', '_')
                .Replace(':', '_')
                .Replace(',', '_')
                .Replace('*', '_')
                .Replace(" ", "")
                .Replace("(", "_").Replace(")", "_")
                .Replace("[", "_").Replace("]", "_");
            return identifier;
        }

        public static CppType UnwrapType(this CppType type)
        {
            switch (type.TypeKind)
            {
                case CppTypeKind.Pointer:
                case CppTypeKind.Reference:
                case CppTypeKind.Array:
                case CppTypeKind.Qualified:
                    return UnwrapType((type as CppTypeWithElementType).ElementType);
                case CppTypeKind.Unexposed:
                    string fullName = type.FullName;
                    if (type.FullName.EndsWith('*') && Session.typeTables.TryGetClass(fullName.Substring(0, fullName.Length - 2), out HtClass klass))
                    {
                        return klass.cppClass;
                    }
                    return type;
                default:
                    return type;
            }
        }
    }

    public interface IHasCppElement
    {
        CppElement element { get; }
    }

}

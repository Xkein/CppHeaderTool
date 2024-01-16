using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Tables
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HeaderToolAttribute : Attribute
    {
    }

    internal class HtTables
    {

        //public SpecifierTables specifierTables { get; } = new();
        public MetaTables metaTables { get; } = new();
        public TypeTables typeTables { get; } = new();


        public HtTables()
        {
            //CheckForAttributes(Assembly.GetExecutingAssembly());
            //PostInitialization();
        }

        private void CheckForAttributes(Assembly? assembly)
        {
            if (assembly != null)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    CheckForAttributes(type);
                }
            }
        }
        private void CheckForAttributes(Type type)
        {
            if (type.IsClass)
            {

                // Loop through the attributes
                foreach (Attribute classAttribute in type.GetCustomAttributes(false))
                {
                    if (classAttribute is HeaderToolAttribute parserAttribute)
                    {
                        HandleHeaderToolAttribute(type, parserAttribute);
                    }
                }
            }
        }

        private void PostInitialization()
        {

        }

        private void HandleHeaderToolAttribute(Type type, HeaderToolAttribute parserAttribute)
        {
            // Scan the methods for things we are interested in
            foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {

                // Scan for attributes we care about
                foreach (Attribute methodAttribute in methodInfo.GetCustomAttributes())
                {
                    if (methodAttribute is HtSpecifierAttribute specifierAttribute)
                    {
                        //specifierTables.OnSpecifierAttribute(type, methodInfo, specifierAttribute);
                    }
                }
            }
        }
    }
}

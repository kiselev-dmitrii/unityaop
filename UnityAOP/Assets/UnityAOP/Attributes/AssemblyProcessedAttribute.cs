using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.UnityAOP.Attributes {
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyProcessedAttribute : Attribute {
    }
}

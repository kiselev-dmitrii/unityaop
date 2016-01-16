using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Observable;

namespace Assets {
    public interface Interface {
        
    }

    public class Implementation : Interface {
        
    }

public class Test {
    public Implementation X { get; set; }

    public Test() {
        object obj = new SetterDelegate<Interface>(delegate(Interface value) { X = (Implementation)value; });
    }
}
}

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

    public int Y { get; set; }

    public Test() {
        object obj = new SetterDelegate<Interface>(Setter);
        object obj2 = new GetterDelegate<Interface>(Getter);
    }

    public void Setter(Interface value) {
        X = (Implementation) value;
    }

    public Interface Getter() {
        return X;
    }
}
}

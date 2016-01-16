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

    public List<Object> Getters; 

    public Test() {
        Getters = new List<object>();
        Getters.Add(new GetterDelegate<Interface>(Getter));
        Getters.Add(new GetterDelegate<Interface>(Getter));
        Getters.Add(null);
    }

    public void Setter(Interface value) {
        X = (Implementation) value;
    }

    public Interface Getter() {
        return X;
    }
}
}

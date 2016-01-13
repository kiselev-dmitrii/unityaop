using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.ObservableTest {
public class BaseClass {
    public virtual void BaseMethod(int a) {
        UnityEngine.Debug.Log("Base method");
    }
}

public class EmptyClass : BaseClass {
}
}

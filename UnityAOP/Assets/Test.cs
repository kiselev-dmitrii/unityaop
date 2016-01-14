using System;
using UnityEngine;

namespace Assets {
public class Test1 {
    public Test1() {
        
    }

    public Test1(int a) {
        
    }
}

public class Test2 {
    public Test2() {
        Debug.Log(0);
    }

    public Test2(int a) {
        Debug.Log(0);
    }
}

public class TestBase {
    public TestBase() {
    }

    public TestBase(int a) {
        
    }
}

public class TestDerived : TestBase {
    public TestDerived() : base() {
        
    }

    public TestDerived(int a) : base(a) {
        
    }
}
}

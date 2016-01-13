using System;
using UnityEngine;

namespace Assets {
public class Test {
    public static int a;
    public static int b;
    
    public static void VoidTryFinally() {
        try {
           
        } finally {
            
        }
    }

    public static int IntTryFinally() {
        try {
            return 0;
        } finally {

        }
    }

    public static int LocalVariable() {
        int local = 0;
        return local;
    }

    public static int Int() {
        return 0;
    }


    public static int LoggedIntMethod() {
        try {
            OnEnter();
            return 0;
        } finally {
            OnExit();
        }
    }

    public static int LoggedIntBranchMethod() {
        try {
            OnEnter();
            if (a == b) {
                return 1;
            } else {
                return 2;
            }
        } finally {
            OnExit();
        }
    }

    public static void LoggedVoidMethod() {
        try {
            OnEnter();
            a = 10;
        } finally {
            OnExit();
        }
    }

    public static void OnEnter() {
        
    }

    public static void OnExit() {
        
    }
}
}

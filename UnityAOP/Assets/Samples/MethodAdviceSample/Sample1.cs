using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Aspect.MethodAdvice;

namespace Assets.Samples.MethodAdviceSample {
    public class TargetClass {
        private int v1, v2;

        public void VoidMethod() {
            return;
        }

        public void OverloadedMethod() {
            return;
        }

        public void OverloadedMethod(int a) {
            if (a == v1) return;
            if (a == v2) return;

            for (int i = 0; i < 10; ++i) {
                if (v2 == i) return;
            }

            UnityEngine.Debug.Log("Go");
        }

        public int IntValueMethod() {
            if (v1 == v2) return 11;

            return 10;
        }

        public static void StaticVoidMethod(int par1, int par2) {
            return;
        }
    }

    public class AdviceClass {
        [MethodAdvice(typeof(TargetClass), "VoidMethod", MethodAdvicePhase.OnEnter, true)]
        public static void OnEnterVoidMethod(TargetClass self) {
            
        }

        [MethodAdvice(typeof(TargetClass), "VoidMethod", MethodAdvicePhase.OnSuccess, true)]
        public static void OnSuccessVoidMethod(TargetClass self) {

        }


        [MethodAdvice(typeof(TargetClass), "OverloadedMethod", MethodAdvicePhase.OnEnter, true)]
        public static void OnEnterOverloadedMethod(TargetClass self) {

        }

        [MethodAdvice(typeof(TargetClass), "OverloadedMethod", MethodAdvicePhase.OnSuccess, true)]
        public static void OnSuccessOverloadedMethod(TargetClass self) {

        }

        [MethodAdvice(typeof(TargetClass), "OverloadedMethod", MethodAdvicePhase.OnEnter, true)]
        public static void OnEnterOverloadedMethod(TargetClass self, int a) {

        }

        [MethodAdvice(typeof(TargetClass), "OverloadedMethod", MethodAdvicePhase.OnSuccess, true)]
        public static void OnSuccessOverloadedMethod(TargetClass self, int a) {

        }

        [MethodAdvice(typeof(TargetClass), "IntValueMethod", MethodAdvicePhase.OnEnter, true)]
        public static void OnEnterIntValueMethod(TargetClass self) {

        }

        [MethodAdvice(typeof(TargetClass), "IntValueMethod", MethodAdvicePhase.OnSuccess, true)]
        public static void OnSuccessIntValueMethod(TargetClass self, int returnValue) {

        }

        [MethodAdvice(typeof(TargetClass), "StaticVoidMethod", MethodAdvicePhase.OnEnter, true)]
        public static void OnEnterStaticVoidMethod(int par1, int par2) {

        }

        [MethodAdvice(typeof(TargetClass), "StaticVoidMethod", MethodAdvicePhase.OnSuccess, true)]
        public static void OnSuccessStaticVoidMethod(int par1, int par2) {

        }
    }
}

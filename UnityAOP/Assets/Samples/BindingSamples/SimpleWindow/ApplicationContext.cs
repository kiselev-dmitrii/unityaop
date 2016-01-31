using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Binding.Core;
using UnityEngine;

namespace Assets.Samples.BindingSamples.SimpleWindow {
    public class ApplicationContext : MonoBehaviour {
        public BindingContext RootContext;

        public void Awake() {
            RootContext.SetModel(RootContext);
        }
    }
}

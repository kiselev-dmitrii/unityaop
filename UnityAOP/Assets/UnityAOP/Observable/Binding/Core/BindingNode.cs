using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.UnityAOP.Observable.Binding.Core {
    /// <summary>
    /// Непосредственная нода, которая байндит некоторое вью со значениями из модели
    /// </summary>
    public abstract class BindingNode : MonoBehaviour {
        public abstract void Bind();
        public abstract void Unbind();
    }
}

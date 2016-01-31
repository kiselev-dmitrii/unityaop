using UnityEngine;

namespace Assets.UnityAOP.Binding.Core {
    /// <summary>
    /// Непосредственная нода, которая байндит некоторое вью со значениями из модели
    /// </summary>
    public abstract class BindingNode : MonoBehaviour {
        public BindingContext Context;

        public abstract void Bind();
        public abstract void Unbind();
    }
}

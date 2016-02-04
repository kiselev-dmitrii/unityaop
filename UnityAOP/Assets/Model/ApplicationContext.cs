using Assets.UnityAOP.Binding.Core;
using UnityEngine;

namespace Assets.Model {
    public class ApplicationContext : MonoBehaviour {
        public BindingContext RootContext;
        public Application Application;

        public void Start() {
            Application = new Application();
            RootContext.SetModel(Application);
        }
    }
}

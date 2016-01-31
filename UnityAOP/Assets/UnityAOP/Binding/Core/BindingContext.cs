using Assets.UnityAOP.Observable.Core;
using Assets.UnityAOP.Utils;
using UnityEngine;

namespace Assets.UnityAOP.Binding.Core {
    /// <summary>
    /// Определяет контекст, относительно которого должна осуществляться привзяка.
    /// Должен быть в корне префабов которые являются самостоятельными элементами, или префабами на которые 
    /// ссылается ExternalLink
    /// </summary>
    public class BindingContext : MonoBehaviour {
        public SerializableType Type;
        public IObservable Model { get; private set; }

        public void SetModel(object model) {
            BindingNode[] nodes = GetComponentsInChildren<BindingNode>();

            foreach (var node in nodes) {
                node.Unbind();
            }

            Model = (IObservable)model;

            foreach (var node in nodes) {
                node.Bind();
            }
        }
    }
}

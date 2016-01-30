using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Observable.Core;
using Assets.UnityAOP.Utils;
using UnityEngine;

namespace Assets.UnityAOP.Observable.Binding.Core {
    /// <summary>
    /// Определяет контекст, относительно которого должна осуществляться привзяка.
    /// Должен быть в корне префабов которые являются самостоятельными элементами, или префабами на которые 
    /// ссылается ExternalLink
    /// </summary>
    public class BindingContext : MonoBehaviour {
        public SerializableType Type;
        public IObservable Model { get; private set; }

        public void SetModel(IObservable model) {
            BindingNode[] nodes = GetComponentsInChildren<BindingNode>();

            foreach (var node in nodes) {
                node.Unbind();
            }

            Model = model;

            foreach (var node in nodes) {
                node.Bind();
            }
        }
    }
}

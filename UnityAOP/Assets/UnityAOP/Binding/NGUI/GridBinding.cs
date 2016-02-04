using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Binding.Core;
using Assets.UnityAOP.Observable.ChainedObservers;
using UnityEngine;

namespace Assets.UnityAOP.Binding.NGUI {
    public class GridBinding : BindingNode {
        public BindingPath Path;
        public BindingContext Template;
        private UntypedListObserver listObserver;

        private UIGrid grid;

        private void Awake() {
            grid = GetComponent<UIGrid>();
        }

        public override void Bind() {
            var root = Context.Model;
            listObserver = root.ObserveList(Path.ToArray());

            listObserver.OnInserted += OnInserted;
            listObserver.OnRemoved += OnRemoved;
            listObserver.OnCleared += OnCleared;
        }

        public override void Unbind() {
            if (listObserver != null) {
                listObserver.Dispose();
            }
        }

        private void OnCleared() {
            while (transform.childCount > 0) {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            grid.Reposition();
        }

        private void OnInserted(int index, object item) {
            // Переименовываем трансформации в гриде, чтобы их имена совпадали с их индексами
            for (var i = 0; i < transform.childCount; ++i) {
                var child = transform.GetChild(i);
                int childIndex;
                if (int.TryParse(child.name, out childIndex) && childIndex >= index) {
                    child.name = String.Format("{0}", childIndex + 1);
                }
            }

            // Вставляем новый элемент
            BindingContext context = Instantiate(Template);
            context.name = String.Format("{0}", index);
            context.transform.parent = transform;
            context.transform.localPosition = Vector3.zero;
            context.transform.localScale = Vector3.one;
            context.transform.localEulerAngles = Vector3.zero;

            //Сетим модель в элемент
            context.SetModel(item);

            grid.Reposition();
        }

        private void OnRemoved(int index, object item) {
            // Удаляем элемент по индексу. 
            // Элементы с большим индексом сдвигаем
            for (var i = 0; i < transform.childCount; ++i) {
                var child = transform.GetChild(i);

                int childIndex;
                if (int.TryParse(child.name, out childIndex)) {
                    if (childIndex > index) {
                        child.name = String.Format("{0}", childIndex - 1);
                    }
                    else if (childIndex == index) {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }

            grid.Reposition();
        }
    }
}

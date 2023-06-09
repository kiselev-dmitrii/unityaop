﻿using System;
using Assets.UnityAOP.Observable.CodeObjectModel;
using Assets.UnityAOP.Observable.Core;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Observable.ChainedObservers {
    public abstract class BaseChainedObserver : IObserver, IDisposable {
        private readonly PropertyMetadata[] props;
        private readonly IObservable[] refs;

        public PropertyMetadata TargetProperty {
            get { return props[props.Length - 1]; }
        }

        protected BaseChainedObserver(IObservable root, PropertyMetadata[] propertyPath) {
            props = propertyPath;

            refs = new IObservable[propertyPath.Length];
            refs[0] = root;
        }

        protected void Bind(int position) {
            for (int i = position; i < refs.Length; ++i) {
                IObservable cur = refs[i];
                if (cur == null) break;

                PropertyMetadata prop = props[i];
                cur.AddMemberObserver(prop.Code, this);

                if (i != props.Length - 1) {
                    var getter = (GetterDelegate<IObservable>)cur.GetGetterDelegate(prop.Code);
                    refs[i + 1] = getter();
                } else {
                    BindTarget(cur, prop);
                }
            }
        }

        protected void Unbind(int position) {
            for (int i = position; i < refs.Length; ++i) {
                IObservable cur = refs[i];
                if (cur != null) {
                    PropertyMetadata prop = props[i];
                    cur.RemoveMemberObserver(prop.Code, this);

                    refs[i] = null;
                }
            }

            UnbindTarget();
        }

        public void OnNodeChanged(IObservable parent, int propertyCode) {
            if (parent == refs[refs.Length - 1]) {
                OnParentNodeChanged();

            } else {
                int parentIndex = Array.IndexOf(refs, parent);
                Assert.IsTrue(parentIndex >= 0, "Не найдена ссылка родителя");

                //Отписываемся от старого объекта
                Unbind(parentIndex + 1);

                //Обновляем следущую ссылку
                var prop = props[parentIndex];
                var getter = (GetterDelegate<IObservable>)parent.GetGetterDelegate(prop.Code);
                refs[parentIndex + 1] = getter();

                //Подписываемся
                Bind(parentIndex + 1);

                OnParentNodeChanged();
            }
        }

        public virtual void Dispose() {
            Unbind(0);
        }

        protected abstract void BindTarget(IObservable parent, PropertyMetadata targetMeta);
        protected abstract void UnbindTarget();
        protected abstract void OnParentNodeChanged();
    }
}

using System;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Observable.ChainedObservers {
public class ChainedPropertyObserver<T> : IObserver, IDisposable {
    private readonly PropertyMetadata[] props;
    private readonly IObservable[] refs;

    private GetterDelegate<T> valueGetter;
    private SetterDelegate<T> valueSetter;

    private Action callback;

    public ChainedPropertyObserver(IObservable root, PropertyMetadata[] propertyPath, Action onValueChanged) {
        props = propertyPath;

        refs = new IObservable[propertyPath.Length];
        refs[0] = root;

        callback = onValueChanged;

        Bind(0);
    }

    public T GetValue() {
        if (valueGetter != null) {
            return valueGetter();
        } else {
            return default(T);
        }
    }

    public void SetValue(T value) {
        if (valueSetter != null) {
            valueSetter(value);
        }   
    }

    private void Bind(int position) {
        for (int i = position; i < refs.Length; ++i) {
            IObservable cur = refs[i];
            if (cur == null) break;

            PropertyMetadata prop = props[i];
            cur.AddObserver(prop.Index, this);

            if (i != props.Length - 1) {
                var getter = (GetterDelegate<IObservable>) cur.GetGetterDelegate(prop.Index);
                refs[i + 1] = getter();
            } else {
                valueGetter = (GetterDelegate<T>) cur.GetGetterDelegate(prop.Index);
                valueSetter = (SetterDelegate<T>) cur.GetSetterDelegate(prop.Index);
            }
        }
    }

    private void Unbind(int position) {
        for (int i = position; i < refs.Length; ++i) {
            IObservable cur = refs[i];
            if (cur != null) {
                PropertyMetadata prop = props[i];
                cur.RemoveObserver(prop.Index, this);

                refs[i] = null;
            }
        }
        
        valueGetter = null;
        valueSetter = null;
    }

    public void OnNodeChanged(IObservable parent, int index) {
        if (parent == refs[refs.Length - 1]) {
            OnValueChanged();
        } else {
            int parentIndex = Array.IndexOf(refs, parent);
            Assert.IsTrue(parentIndex >= 0, "Не найдена ссылка родителя");

            //Отписываемся от старого объекта
            Unbind(parentIndex + 1);

            //Обновляем следущую ссылку
            var prop = props[parentIndex];
            var getter = (GetterDelegate<IObservable>) parent.GetGetterDelegate(prop.Index);
            refs[parentIndex + 1] = getter();

            //Подписываемся
            Bind(parentIndex + 1);

            OnValueChanged();
        }
    }

    private void OnValueChanged() {
        callback();
    }

    public void Dispose() {
        Unbind(0);
        callback = null;
    }


}
}

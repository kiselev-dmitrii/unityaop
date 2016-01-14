using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Observable.ChainedObservers {
public class ChainedPropertyObserver<T> : IObserver, IDisposable {
    private readonly PropertyMetadata[] props;
    private readonly IObservable[] refs;
    private Action onValueChanged;

    public ChainedPropertyObserver(IObservable root, PropertyMetadata[] propertyPath, Action onValueChanged) {
        props = propertyPath;

        refs = new IObservable[propertyPath.Length];
        refs[0] = root;
        
        this.onValueChanged = onValueChanged;
    }

    public void Bind(int position = 0) {
        for (int i = position; i < props.Length; ++i) {
            IObservable cur = refs[i];
            PropertyMetadata prop = props[i];
            cur.AddObserver(prop.Index, this);

            //IObservable next = cur.Get
        }
    } 

    public void Dispose() {
        throw new NotImplementedException();
    }

    public void OnNodeChanged(object parent, int index) {
        throw new NotImplementedException();
    }
}
}

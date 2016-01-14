using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.UnityAOP.Observable.ChainedObservers {
public class ChainedPropertyObserver<T> : IDisposable {
    public ChainedPropertyObserver(String[] path) {
        
    }   

    public void Dispose() {
        throw new NotImplementedException();
    }
}
}

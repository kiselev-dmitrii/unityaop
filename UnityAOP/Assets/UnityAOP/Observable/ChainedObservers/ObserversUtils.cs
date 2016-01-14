using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Observable.ChainedObservers {
public static class ObserverUtils {
    public static ChainedPropertyObserver<TTarget> Observe<TTarget>(this object root, String path, Action onChanged) {
        var props = CalculatePropertyPath(root.GetType(), path);
        return new ChainedPropertyObserver<TTarget>(root, props, onChanged);
    }

    public static ChainedPropertyObserver<TTarget> Observe<TRoot, TTarget>(this TRoot root,
        Expression<Func<TRoot, TTarget>> expression, Action onChanged) {

        String path = expression.ToString().Replace("get_Item(", "").Replace(")", "");
        String[] fieldsName = path.Split('.').Skip(1).ToArray();
        var fullPath = GetPath(root.GetType(), fieldsName);
        return new ChainedPropertyObserver<TTarget>(root, fullPath, onChanged);
    }

    private static PropertyMetadata[] CalculatePropertyPath(Type rootType, String path) {
        String[] fieldsName = path.Split('.');
        return CalculatePropertyPath(rootType, fieldsName);
    }

    private static PropertyMetadata[] CalculatePropertyPath(Type rootType, String[] path) {
        PropertyMetadata[] props = new PropertyMetadata[path.Length];

        TypeMetadata typeMeta = ObservableMetadata.GetTypeMetadata(rootType);
        for (int i = 0; i < path.Length; ++i) {
            var propName = path[i];
            var propMeta = typeMeta.GetPropertyMetadata(propName);

            Assert.IsTrue(propMeta != null, "Неправильный путь, возможно");

            props[i] = propMeta;

            if (i != path.Length - 1) {
                typeMeta = ObservableMetadata.GetTypeMetadata(propMeta.Type);
            }
        }

        return props;
    }
}
}

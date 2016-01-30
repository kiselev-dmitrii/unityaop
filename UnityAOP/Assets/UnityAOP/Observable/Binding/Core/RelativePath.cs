using System;
using Assets.UnityAOP.Utils;
using UnityEngine;

namespace Assets.UnityAOP.Observable.Binding.Core {
    /// <summary>
    /// Задает относительный путь.
    /// Путь определяется типом корня BindingContext.
    /// При указании пути определяется ее тип - тип последнего узла в пути.
    /// Путь может состоять из нескольких RelativePath
    /// </summary>
    public class RelativePath : MonoBehaviour {
        public String Path;
        public SerializableType Type;
        public RelativePath Parent;

        private String cachedFullPath;
    }
}

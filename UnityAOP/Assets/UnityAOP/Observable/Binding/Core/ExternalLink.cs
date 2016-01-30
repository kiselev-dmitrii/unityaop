using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Utils;
using UnityEngine;

namespace Assets.UnityAOP.Observable.Binding.Core {
    /// <summary>
    /// Штука которая подгружает внешний префаб и связывает его с моделью которая находится по пути Path
    /// Результирующий тип линка должен соостветствовать с типом префаба
    /// </summary>
    public class ExternalLink : MonoBehaviour {
        public BindingContext Prefab;
        public String Path;
        public SerializableType Type;
    }
}

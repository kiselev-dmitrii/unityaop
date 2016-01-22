using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.UnityAOP.Editor.Injectors {
    class InjectionException : Exception {
        public InjectionException(String message) : base(message) {
        }

        public InjectionException(String format, params object[] objects) : base(String.Format(format, objects)) {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Assets.UnityAOP.Editor.CodeProcessors {
public interface IMethodProcessor {
    void ProcessMethod();
}
}

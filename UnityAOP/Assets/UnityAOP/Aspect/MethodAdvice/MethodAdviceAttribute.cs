using System;

namespace Assets.UnityAOP.Aspect.MethodAdvice {
    /// <summary>
    /// Аттрибут позволяет инжектить метод до или после выполнения. 
    /// 
    /// Инжектируемый метод должен быть статическим.
    /// В случае, если перехватываемый метод не является статическим, то первым параметром идет ссылка на экземпляр класса.
    /// Далее должны идти входные параметры таргетного метода с теми же типами.
    /// Если таргетный метод имеет возвращаемое значение, то результат передается последним параметром.
    /// 
    /// В случае, если enableForChild = true, то аттрибут наследуется. Для абстрактных и интерфейсных методов он не имеет смысла и всегда включен.
    /// 
    /// MethodAdvicePhase.OnEnter вызывает метод перед выполнением таргетного метода.
    /// MethodAdvicePhase.OnSuccess вызывает метод только в случае успешного завершения таргетного метода
    /// 
    /// Пример:
    /// public class Foo {
    ///     public void Method1(int a, int b);
    ///     public int Method2(int a, int b, int c);
    /// }
    /// ....
    /// 
    /// [MethodAdvice(typeof(Foo), "Method1", MethodAdvicePhase.OnEnter)]
    /// public static OnEnterMethod1(Foo self, int a, int b) {
    /// }
    /// 
    /// [MethodAdvice(typeof(Foo), "Method2", MethodAdvicePhase.OnSuccess)]
    /// public static OnSuccessMethod2(Foo self, int a, int b, int returnValue) {
    /// 
    /// }
    /// </summary>

    public enum MethodAdvicePhase {
        OnEnter,
        OnSuccess
    }

    public class MethodAdviceAttribute : Attribute {
        public Type TargetType { get; private set; }
        public String TargetMethod { get; private set; }
        public MethodAdvicePhase AdvicePhase { get; private set; }
        public bool EnableForChild { get; private set; }
    
        public MethodAdviceAttribute(Type targetType, String targetMethod, MethodAdvicePhase advicePhase, bool enableForChild) {
            TargetType = targetType;
            TargetMethod = targetMethod;
            AdvicePhase = advicePhase;
            EnableForChild = enableForChild;
        }
    }
}

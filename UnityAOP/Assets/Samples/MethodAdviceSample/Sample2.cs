using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Aspect.MethodAdvice;

namespace Assets.Samples.MethodAdviceSample {
    public interface IEntityService {
        Entity FindEntity(String entityName);
    }

    public class Entity {
        public String Name;
    }

    public class DictionaryImplementation : IEntityService {
        private Dictionary<String, Entity> dictionary;

        public DictionaryImplementation() {
            dictionary = new Dictionary<string, Entity>();
        }

        public Entity FindEntity(string entityName) {
            return dictionary[entityName];
        }
    }

    public class ListImplementation : IEntityService {
        private List<Entity> list;

        public ListImplementation() {
            list = new List<Entity>();
        }

        public Entity FindEntity(string entityName) {
            return list.FirstOrDefault(x => x.Name == entityName);
        }
    }

    public class EntityServiceAdvices {
        [MethodAdvice(typeof(IEntityService), "FindEntity", MethodAdvicePhase.OnEnter)]
        public static void OnEnterFindEntity(IEntityService self, String entityName) {
            
        }

        [MethodAdvice(typeof(IEntityService), "FindEntity", MethodAdvicePhase.OnSuccess)]
        public static void OnSuccessFindEntity(IEntityService self, String entityName, Entity returnValue) {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Utils;
using Mono.Cecil;

namespace Assets.UnityAOP.Editor.Injectors {
    public class TypeNode {
        public TypeDefinition Type;
        public List<TypeNode> Interfaces; 
        public TypeNode Base;
        public List<TypeNode> Derived;

        public TypeNode(TypeDefinition type) {
            Type = type;
            Interfaces = new List<TypeNode>();
            Base = null;
            Derived = new List<TypeNode>();
        }
    }

    public class TypeTree {
        private Dictionary<TypeDefinition, TypeNode> nodes;

        public TypeTree(ModuleDefinition module) {
            nodes = new Dictionary<TypeDefinition, TypeNode>();

            foreach (var type in module.Types) {
                nodes[type] = new TypeNode(type);
            }

            foreach (var pair in nodes) {
                var node = pair.Value;
                var type = node.Type;

                foreach (TypeReference interfaceRef in type.Interfaces) {
                    TypeDefinition interfaceDef = interfaceRef.Resolve();
                    if (interfaceDef != null) {
                        var interfaceNode = nodes.Get(interfaceDef);
                        if (interfaceNode != null) {
                            node.Interfaces.Add(interfaceNode);
                            interfaceNode.Derived.Add(node); 
                        }
                    }
                }

                if (type.BaseType != null) {
                    TypeDefinition baseTypeDef = type.BaseType.Resolve();
                    if (baseTypeDef != null) {
                        var baseNode = nodes.Get(baseTypeDef);
                        if (baseNode != null) {
                            node.Base = baseNode;
                            baseNode.Derived.Add(node);
                        }
                    }
                }
            }
        }

        public TypeNode GetNode(TypeDefinition type) {
            return nodes.Get(type);
        }
    }
}

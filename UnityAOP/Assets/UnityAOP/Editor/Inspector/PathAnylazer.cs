using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.UnityAOP.Observable;
using UnityEngine.Assertions;

namespace Assets.UnityAOP.Editor.Inspector {
    public class PathAnylazer {
        public enum TokenType {
            Separator,
            Property,
            Index
        }

        public class Token {
            public TokenType Type;
            public String Property;
            public int Index;

            public Token(TokenType type) {
                Type = type;
            }

            public Token(int index) {
                Type = TokenType.Index;
                Index = index;
            }

            public Token(String property) {
                Type = TokenType.Property;
                Property = property;
            }

            public override string ToString() {
                switch (Type) {
                    case TokenType.Separator:
                        return ".";
                        break;
                    case TokenType.Property:
                        return Property;
                        break;
                    case TokenType.Index:
                        return Index.ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public enum SymbolType {
            Property,
            CollectionProperty,
            Index,
            Separator,
        }

        public class ResolvedSymbol {
            public SymbolType SymType;
            public Type ValueType;
            public Type GenericParameter;
            public String Name;
            public int Index;
        }

        public class InvalidPathException : Exception {
            public InvalidPathException(String message) : base(message) { }    
        }

        public Type RootType { get; private set; }
        public List<ResolvedSymbol> Resolved { get; private set; }
        public String Unresolved { get; private set; }
        public List<PropertyMetadata> Variants { get; private set; }
        public String Message { get; private set; }

        public int NumVariants {
            get { return Variants != null ? Variants.Count : 0; }
        }

        public bool IsValid {
            get { return String.IsNullOrEmpty(Message); }
        }

        public Type ResolvedType {
            get {
                if (!IsValid) return null;
                if (Resolved.Count == 0) return RootType;
                return Resolved[Resolved.Count - 1].ValueType;
            }
        }

        public PathAnylazer() {
            Resolved = new List<ResolvedSymbol>();
        }

        public void SetType(Type rootType) {
            RootType = rootType;
        }

        public bool Anylize(String path) {
            List<Token> tokens = ParseTokens(path).ToList();

            String warningMessage;
            var resolvedSymbols = ParseSymbols(RootType, tokens, out warningMessage);
            Message = warningMessage;

            Resolved = resolvedSymbols;

            var unresolvedTokens = tokens.Skip(resolvedSymbols.Count);

            Unresolved = String.Join("", unresolvedTokens.Select(x => x.ToString()).ToArray());
            Variants = CalculateVariants(RootType, tokens, resolvedSymbols, Unresolved);

            return IsValid;
        }

        public static IEnumerable<Token> ParseTokens(String path) {
            var tokens = path.Split('.');
            for (int i = 0; i < tokens.Length; i++) {
                var token = tokens[i];

                int integer = 0;
                if (int.TryParse(token, out integer)) {
                    yield return new Token(integer);
                } else {
                    yield return new Token(token);
                }

                if (i != tokens.Length - 1) {
                    yield return new Token(TokenType.Separator);
                }
            }
        }

        public static List<ResolvedSymbol> ParseSymbols(Type rootType, List<Token> tokens, out String warning) {
            List<ResolvedSymbol> result = new List<ResolvedSymbol>();

            warning = null;
            TypeMetadata lastType = ObservableMetadata.GetTypeMetadata(rootType);
            ResolvedSymbol lastValueSymbol = null;

            for (int i = 0; i < tokens.Count; i++) {
                var token = tokens[i];

                if (token.Type == TokenType.Property) {
                    var propMeta = lastType.GetPropertyMetadata(token.Property);
                    if (propMeta == null) {
                        warning = "Cannot find property " + token.Property;
                        break;
                    }

                    var symbol = new ResolvedSymbol() {
                        SymType = propMeta.IsCollection ? SymbolType.CollectionProperty : SymbolType.Property,
                        ValueType = propMeta.Type,
                        Name = propMeta.Name,
                        GenericParameter = propMeta.IsCollection ? propMeta.ItemType : null
                    };
                    result.Add(symbol);

                    lastValueSymbol = symbol;

                    if (i != tokens.Count - 1) {
                        if (symbol.SymType != SymbolType.CollectionProperty) {
                            lastType = ObservableMetadata.GetTypeMetadata(symbol.ValueType);
                            if (lastType == null) {
                                warning = symbol.Name + " is not observable";
                                break;
                            }
                        }
                    }

                } else if (token.Type == TokenType.Index) {
                    if (lastValueSymbol.SymType != SymbolType.CollectionProperty) {
                        warning = lastValueSymbol.Name + " is not collection";
                        break;
                    }

                    var symbol = new ResolvedSymbol() {
                        SymType = SymbolType.Index,
                        ValueType = lastValueSymbol.GenericParameter,
                        Index = token.Index
                    };

                    result.Add(symbol);

                    lastValueSymbol = symbol;
                    lastType = ObservableMetadata.GetTypeMetadata(symbol.ValueType);

                } else if (token.Type == TokenType.Separator) {

                    result.Add(new ResolvedSymbol() {
                        SymType = SymbolType.Separator
                    });
                }
            }

            return result;
        }

        public static List<PropertyMetadata> CalculateVariants(Type rootType, List<Token> tokens, List<ResolvedSymbol> resolved, String unresolved) {
            var result = new List<PropertyMetadata>();

            var lastValueSymbol = resolved.LastOrDefault(x => x.ValueType != null);

            //Ничего существенного не написано - ориентируемся на RootType
            if (lastValueSymbol == null) {
                var meta = ObservableMetadata.GetTypeMetadata(rootType);
                return meta.Properties.Values.Where(x => x.Name.Contains(unresolved)).ToList();
            } 
            
            // Есть последний значимый символ
            var lastResolvedSymbol = resolved.LastOrDefault();
            if (lastResolvedSymbol.SymType == SymbolType.Separator) {
                if (lastValueSymbol.SymType == SymbolType.CollectionProperty) {
                    result = new List<PropertyMetadata>() {
                        new PropertyMetadata("index", 0, lastValueSymbol.GenericParameter)
                    };
                } else {
                    var meta = ObservableMetadata.GetTypeMetadata(lastValueSymbol.ValueType);
                    result = meta.Properties.Values.Where(x => x.Name.Contains(unresolved)).ToList();
                }
                
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.UnityAOP.Aspect.PartialAdvice;
using UnityEngine;

namespace Assets {
    public class Entity {
        public String Name { get; private set; }

        public Entity(String name) {
            Name = name;
        }
    }

    public partial class EntityController {
        public String Path { get; private set; }
        private Dictionary<String, Entity> entities; 

        public EntityController(String path) {
            entities = new Dictionary<string, Entity>();
            Path = path;
        }

        public EntityController() {
            entities = new Dictionary<string, Entity>();
            Path = "/default/path";
        }

        public void AddEntity(Entity entity) {
            entities[entity.Name] = entity;
        }

        public Entity FindEntity(String name) {
            return entities[name];
        }

        public static Entity Create() {
            return new Entity("Empty");
        }
    }

    public partial class EntityController {
        [BeforeConstructor]
        public void BeforeEntityController(String path) {
            Debug.Log(path);
        }

        [AfterConstructor]
        public void AfterEntityController(String path) {
            Debug.Log(path);
        }

        [BeforeConstructor]
        public void BeforeEntityController() {
            Debug.Log("Before default constructor");
        }

        [AfterConstructor]
        public void AfterEntityController() {
            Debug.Log("After default constructor");
        }

        [BeforeMethod("AddEntity")]
        public void BeforeAddEntity(Entity entity) {
            Debug.Log(entity);
        }

        [AfterMethod("AddEntity")]
        public void AfterAddEntity(Entity entity) {
            Debug.Log(entity);
        }

        [BeforeMethod("FindEntity")]
        public void BeforeFindEntity(String name) {
            Debug.Log("Find Entity " + name);
        }

        [AfterMethod("FindEntity")]
        public void AfterFindEntity(String name, Entity returnValue) {
            Debug.Log("Find entity " + name + " " + returnValue);
        }

        [BeforeMethod("Create")]
        public static void BeforeCreate() {
            Debug.Log("Before create");
        }

        [AfterMethod("Create")]
        public static void AfterCreate(Entity returnValue) {
            Debug.Log("After create");
        }
    }
}

using Assets.UnityAOP.Attributes;

namespace Assets.Model {
    [Observable]
    public class Application {
        public User Player { get; set; }

        public Application() {
            Player = new User(1, "Player", "http://vk.com/id0/avatar.jpg");
        }
    }
}

using System.Collections.Generic;

public static class ActorRegistry
{
    public static List<Actor> Actors { get;  set; } = new List<Actor>();
    public static PlayerActor PlayerActor { get;  set; }

    public static void RegisterActor(Actor actor)
    {
        Actors.Add(actor);
    }

    public static void UnregisterActor(Actor actor)
    {
        Actors.Remove(actor);
    }
}